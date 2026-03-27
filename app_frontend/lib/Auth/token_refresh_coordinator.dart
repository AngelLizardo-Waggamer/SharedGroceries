import 'dart:async';

import 'package:app_frontend/Auth/session_manager.dart';
import 'package:dio/dio.dart';

typedef TokenRefreshRequester =
    Future<Response<dynamic>> Function(String refreshToken);

/// Single gate for token refresh operations.
///
/// Prevents multiple layers from refreshing at the same time and
/// reuses the same in-flight refresh result.
class TokenRefreshCoordinator {
  TokenRefreshCoordinator._();

  static final TokenRefreshCoordinator instance = TokenRefreshCoordinator._();

  Completer<String>? _inFlightRefresh;

  Future<String> refreshAccessToken({
    required SessionManager session,
    required TokenRefreshRequester requester,
    bool clearSessionOnFailure = true,
  }) async {
    // If a refresh is already running, wait for that one.
    if (_inFlightRefresh != null) {
      return _inFlightRefresh!.future;
    }

    final completer = Completer<String>();
    _inFlightRefresh = completer;

    try {
      final refreshToken = await session.getRefreshToken();
      if (refreshToken == null) {
        throw const SessionExpiredException(
          'No refresh token available. Please log in again.',
        );
      }

      final response = await requester(refreshToken);
      final body = response.data as Map<String, dynamic>;

      final newAuthToken = (body['authToken'] ?? body['token']) as String?;
      final newRefreshToken = body['refreshToken'] as String?;

      if (newAuthToken == null || newRefreshToken == null) {
        throw const SessionExpiredException(
          'Refresh response is missing required token fields.',
        );
      }

      await session.saveTokens(
        authToken: newAuthToken,
        refreshToken: newRefreshToken,
      );

      completer.complete(newAuthToken);
      return newAuthToken;
    } catch (e) {
      // Optional hard cleanup when refresh is not recoverable.
      if (clearSessionOnFailure) {
        await session.clearSession();
      }

      final exception = e is SessionExpiredException
          ? e
          : const SessionExpiredException(
              'Session refresh failed. Please log in again.',
            );

      completer.completeError(exception);
      throw exception;
    } finally {
      // Always release the gate so future refreshes can run.
      _inFlightRefresh = null;
    }
  }
}
