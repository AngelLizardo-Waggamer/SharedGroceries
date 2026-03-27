import 'package:app_frontend/API/DTOs/shopping_lists_dtos.dart';
import 'package:app_frontend/API/DTOs/Responses/responses.dart';
import 'package:app_frontend/API/api_client.dart';
import 'package:app_frontend/Repositories/repository_exception.dart';
import 'package:dio/dio.dart';

/// Handles all shopping list data operations.
class ShoppingListsRepository {
  final ApiClient _apiClient;

  ShoppingListsRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  // ─── Actions ──────────────────────────────────────────────────────────────

  /// Creates a new shopping list and returns its data. Throws [RepositoryException] on failure.
  Future<ShoppingListResponseDTO> create(String name) async {
    try {
      final response = await _apiClient.listsRequest(
        ListsRoutes.create,
        CreateShoppingListDTO(name: name),
      );
      return ShoppingListResponseDTO.fromJson(
        response.data as Map<String, dynamic>,
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ShoppingListsRepository.create'),
      );
    }
  }

  /// Fetches all active shopping lists. Throws [RepositoryException] on failure.
  Future<List<ShoppingListResponseDTO>> fetchActiveLists() async {
    try {
      final response = await _apiClient.listsRequest(
        ListsRoutes.list,
        CreateShoppingListDTO(
          name: '',
        ), // Dummy DTO since endpoint doesn't need data
      );
      return ShoppingListResponseDTO.listFromJson(
        response.data as List<dynamic>,
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ShoppingListsRepository.fetchActiveLists'),
      );
    }
  }

  /// Fetches all shopping lists (active and inactive). Throws [RepositoryException] on failure.
  Future<List<ShoppingListResponseDTO>> fetchAllLists() async {
    try {
      final response = await _apiClient.listsRequest(
        ListsRoutes.listAll,
        CreateShoppingListDTO(
          name: '',
        ), // Dummy DTO since endpoint doesn't need data
      );
      return ShoppingListResponseDTO.listFromJson(
        response.data as List<dynamic>,
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ShoppingListsRepository.fetchAllLists'),
      );
    }
  }

  /// Deletes a shopping list. Throws [RepositoryException] on failure.
  Future<void> delete(String listId) async {
    try {
      await _apiClient.listsRequest(
        ListsRoutes.delete,
        ShoppingListIdDTO(listId: listId),
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ShoppingListsRepository.delete'),
      );
    }
  }

  /// Restores a deleted shopping list. Throws [RepositoryException] on failure.
  Future<void> restore(String listId) async {
    try {
      await _apiClient.listsRequest(
        ListsRoutes.restore,
        ShoppingListIdDTO(listId: listId),
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ShoppingListsRepository.restore'),
      );
    }
  }

  /// Updates a shopping list's active status. Throws [RepositoryException] on failure.
  Future<void> updateStatus(String listId, bool isActive) async {
    try {
      await _apiClient.listsRequest(
        ListsRoutes.status,
        UpdateShoppingListStatusDTO(listId: listId, isActive: isActive),
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ShoppingListsRepository.updateStatus'),
      );
    }
  }
}
