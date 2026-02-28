class AuthResponseDTO {
  final String token;
  final String refreshToken;
  final String username;
  final String? familyId;

  AuthResponseDTO({
    required this.token,
    required this.refreshToken,
    required this.username,
    this.familyId,
  });

  factory AuthResponseDTO.fromJson(Map<String, dynamic> json) {
    return AuthResponseDTO(
      token: json['token'] as String,
      refreshToken: json['refreshToken'] as String,
      username: json['username'] as String,
      familyId: json['familyId'] as String?,
    );
  }
}
