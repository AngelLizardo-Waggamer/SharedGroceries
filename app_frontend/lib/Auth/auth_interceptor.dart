import 'dart:async';

import 'package:app_frontend/API/DTOs/auth_dtos.dart';
import 'package:app_frontend/Auth/session_expiration_recovery.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Auth/token_refresh_coordinator.dart';
import 'package:dio/dio.dart';

/// Builds a [QueuedInterceptorsWrapper] that:
///  1. Injects the stored Bearer token into every outgoing request as-is.
///  2. On 401, requests a token refresh through [TokenRefreshCoordinator]
///     and retries the original request once.
///  3. If refresh fails definitively, triggers the global forced-logout flow.
QueuedInterceptorsWrapper buildAuthInterceptor(
  Dio dio,
  String apiVersion,
  SessionManager session,
) {
  return QueuedInterceptorsWrapper(
    onRequest: (options, handler) async {
      // Skip token injection for unauthenticated routes (login, register, refresh).
      if (options.extra['skipAuth'] == true) {
        return handler.next(options);
      }

      // Inject the stored token as-is. If it is expired the server
      // will respond with 401 and onError will handle the refresh.
      final token = await session.getAuthToken();
      if (token != null) {
        options.headers['Authorization'] = 'Bearer $token';
      }
      handler.next(options);
    },

    onError: (error, handler) async {
      final requestOptions = error.requestOptions;

      // Never try to refresh again for skipped auth requests
      // (login/register/refresh) or already retried requests.
      if (requestOptions.extra['skipAuth'] == true ||
          requestOptions.extra['retriedAfterRefresh'] == true) {
        return handler.next(error);
      }

      // Only attempt a refresh once when we receive an Unauthorized response.
      if (error.response?.statusCode != 401) {
        return handler.next(error);
      }

      try {
        final newAuthToken = await TokenRefreshCoordinator.instance
            .refreshAccessToken(
              session: session,
              requester: (refreshToken) {
                return dio.post(
                  'auth/$apiVersion/refresh',
                  data: RefreshDTO(refreshToken: refreshToken).toJson(),
                  options: Options(extra: {'skipAuth': true}),
                );
              },
              clearSessionOnFailure: true,
            );

        // Retry the original request with the fresh token.
        final retryOptions = requestOptions
          ..headers['Authorization'] = 'Bearer $newAuthToken'
          ..extra = {...requestOptions.extra, 'retriedAfterRefresh': true};

        final retryResponse = await dio.fetch(retryOptions);
        return handler.resolve(retryResponse);
      } on SessionExpiredException catch (e) {
        // Refresh also failed – clear the session and propagate.
        unawaited(SessionExpirationRecovery.instance.recover());
        return handler.reject(
          DioException(
            requestOptions: requestOptions,
            error: e,
            message: e.message,
            type: DioExceptionType.badResponse,
          ),
        );
      } on DioException catch (e) {
        return handler.reject(
          DioException(
            requestOptions: requestOptions,
            error: e.error,
            message: e.message,
            type: e.type,
          ),
        );
      }
    },
  );
}
