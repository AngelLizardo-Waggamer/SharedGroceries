import 'package:app_frontend/API/DTOs/auth_dtos.dart';
import 'package:app_frontend/API/DTOs/Responses/responses.dart';
import 'package:app_frontend/API/api_client.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Repositories/repository_exception.dart';
import 'package:dio/dio.dart';

/// Handles all authentication data operations: API call + session persistence.
class AuthRepository {
  final ApiClient _apiClient;
  final SessionManager _sessionManager;

  AuthRepository({
    required ApiClient apiClient,
    required SessionManager sessionManager,
  })  : _apiClient = apiClient,
        _sessionManager = sessionManager;

  // ─── Actions ──────────────────────────────────────────────────────────────

  /// Sends credentials, persists the returned session, and returns the
  /// user's [familyId] (null if they haven't joined a family yet).
  /// Throws [RepositoryException] on any failure.
  Future<String?> login(String username, String password) async {
    try {
      final response = await _apiClient.authRequest(
        AuthRoutes.login,
        LoginDTO(username: username, password: password),
      );

      final auth = AuthResponseDTO.fromJson(
        response.data as Map<String, dynamic>,
      );

      await _sessionManager.saveTokens(
        authToken: auth.token,
        refreshToken: auth.refreshToken,
      );

      await _sessionManager.saveUserData(
        username: auth.username,
        familyId: auth.familyId,
      );

      return auth.familyId;
    } on DioException catch (e) {
      // 401 has its own message since it means wrong credentials, not a server fault.
      if (e.response?.statusCode == 401) {
        throw const RepositoryException('Las credenciales son incorrectas. Inténtalo de nuevo.');
      }
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'AuthRepository.login'),
      );
    }
  }

  /// Registers a new account. Throws [RepositoryException] on failure.
  Future<void> register(String username, String password) async {
    try {
      await _apiClient.authRequest(
        AuthRoutes.register,
        RegisterDTO(username: username, password: password),
      );

      login(username, password); // Auto-login after successful registration
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'AuthRepository.register'),
      );
    }
  }
}
