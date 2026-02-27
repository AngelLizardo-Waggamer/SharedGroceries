import 'dart:io';

import 'package:app_frontend/API/DTOs/dto.dart';
import 'package:app_frontend/Auth/auth_interceptor.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:dio/dio.dart';
import 'package:dio/io.dart';

enum AuthRoutes { register, login, refresh }
enum FamiliesRoutes { create, join, leave }
enum ProductsRoutes { suggestions, create, update, delete, sync }

class ApiClient {
	late Dio _dio;
	late String _apiVersion;

	// Constructor to initialize the API client with a base URL and configure Dio.
	// Provide [sessionManager] to enable automatic Bearer-token injection and
	// transparent token refresh for authorised routes.
	ApiClient({
		required String baseUrl,
		required String apiVersion,
		SessionManager? sessionManager,
	}) {
		_apiVersion = apiVersion;

		// Dio base instance
		_dio = Dio(BaseOptions(
			baseUrl: baseUrl,
			connectTimeout: const Duration(seconds: 5),
			receiveTimeout: const Duration(seconds: 2),
			responseType: ResponseType.json,
		));

		// HttpClient that accepts all or none certificates (this mainly because local API is using http, not https)
		_dio.httpClientAdapter = IOHttpClientAdapter(
			createHttpClient: () {
				final client = HttpClient();
				client.badCertificateCallback =
						(X509Certificate cert, String host, int port) => true;
				return client;
			},
		);

		// When a SessionManager is provided, add the auth interceptor that
		// injects Bearer tokens and handles transparent token refresh on 401.
		if (sessionManager != null) {
			_dio.interceptors.add(buildAuthInterceptor(_dio, _apiVersion, sessionManager));
		}

		// Log interceptor for debugging. Can be removed in prod.
		_dio.interceptors.add(LogInterceptor(requestBody: true, responseBody: true));
	}

	/// Handles requests sent to AuthController.
	/// All auth routes skip token injection since they are unauthenticated endpoints.
	Future<Response> authRequest(AuthRoutes route, DTO data) {
		Options noAuth = Options(extra: {'skipAuth': true});
		try {
			switch (route) {
				case .register:
					return _dio.post('auth/$_apiVersion/register', data: data.toJson(), options: noAuth);
				case .login:
					return _dio.post('auth/$_apiVersion/login', data: data.toJson(), options: noAuth);
				case .refresh:
					return _dio.post('auth/$_apiVersion/refresh', data: data.toJson(), options: noAuth);
			}
		} on DioException catch (e) {
			throw _handleError(e);
		}
	}

	/// <h2>Authorized Routes</h2>
	/// Handles requests sent to FamiliesController <br>
	Future<Response> familiesRequest(FamiliesRoutes route, DTO data) {
		try {
			switch (route) {
				case .create:
					return _dio.post('families/$_apiVersion/create', data: data.toJson());
				case .join:
					return _dio.post('families/$_apiVersion/join', data: data.toJson());
				case .leave:
					return _dio.post('families/$_apiVersion/leave', data: null); // The endpoint does not require any data
			}
		} on DioException catch (e) {
			throw _handleError(e);
		}
	}

	/// <h2>Authorized Routes</h2>
	/// Handles requests sent to ProductsController
	Future<Response> productsRequest(ProductsRoutes route, DTO data) {
		try {
			switch (route) {
				case .suggestions:
					return _dio.get('products/$_apiVersion/suggestions', data: null); // The endpoint does not require any data
				case .create:
					return _dio.post('products/$_apiVersion/create', data: data.toJson());
				case .update:
					return _dio.put('products/$_apiVersion/update', data: data.toJson());
				case .delete:
					return _dio.delete('products/$_apiVersion/delete', data: data.toJson());
				case .sync:
					return _dio.post('products/$_apiVersion/sync', data: data.toJson());
			}
		} on DioException catch (e) {
			throw _handleError(e);
		}
	}

	// Method to extract error msg from DioException to provide readable error messages
	String _handleError(DioException e) {
		return e.message ?? 'An unknown error occurred';
	}
}