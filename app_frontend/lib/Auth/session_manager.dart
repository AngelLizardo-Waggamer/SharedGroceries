import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Exception thrown when the session is invalid and cannot be recovered.
class SessionExpiredException implements Exception {
  final String message;
  const SessionExpiredException(this.message);

  @override
  String toString() => 'SessionExpiredException: $message';
}

/// Manages the user session: securely stores the auth and refresh tokens.
///
/// Token expiry is handled reactively — requests are sent with the stored
/// token and [ApiClient]'s 401 interceptor triggers a refresh when needed.
/// Network concerns live in [ApiClient], which owns the [Dio] instance.
class SessionManager {
  static const _authTokenKey = 'auth_token';
  static const _refreshTokenKey = 'refresh_token';
  static const _usernameKey = 'username';
  static const _familyIdKey = 'family_id';

  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  SessionManager();

  // ─── Storage ─────────────────────────────────────────────────────────────

  /// Persists both tokens. Called by the auth interceptor on token refresh.
  Future<void> saveTokens({
    required String authToken,
    required String refreshToken,
  }) async {
    await Future.wait([
      _storage.write(key: _authTokenKey, value: authToken),
      _storage.write(key: _refreshTokenKey, value: refreshToken),
    ]);
  }

  /// Persists user info returned by the login endpoint.
  /// [familyId] may be null when the user hasn't joined a family yet.
  Future<void> saveUserData({
    required String username,
    required String? familyId,
  }) async {
    await Future.wait([
      _storage.write(key: _usernameKey, value: username),
      _storage.write(key: _familyIdKey, value: familyId),
    ]);
  }

  Future<String?> getAuthToken() => _storage.read(key: _authTokenKey);
  Future<String?> getRefreshToken() => _storage.read(key: _refreshTokenKey);
  Future<String?> getUsername() => _storage.read(key: _usernameKey);

  /// Returns null when the user has no family yet.
  Future<String?> getFamilyId() => _storage.read(key: _familyIdKey);

  /// Removes only the familyId when the user leaves their family.
  Future<void> clearFamilyId() => _storage.delete(key: _familyIdKey);

  /// Wipes the full session (tokens + user data) on logout.
  Future<void> clearSession() => Future.wait([
    _storage.delete(key: _authTokenKey),
    _storage.delete(key: _refreshTokenKey),
    _storage.delete(key: _usernameKey),
    _storage.delete(key: _familyIdKey),
  ]);
}
