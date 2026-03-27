import 'dto.dart';

enum ProductStatus { pending, inCart, paid }

class ProductSuggestionsDTO extends DTO {
  @override
  Map<String, dynamic> toJson() {
    return {};
  }
}

class ProductCreateDTO extends DTO {
  final String productGuid;
  final String name;
  final String quantity;
  final String listGuid;
  final String clientTimestamp;

  ProductCreateDTO({
    required this.productGuid,
    required this.name,
    required this.quantity,
    required this.listGuid,
    required this.clientTimestamp,
  });

  @override
  Map<String, dynamic> toJson() {
    return {
      'id': productGuid,
      'name': name,
      'quantity': quantity,
      'status': ProductStatus.pending.index,
      'listId': listGuid,
      'clientTimestamp': clientTimestamp,
    };
  }
}

class ProductUpdateDTO extends DTO {
  final String productGuid;
  final String name;
  final String quantity;
  final ProductStatus status;
  final String listGuid;
  final String clientTimestamp;

  ProductUpdateDTO({
    required this.productGuid,
    required this.name,
    required this.quantity,
    required this.status,
    required this.listGuid,
    required this.clientTimestamp,
  });

  @override
  Map<String, dynamic> toJson() {
    return {
      'id': productGuid,
      'name': name,
      'quantity': quantity,
      'status': status.index,
      'listId': listGuid,
      'clientTimestamp': clientTimestamp,
    };
  }
}

class ProductSyncDTO extends DTO {
  final List<ProductUpdateDTO> products;

  ProductSyncDTO({required this.products});

  @override
  Map<String, dynamic> toJson() {
    return {'products': products.map((p) => p.toJson()).toList()};
  }
}
