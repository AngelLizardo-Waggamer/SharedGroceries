import 'dto.dart';

class RegisterDTO extends DTO {
  final String username;
  final String password;

  RegisterDTO({required this.username, required this.password});

  @override
  Map<String, dynamic> toJson() {
    return {'username': username, 'password': password};
  }
}

class LoginDTO extends DTO {
  final String username;
  final String password;

  LoginDTO({required this.username, required this.password});

  @override
  Map<String, dynamic> toJson() {
    return {'username': username, 'password': password};
  }
}

class RefreshDTO extends DTO {
  final String refreshToken;

  RefreshDTO({required this.refreshToken});

  @override
  Map<String, dynamic> toJson() {
    return {'refreshToken': refreshToken};
  }
}
