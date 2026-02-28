class ShoppingListResponseDTO {
  final String id;
  final String name;
  final bool isActive;
  final DateTime createdAt;
  final String familyId;

  ShoppingListResponseDTO({
    required this.id,
    required this.name,
    required this.isActive,
    required this.createdAt,
    required this.familyId,
  });

  factory ShoppingListResponseDTO.fromJson(Map<String, dynamic> json) {
    return ShoppingListResponseDTO(
      id: json['id'] as String,
      name: json['name'] as String,
      isActive: json['isActive'] as bool,
      createdAt: DateTime.parse(json['createdAt'] as String),
      familyId: json['familyId'] as String,
    );
  }

  static List<ShoppingListResponseDTO> listFromJson(List<dynamic> json) {
    return json
        .map((e) => ShoppingListResponseDTO.fromJson(e as Map<String, dynamic>))
        .toList();
  }
}
