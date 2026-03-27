import 'package:app_frontend/Repositories/auth_repository.dart';
import 'package:app_frontend/Repositories/repository_exception.dart';
import 'package:flutter/widgets.dart';

/// Mediates between [LoginView] and [AuthRepository].
/// Owns only UI concerns: form fields, loading state, error message, and routing signal.
class LoginController extends ChangeNotifier {
  final AuthRepository _repository;

  // ─── Form fields ──────────────────────────────────────────────────────────

  // Bound directly to the TextFields so the view never reads .text itself.
  final TextEditingController usernameController = TextEditingController();
  final TextEditingController passwordController = TextEditingController();

  // ─── UI state ─────────────────────────────────────────────────────────────

  bool _isLoading = false;
  String? _errorMessage;

  // Null means the user has not joined a family yet → navigate to onboarding.
  String? _familyId;

  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  String? get familyId => _familyId;

  // ─── Constructor ──────────────────────────────────────────────────────────

  LoginController({required AuthRepository repository})
    : _repository = repository;

  // ─── Actions ──────────────────────────────────────────────────────────────

  /// Validates fields then delegates to [AuthRepository.login].
  /// Returns `true` on success; populates [errorMessage] and returns `false` on failure.
  Future<bool> login() async {
    if (!_validate()) return false;

    _errorMessage = null;
    _isLoading = true;
    notifyListeners();

    try {
      _familyId = await _repository.login(
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

  // Returns false and sets [errorMessage] if any field is empty.
  bool _validate() {
    if (usernameController.text.trim().isEmpty ||
        passwordController.text.isEmpty) {
      _errorMessage = 'Rellena todos los campos.';
      notifyListeners();
      return false;
    }
    return true;
  }

  @override
  void dispose() {
    usernameController.dispose();
    passwordController.dispose();
    super.dispose();
  }
}
