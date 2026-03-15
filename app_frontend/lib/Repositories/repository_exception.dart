import 'dart:developer';

import 'package:app_frontend/API/DTOs/Responses/responses.dart';
import 'package:dio/dio.dart';

/// Converts a [DioException] into a short user-facing message.
/// Shared by all repositories to avoid duplication.
String dioErrorMessage(DioException e) {
  final statusCode = e.response?.statusCode;
  if (statusCode != null) {
    final data = e.response?.data;
    if (data is Map<String, dynamic> && data.containsKey('message')) {
      return ApiErrorResponseDTO.fromJson(data).message;
    }
    return 'Error de servidor ($statusCode). Intenta de nuevo más tarde.';
  }
  // null statusCode means no network connection was established.
  return 'Ocurrió un error inesperado. Verifica tu conexión e intenta de nuevo.';
}

/// Logs an unexpected (non-Dio) error and returns a generic message.
String unexpectedErrorMessage(Object e, String context) {
  log('$context unexpected error: $e');
  return 'Ocurrió un error inesperado. Verifica tu conexión e intenta de nuevo.';
}

/// Base exception thrown by every repository.
/// Carries a user-facing [message] so controllers only need one catch.
class RepositoryException implements Exception {
  final String message;
  const RepositoryException(this.message);
}
