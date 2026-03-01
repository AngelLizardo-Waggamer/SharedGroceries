import 'package:app_frontend/API/DTOs/families_dtos.dart';
import 'package:app_frontend/API/DTOs/Responses/responses.dart';
import 'package:app_frontend/API/api_client.dart';
import 'package:app_frontend/Repositories/repository_exception.dart';
import 'package:dio/dio.dart';

/// Handles all family data operations.
class FamiliesRepository {
  final ApiClient _apiClient;

  FamiliesRepository({required ApiClient apiClient}) : _apiClient = apiClient;

  // ─── Actions ──────────────────────────────────────────────────────────────

  /// Fetches the user's current family data. Throws [RepositoryException] on failure.
  Future<FamilyResponseDTO> getFamilyData() async {
    try {
      final response = await _apiClient.familiesRequest(
        FamiliesRoutes.get,
        GetFamilyDTO(),
      );
      return FamilyResponseDTO.fromJson(
        response.data as Map<String, dynamic>,
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'FamiliesRepository.getFamilyData'),
      );
    }
  }

  /// Creates a new family and returns its data. Throws [RepositoryException] on failure.
  Future<FamilyResponseDTO> create(String familyName) async {
    try {
      final response = await _apiClient.familiesRequest(
        FamiliesRoutes.create,
        CreateFamilyDTO(familyName: familyName),
      );
      return FamilyResponseDTO.fromJson(
        response.data as Map<String, dynamic>,
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'FamiliesRepository.create'),
      );
    }
  }

  /// Joins an existing family via invite code. Throws [RepositoryException] on failure.
  Future<FamilyResponseDTO> join(String inviteCode) async {
    try {
      final response = await _apiClient.familiesRequest(
        FamiliesRoutes.join,
        JoinFamilyDTO(inviteCode: inviteCode),
      );
      return FamilyResponseDTO.fromJson(
        response.data as Map<String, dynamic>,
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'FamiliesRepository.join'),
      );
    }
  }

  /// Removes the current user from their family. Throws [RepositoryException] on failure.
  Future<void> leave() async {
    try {
      await _apiClient.familiesRequest(
        FamiliesRoutes.leave,
        LeaveFamilyDTO(),
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'FamiliesRepository.leave'),
      );
    }
  }
}
