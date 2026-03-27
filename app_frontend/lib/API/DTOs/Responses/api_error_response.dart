class ApiErrorResponseDTO {
  final String message;

  ApiErrorResponseDTO({required this.message});

  factory ApiErrorResponseDTO.fromJson(Map<String, dynamic> json) {
    return ApiErrorResponseDTO(message: json['message'] as String);
  }
}
