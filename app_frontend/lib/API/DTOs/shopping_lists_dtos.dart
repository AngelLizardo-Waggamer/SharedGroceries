import 'dto.dart';

/// DTO for creating a new shopping list.
/// Corresponds to CreateShoppingListRequest in the backend.
/// The name field is sent in the request body.
class CreateShoppingListDTO extends DTO {
	final String name;

	CreateShoppingListDTO({
		required this.name,
	});

	@override
	Map<String, dynamic> toJson() {
		return {
			'name': name,
		};
	}
}

/// DTO for delete/restore shopping list operations.
/// The listId is extracted from toJson() and embedded in the URL path.
/// Example: DELETE /api/shopping-lists/v1/{listId}
class ShoppingListIdDTO extends DTO {
	final String listId;

	ShoppingListIdDTO({
		required this.listId,
	});

	@override
	Map<String, dynamic> toJson() {
		return {
			'listId': listId,
		};
	}
}

/// DTO for updating shopping list status.
/// The listId is embedded in URL path and isActive is sent as query parameter.
/// Example: PATCH /api/shopping-lists/v1/{listId}/status?isActive=true
class UpdateShoppingListStatusDTO extends DTO {
	final String listId;
	final bool isActive;

	UpdateShoppingListStatusDTO({
		required this.listId,
		required this.isActive,
	});

	@override
	Map<String, dynamic> toJson() {
		return {
			'listId': listId,
			'isActive': isActive,
		};
	}
}
