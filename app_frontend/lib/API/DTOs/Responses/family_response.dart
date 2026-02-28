class FamilyResponseDTO {
  final String id;
  final String name;
  final String inviteCode;

  FamilyResponseDTO({
    required this.id,
    required this.name,
    required this.inviteCode,
  });

  factory FamilyResponseDTO.fromJson(Map<String, dynamic> json) {
    return FamilyResponseDTO(
      id: json['id'] as String,
      name: json['name'] as String,
      inviteCode: json['inviteCode'] as String,
    );
  }
}
