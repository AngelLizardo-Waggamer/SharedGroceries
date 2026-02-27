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
	static const _authTokenKey    = 'auth_token';
	static const _refreshTokenKey = 'refresh_token';

	final FlutterSecureStorage _storage = const FlutterSecureStorage();

	SessionManager();

	// ─── Storage ─────────────────────────────────────────────────────────────

	/// Persists both tokens after a successful login / token refresh.
	Future<void> saveTokens({
		required String authToken,
		required String refreshToken,
	}) async {
		await Future.wait([
			_storage.write(key: _authTokenKey,    value: authToken),
			_storage.write(key: _refreshTokenKey, value: refreshToken),
		]);
	}

	/// Returns the stored access token, or `null` if not present.
	Future<String?> getAuthToken() => _storage.read(key: _authTokenKey);

	/// Returns the stored refresh token, or `null` if not present.
	Future<String?> getRefreshToken() => _storage.read(key: _refreshTokenKey);

	/// Removes both tokens from secure storage (e.g. on explicit logout).
	Future<void> clearSession() => Future.wait([
		_storage.delete(key: _authTokenKey),
		_storage.delete(key: _refreshTokenKey),
	]);

}
