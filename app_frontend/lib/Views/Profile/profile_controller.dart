import 'package:app_frontend/API/signal_r_client.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:flutter/widgets.dart';

/// Mediates between [ProfileView] and [SessionManager].
/// Manages user profile display and logout flow.
class ProfileController extends ChangeNotifier {
  final SessionManager _sessionManager;
  final SignalRClient _signalRClient;

  // ─── UI state ─────────────────────────────────────────────────────────────

  bool _isLoading = true;
  String? _username;
  String? _errorMessage;

  bool get isLoading => _isLoading;
  String? get username => _username;
  String? get errorMessage => _errorMessage;

  // ─── Constructor ──────────────────────────────────────────────────────────

  ProfileController({
    required SessionManager sessionManager,
    required SignalRClient signalRClient,
  })  : _sessionManager = sessionManager,
        _signalRClient = signalRClient {
    _loadUserData();
  }

  // ─── Actions ──────────────────────────────────────────────────────────────

  /// Loads the username from secure storage.
  Future<void> _loadUserData() async {
    try {
      _username = await _sessionManager.getUsername();
      _errorMessage = null;
    } catch (e) {
      _errorMessage = 'Error al cargar datos del usuario';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// Clears the session and returns `true` on success.
  /// The view handles navigation after this completes.
  Future<bool> logout() async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      await _signalRClient.disconnect(disposeConnection: true);
      await _sessionManager.clearSession();
      return true;
    } catch (e) {
      _errorMessage = 'Error al cerrar sesión';
      return false;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
}
