import 'package:app_frontend/API/DTOs/Responses/responses.dart';
import 'package:app_frontend/API/DTOs/dto.dart';
import 'package:app_frontend/API/DTOs/products_dtos.dart';
import 'package:app_frontend/API/api_client.dart';
import 'package:app_frontend/Repositories/repository_exception.dart';
import 'package:dio/dio.dart';

// Private DTO used only for the delete endpoint.
class _ProductDeleteDTO extends DTO {
  final String productId;
  _ProductDeleteDTO(this.productId);

  @override
  Map<String, dynamic> toJson() => {'id': productId};
}

/// Handles all product data operations.
class ProductsRepository {
  final ApiClient _apiClient;

  ProductsRepository({required ApiClient apiClient}) : _apiClient = apiClient;

  // ─── Actions ──────────────────────────────────────────────────────────────

  /// Returns name suggestions for autocomplete. Throws [RepositoryException] on failure.
  Future<ProductSuggestionsResponseDTO> suggestions() async {
    try {
      final response = await _apiClient.productsRequest(
        ProductsRoutes.suggestions,
        ProductSuggestionsDTO(),
      );
      return ProductSuggestionsResponseDTO.fromJson(
        response.data as List<dynamic>,
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ProductsRepository.suggestions'),
      );
    }
  }

  /// Creates a product and returns the server-confirmed record. Throws [RepositoryException] on failure.
  Future<ProductResponseDTO> create(ProductCreateDTO dto) async {
    try {
      final response = await _apiClient.productsRequest(
        ProductsRoutes.create,
        dto,
      );
      return ProductResponseDTO.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ProductsRepository.create'),
      );
    }
  }

  /// Updates a product and returns the server-confirmed record. Throws [RepositoryException] on failure.
  Future<ProductResponseDTO> update(ProductUpdateDTO dto) async {
    try {
      final response = await _apiClient.productsRequest(
        ProductsRoutes.update,
        dto,
      );
      return ProductResponseDTO.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ProductsRepository.update'),
      );
    }
  }

  /// Deletes a product by its GUID. Throws [RepositoryException] on failure.
  Future<void> delete(String productId) async {
    try {
      await _apiClient.productsRequest(
        ProductsRoutes.delete,
        _ProductDeleteDTO(productId),
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ProductsRepository.delete'),
      );
    }
  }

  /// Syncs a batch of products and returns the result summary. Throws [RepositoryException] on failure.
  Future<SyncResultResponseDTO> sync(List<ProductUpdateDTO> products) async {
    try {
      final response = await _apiClient.productsRequest(
        ProductsRoutes.sync,
        ProductSyncDTO(products: products),
      );
      return SyncResultResponseDTO.fromJson(
        response.data as Map<String, dynamic>,
      );
    } on DioException catch (e) {
      throw RepositoryException(dioErrorMessage(e));
    } catch (e) {
      throw RepositoryException(
        unexpectedErrorMessage(e, 'ProductsRepository.sync'),
      );
    }
  }
}
