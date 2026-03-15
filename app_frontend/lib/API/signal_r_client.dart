import 'dart:async';
import 'dart:convert';

import 'package:app_frontend/API/DTOs/auth_dtos.dart';
import 'package:app_frontend/API/api_client.dart';
import 'package:app_frontend/Auth/session_expiration_recovery.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Auth/token_refresh_coordinator.dart';
import 'package:app_frontend/config.dart';
import 'package:flutter/foundation.dart';
import 'package:signalr_netcore/iretry_policy.dart';
import 'package:signalr_netcore/signalr_client.dart';

enum SignalRConnectionStatus {
  disconnected,
  connecting,
  connected,
  reconnecting,
}

enum SignalRHubMethod {
  productAdded,
  productUpdated,
  productDeleted,
  listCreated,
  listArchived,
}

extension SignalRHubMethodName on SignalRHubMethod {
  String get value {
    switch (this) {
      case SignalRHubMethod.productAdded:
        return 'ProductAdded';
      case SignalRHubMethod.productUpdated:
        return 'ProductUpdated';
      case SignalRHubMethod.productDeleted:
        return 'ProductDeleted';
      case SignalRHubMethod.listCreated:
        return 'ListCreated';
      case SignalRHubMethod.listArchived:
        return 'ListArchived';
    }
  }
}

class SignalRHubEvent {
  final SignalRHubMethod method;
  final List<Object?> arguments;

  const SignalRHubEvent({required this.method, required this.arguments});

  Object? get payload => arguments.isNotEmpty ? arguments.first : null;
}

class _FixedRetryPolicy implements IRetryPolicy {
  final int delayInMilliseconds;

  _FixedRetryPolicy({required this.delayInMilliseconds});

  @override
  int? nextRetryDelayInMilliseconds(RetryContext retryContext) {
    return delayInMilliseconds;
  }
}

/// Shared SignalR provider.
///
/// Exposes connection state + hub events while keeping reconnection
/// and token refresh handling internal.
class SignalRClient extends ChangeNotifier {
  final SessionManager _sessionManager;
  final ApiClient _apiClient;

  final StreamController<SignalRHubEvent> _eventsController =
      StreamController<SignalRHubEvent>.broadcast();

  HubConnection? _connection;
  StreamSubscription<HubConnectionState>? _stateSubscription;

  SignalRConnectionStatus _status = SignalRConnectionStatus.disconnected;
  String? _lastError;

  SignalRClient({
    required SessionManager sessionManager,
    required ApiClient apiClient,
  })  : _sessionManager = sessionManager,
        _apiClient = apiClient;

  SignalRConnectionStatus get status => _status;
  bool get isConnected => _status == SignalRConnectionStatus.connected;
  bool get isConnecting => _status == SignalRConnectionStatus.connecting;
  bool get isReconnecting => _status == SignalRConnectionStatus.reconnecting;
  String? get lastError => _lastError;
  Stream<SignalRHubEvent> get events => _eventsController.stream;

  Future<bool> connect() async {
    // Idempotent connect to avoid duplicate starts from multiple screens.
    if (isConnected || isConnecting || isReconnecting) {
      return true;
    }

    final familyId = await _sessionManager.getFamilyId();
    // Users without family should not connect to the family-scoped hub.
    if (familyId == null) {
      _setStatus(SignalRConnectionStatus.disconnected);
      _lastError = null;
      notifyListeners();
      return false;
    }

    _setStatus(SignalRConnectionStatus.connecting);

    await _ensureConnectionCreated();

    try {
      await _connection!.start();
      _lastError = null;
      _setStatus(SignalRConnectionStatus.connected);
      return true;
    } catch (e) {
      _lastError = 'No fue posible conectar con el hub en tiempo real.';
      _setStatus(SignalRConnectionStatus.disconnected);
      return false;
    }
  }

  Future<void> disconnect({bool disposeConnection = false}) async {
    final connection = _connection;

    if (connection == null) {
      _setStatus(SignalRConnectionStatus.disconnected);
      return;
    }

    try {
      for (final method in SignalRHubMethod.values) {
        connection.off(method.value);
      }
      await connection.stop();
    } catch (_) {
    } finally {
      _setStatus(SignalRConnectionStatus.disconnected);

      if (disposeConnection) {
        await _stateSubscription?.cancel();
        _stateSubscription = null;
        _connection = null;
      }
    }
  }

  Future<Object?> invoke(String methodName, {List<Object>? args}) async {
    final connection = _connection;
    if (connection == null || !isConnected) {
      throw StateError('SignalR no está conectado.');
    }

    return connection.invoke(methodName, args: args);
  }

  Future<void> send(String methodName, {List<Object>? args}) async {
    final connection = _connection;
    if (connection == null || !isConnected) {
      throw StateError('SignalR no está conectado.');
    }

    await connection.send(methodName, args: args);
  }

