import 'package:app_frontend/API/signal_r_client.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Routes/routes.dart';
import 'package:flutter/widgets.dart';

/// Centralized recovery flow when the current session is no longer valid.
///
/// Keeps logout + navigation logic in one place so every layer
/// (REST interceptor, SignalR, etc.) behaves consistently.
class SessionExpirationRecovery {
  SessionExpirationRecovery._();

  static final SessionExpirationRecovery instance =
      SessionExpirationRecovery._();

  SessionManager? _sessionManager;
  SignalRClient? _signalRClient;
  GlobalKey<NavigatorState>? _navigatorKey;
  bool _isRecovering = false;

  void configure({
    required SessionManager sessionManager,
    required SignalRClient signalRClient,
    required GlobalKey<NavigatorState> navigatorKey,
  }) {
    // Dependencies are injected once at app startup.
    _sessionManager = sessionManager;
    _signalRClient = signalRClient;
    _navigatorKey = navigatorKey;
  }

  Future<void> recover() async {
    // Guard against parallel recoveries when many requests fail together.
    if (_isRecovering) return;

    _isRecovering = true;
    try {
      // Order matters: disconnect realtime first, then clear local session.
      await _signalRClient?.disconnect(disposeConnection: true);
      await _sessionManager?.clearSession();

      final navigator = _navigatorKey?.currentState;
      if (navigator != null) {
        // Reset stack to avoid navigating back into protected screens.
        navigator.pushNamedAndRemoveUntil(Routes.login, (route) => false);
      }
    } finally {
      _isRecovering = false;
    }
  }
}
