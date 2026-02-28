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
    return 'Server error ($statusCode). Please try again.';
  }
  // null statusCode means no network connection was established.
  return e.message ?? 'Network error. Please try again.';
}

/// Logs an unexpected (non-Dio) error and returns a generic message.
String unexpectedErrorMessage(Object e, String context) {
  log('$context unexpected error: $e');
  return 'An unexpected error occurred.';
}

/// Base exception thrown by every repository.
/// Carries a user-facing [message] so controllers only need one catch.
class RepositoryException implements Exception {
  final String message;
  const RepositoryException(this.message);
}
