import 'package:app_frontend/API/DTOs/Responses/family_response.dart';
import 'package:app_frontend/API/signal_r_client.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Repositories/families_repository.dart';
import 'package:app_frontend/Repositories/repository_exception.dart';
import 'package:flutter/widgets.dart';

/// Mediates between [FamilyView] and [FamiliesRepository].
/// Manages family data display and leave flow.
class FamilyController extends ChangeNotifier {
  final FamiliesRepository _repository;
  final SessionManager _sessionManager;
  final SignalRClient _signalRClient;

  // ─── UI state ─────────────────────────────────────────────────────────────

  bool _isLoading = true;
  bool _isLeavingFamily = false;
  FamilyResponseDTO? _familyData;
  String? _errorMessage;

  bool get isLoading => _isLoading;
  bool get isLeavingFamily => _isLeavingFamily;
  FamilyResponseDTO? get familyData => _familyData;
  String? get errorMessage => _errorMessage;

  // ─── Constructor ──────────────────────────────────────────────────────────

  FamilyController({
    required FamiliesRepository repository,
    required SessionManager sessionManager,
    required SignalRClient signalRClient,
  })  : _repository = repository,
        _sessionManager = sessionManager,
        _signalRClient = signalRClient {
    _loadFamilyData();
  }

  // ─── Actions ──────────────────────────────────────────────────────────────

  /// Loads the family data from the API.
  Future<void> _loadFamilyData() async {
    try {
      _familyData = await _repository.getFamilyData();
      _errorMessage = null;
    } on RepositoryException catch (e) {
      _errorMessage = e.message;
    } catch (e) {
      _errorMessage = 'Error al cargar datos de la familia';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// Leaves the family and clears the familyId from session.
  /// Returns `true` on success. The view handles navigation after this completes.
  Future<bool> leaveFamily() async {
    _isLeavingFamily = true;
    _errorMessage = null;
    notifyListeners();

    try {
      await _repository.leave();
      await _signalRClient.disconnect(disposeConnection: true);
      await _sessionManager.clearFamilyId();
      return true;
    } on RepositoryException catch (e) {
      _errorMessage = e.message;
      return false;
    } catch (e) {
      _errorMessage = 'Error al abandonar la familia';
      return false;
    } finally {
      _isLeavingFamily = false;
      notifyListeners();
    }
  }
}
