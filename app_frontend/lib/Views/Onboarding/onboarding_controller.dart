import 'package:app_frontend/API/signal_r_client.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Repositories/families_repository.dart';
import 'package:app_frontend/Repositories/repository_exception.dart';
import 'package:flutter/widgets.dart';

/// Mediates between [OnboardingView] and [FamiliesRepository].
/// Owns only UI concerns: form fields, loading state, error messages, and success state.
class OnboardingController extends ChangeNotifier {
  final FamiliesRepository _repository;
  final SessionManager _sessionManager;
  final SignalRClient _signalRClient;

  // ─── Form fields ──────────────────────────────────────────────────────────

  final TextEditingController familyNameController = TextEditingController();
  final TextEditingController inviteCodeController = TextEditingController();

  // ─── UI state ─────────────────────────────────────────────────────────────

  bool _isCreating = false;
  bool _isJoining = false;
  String? _errorMessage;
  String? _familyId; // Set on success to trigger navigation

  bool get isCreating => _isCreating;
  bool get isJoining => _isJoining;
  String? get errorMessage => _errorMessage;
  String? get familyId => _familyId;

  // ─── Constructor ──────────────────────────────────────────────────────────

  OnboardingController({
    required FamiliesRepository repository,
    required SessionManager sessionManager,
    required SignalRClient signalRClient,
  }) : _repository = repository,
       _sessionManager = sessionManager,
       _signalRClient = signalRClient;

  // ─── Actions ──────────────────────────────────────────────────────────────

  /// Validates the family name then delegates to [FamiliesRepository.create].
  /// Returns `true` on success; populates [errorMessage] and returns `false` on failure.
  Future<bool> createFamily() async {
    if (!_validateFamilyName()) return false;

    _errorMessage = null;
    _isCreating = true;
    notifyListeners();

    try {
      final family = await _repository.create(familyNameController.text.trim());

      // Persist the family ID in session storage
      await _sessionManager.saveUserData(
        username: (await _sessionManager.getUsername())!,
        familyId: family.id,
      );

      await _signalRClient.connect();

      _familyId = family.id;
      return true;
    } on RepositoryException catch (e) {
      _errorMessage = e.message;
      return false;
    } finally {
      _isCreating = false;
      notifyListeners();
    }
  }

  /// Validates the invite code then delegates to [FamiliesRepository.join].
  /// Returns `true` on success; populates [errorMessage] and returns `false` on failure.
  Future<bool> joinFamily() async {
    if (!_validateInviteCode()) return false;

    _errorMessage = null;
    _isJoining = true;
    notifyListeners();

    try {
      final family = await _repository.join(inviteCodeController.text.trim());

      // Persist the family ID in session storage
      await _sessionManager.saveUserData(
        username: (await _sessionManager.getUsername())!,
        familyId: family.id,
      );

      await _signalRClient.connect();

      _familyId = family.id;
      return true;
    } on RepositoryException catch (e) {
      _errorMessage = e.message;
      return false;
    } finally {
      _isJoining = false;
      notifyListeners();
    }
  }

  // ─── Helpers ──────────────────────────────────────────────────────────────

  /// Clears error message (used when dismissing modals).
  void clearError() {
    _errorMessage = null;
    notifyListeners();
  }

  // Returns false and sets [errorMessage] if family name is empty.
  bool _validateFamilyName() {
    if (familyNameController.text.trim().isEmpty) {
      _errorMessage = 'El nombre de la familia no puede estar vacío.';
      notifyListeners();
      return false;
    }
    return true;
  }

  // Returns false and sets [errorMessage] if invite code is empty or invalid length.
  bool _validateInviteCode() {
    final code = inviteCodeController.text.trim();
    if (code.isEmpty || code.length != 6) {
      _errorMessage = 'El código de invitación debe tener 6 caracteres.';
      notifyListeners();
      return false;
    }
    return true;
  }

  @override
  void dispose() {
    familyNameController.dispose();
    inviteCodeController.dispose();
    super.dispose();
  }
}
