import 'package:app_frontend/Repositories/auth_repository.dart';
import 'package:app_frontend/Repositories/repository_exception.dart';
import 'package:flutter/widgets.dart';

/// Mediates between [RegisterView] and [AuthRepository].
/// Owns only UI concerns: form fields, loading state, error message, and routing signal.
class RegisterController extends ChangeNotifier {
  final AuthRepository _repository;

  // ─── Form fields ──────────────────────────────────────────────────────────

  // Bound directly to the TextFields so the view never reads .text itself.
  final TextEditingController usernameController = TextEditingController();
  final TextEditingController passwordController = TextEditingController();
  final TextEditingController passwordConfirmController = TextEditingController();

  // ─── UI state ─────────────────────────────────────────────────────────────

  bool _isLoading = false;
  String? _errorMessage;

  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;

  // ─── Constructor ──────────────────────────────────────────────────────────

  RegisterController({required AuthRepository repository})
      : _repository = repository;

  // ─── Actions ──────────────────────────────────────────────────────────────

  /// Validates fields then delegates to [AuthRepository.register].
  /// Returns `true` on success; populates [errorMessage] and returns `false` on failure.
  Future<bool> register() async {
    if (!_validate()) return false;

    _errorMessage = null;
    _isLoading = true;
    notifyListeners();

    try {
      await _repository.register(
        usernameController.text.trim(),
        passwordController.text,
      );
      return true;
    } on RepositoryException catch (e) {
      _errorMessage = e.message;
      return false;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  // ─── Helpers ──────────────────────────────────────────────────────────────

  // Returns false and sets [errorMessage] if any field is empty or passwords don't match.
  bool _validate() {
    if (usernameController.text.trim().isEmpty ||
        passwordController.text.isEmpty ||
        passwordConfirmController.text.isEmpty) {
      _errorMessage = 'Rellena todos los campos.';
      notifyListeners();
      return false;
    }

    if (passwordController.text != passwordConfirmController.text) {
      _errorMessage = 'Las contraseñas no coinciden.';
      notifyListeners();
      return false;
    }

    return true;
  }

  @override
  void dispose() {
    usernameController.dispose();
    passwordController.dispose();
    passwordConfirmController.dispose();
    super.dispose();
  }
}
