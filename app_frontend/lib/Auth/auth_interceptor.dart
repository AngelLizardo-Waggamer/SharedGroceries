import 'package:app_frontend/API/DTOs/auth_dtos.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:dio/dio.dart';

/// Builds a [QueuedInterceptorsWrapper] that:
///  1. Injects the stored Bearer token into every outgoing request as-is.
///  2. On a 401 response, calls the refresh endpoint via [dio]
///     (marked with a `skipAuth` flag to avoid interceptor recursion),
///     saves the new token pair, and retries the original request once.
///  3. Propagates [SessionExpiredException] (as a [DioException]) when the
///     refresh token is also invalid, clears the session so the user must
///     log in again.
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
			// Only attempt a refresh once when we receive an Unauthorized response.
			if (error.response?.statusCode != 401) {
				return handler.next(error);
			}

			final refreshToken = await session.getRefreshToken();
			if (refreshToken == null) {
				await session.clearSession();
				return handler.reject(DioException(
					requestOptions: error.requestOptions,
					error: const SessionExpiredException(
						'No refresh token available. Please log in again.',
					),
					type: DioExceptionType.badResponse,
				));
			}

			try {
				// Use the same dio instance with skipAuth so the interceptor
				// does not try to inject / refresh a token for this request.
				final refreshResponse = await dio.post(
					'auth/$apiVersion/refresh',
					data: RefreshDTO(refreshToken: refreshToken).toJson(),
					options: Options(extra: {'skipAuth': true}),
				);

				final body = refreshResponse.data as Map<String, dynamic>;
				final newAuthToken = body['authToken'] as String;
				final newRefreshToken = body['refreshToken'] as String;

				await session.saveTokens(
					authToken: newAuthToken,
					refreshToken: newRefreshToken,
				);

				// Retry the original request with the fresh token.
				final retryOptions = error.requestOptions
					..headers['Authorization'] = 'Bearer $newAuthToken';

				final retryResponse = await dio.fetch(retryOptions);
				return handler.resolve(retryResponse);
			} on DioException catch (e) {
				// Refresh also failed – clear the session and propagate.
				await session.clearSession();
				const expired = SessionExpiredException(
					'Session refresh failed. Please log in again.',
				);
				return handler.reject(DioException(
					requestOptions: error.requestOptions,
					error: expired,
					message: expired.message,
					type: e.type,
				));
			}
		},
	);
}