  Future<void> _ensureConnectionCreated() async {
    if (_connection != null) return;

    final hubUrl = _buildHubUrl(Config.apiURL);

    final connection = HubConnectionBuilder()
        .withUrl(
          hubUrl,
          options: HttpConnectionOptions(
            accessTokenFactory: () async {
              // Token is resolved on demand for connect/reconnect attempts.
              final token = await _getValidAccessToken();
              if (token == null) {
                throw Exception('No hay token de acceso disponible.');
              }
              return token;
            },
            requestTimeout: 5000,
          ),
        )
        .withAutomaticReconnect(
          reconnectPolicy: _FixedRetryPolicy(delayInMilliseconds: 2000),
        )
        .build();

    _wireConnectionLifecycle(connection);
    _wireServerEvents(connection);

    _stateSubscription = connection.stateStream.listen((state) {
      switch (state) {
        case HubConnectionState.Connected:
          _setStatus(SignalRConnectionStatus.connected);
          break;
        case HubConnectionState.Connecting:
          _setStatus(SignalRConnectionStatus.connecting);
          break;
        case HubConnectionState.Reconnecting:
          _setStatus(SignalRConnectionStatus.reconnecting);
          break;
        case HubConnectionState.Disconnected:
        case HubConnectionState.Disconnecting:
          _setStatus(SignalRConnectionStatus.disconnected);
          break;
      }
    });

    _connection = connection;
  }

  void _wireConnectionLifecycle(HubConnection connection) {
    connection.onclose(({Exception? error}) {
      if (error != null) {
        _lastError = 'La conexión en tiempo real se cerró inesperadamente.';
      }
      _setStatus(SignalRConnectionStatus.disconnected);
    });

    connection.onreconnecting(({Exception? error}) {
      _setStatus(SignalRConnectionStatus.reconnecting);
    });

    connection.onreconnected(({String? connectionId}) {
      _lastError = null;
      _setStatus(SignalRConnectionStatus.connected);
    });
  }

  void _wireServerEvents(HubConnection connection) {
    for (final method in SignalRHubMethod.values) {
      connection.on(method.value, (arguments) {
        _eventsController.add(
          SignalRHubEvent(
            method: method,
            arguments: arguments ?? const <Object?>[],
          ),
        );
      });
    }
  }

  Future<String?> _getValidAccessToken() async {
    final authToken = await _sessionManager.getAuthToken();
    if (authToken == null) return null;

    if (_isTokenExpiredOrNearExpiry(authToken)) {
      final refreshed = await _refreshAccessToken();
      return refreshed;
    }

    return authToken;
  }

  bool _isTokenExpiredOrNearExpiry(String token) {
    try {
      final parts = token.split('.');
      if (parts.length != 3) return false;

      final normalized = base64Url.normalize(parts[1]);
      final payload = json.decode(utf8.decode(base64Url.decode(normalized)))
          as Map<String, dynamic>;
      final exp = payload['exp'];
      if (exp is! num) return false;

      final expiresAt = DateTime.fromMillisecondsSinceEpoch(
        exp.toInt() * 1000,
        isUtc: true,
      );

      return DateTime.now().toUtc().isAfter(
        expiresAt.subtract(const Duration(seconds: 30)),
      );
    } catch (_) {
      return false;
    }
  }

  Future<String?> _refreshAccessToken() async {
    try {
      // Reuses centralized refresh mutex shared with the HTTP interceptor.
      final token = await TokenRefreshCoordinator.instance.refreshAccessToken(
        session: _sessionManager,
        requester: (refreshToken) {
          return _apiClient.authRequest(
            AuthRoutes.refresh,
            RefreshDTO(refreshToken: refreshToken),
          );
        },
        clearSessionOnFailure: true,
      );

      return token;
    } on SessionExpiredException {
      // If session cannot be refreshed, force global logout flow.
      unawaited(SessionExpirationRecovery.instance.recover());
      return null;
    } catch (_) {
      return null;
    }
  }

  String _buildHubUrl(String apiUrl) {
    final apiUri = Uri.parse(apiUrl);
    final segments = apiUri.pathSegments.where((e) => e.isNotEmpty).toList();

    if (segments.isNotEmpty &&
        segments.last.toLowerCase() == 'api') {
      segments.removeLast();
    }

    final hubUri = apiUri.replace(pathSegments: [...segments, 'hubs', 'shopping']);
    return hubUri.toString();
  }

  void _setStatus(SignalRConnectionStatus status) {
    if (_status == status) return;
    _status = status;
    notifyListeners();
  }

  @override
  void dispose() {
    unawaited(disconnect(disposeConnection: true));
    _eventsController.close();
    super.dispose();
  }
}