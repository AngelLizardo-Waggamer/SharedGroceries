import '../products_dtos.dart';

class ProductResponseDTO {
  final String id;
  final String name;
  final String? quantity;
  final ProductStatus status;
  final String listId;
  final DateTime clientTimestamp;
  final DateTime updatedAt;

  ProductResponseDTO({
    required this.id,
    required this.name,
    this.quantity,
    required this.status,
    required this.listId,
    required this.clientTimestamp,
    required this.updatedAt,
  });

  factory ProductResponseDTO.fromJson(Map<String, dynamic> json) {
    return ProductResponseDTO(
      id: json['id'] as String,
      name: json['name'] as String,
      quantity: json['quantity'] as String?,
      status: ProductStatus.values[json['status'] as int],
      listId: json['listId'] as String,
      clientTimestamp: DateTime.parse(json['clientTimestamp'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
    );
  }
}

class ProductSuggestionsResponseDTO {
  final List<String> suggestions;

  ProductSuggestionsResponseDTO({required this.suggestions});

  factory ProductSuggestionsResponseDTO.fromJson(List<dynamic> json) {
    return ProductSuggestionsResponseDTO(
      suggestions: List<String>.from(json),
    );
  }
}

class SyncResultResponseDTO {
  final List<String> synced;
  final List<String> ignored;
  final int totalProcessed;

  SyncResultResponseDTO({
    required this.synced,
    required this.ignored,
    required this.totalProcessed,
  });

  factory SyncResultResponseDTO.fromJson(Map<String, dynamic> json) {
    return SyncResultResponseDTO(
      synced: List<String>.from(json['synced'] as List),
      ignored: List<String>.from(json['ignored'] as List),
      totalProcessed: json['totalProcessed'] as int,
    );
  }
}
