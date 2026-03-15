import 'package:app_frontend/API/DTOs/Responses/responses.dart';
import 'package:app_frontend/API/signal_r_client.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Repositories/repository_exception.dart';
import 'package:app_frontend/Repositories/shopping_lists_repository.dart';
import 'package:flutter/widgets.dart';

/// Mediates between [HomeView] and [ShoppingListsRepository].
/// Owns UI concerns: list state, loading indicators, error messages, and form fields.
class HomeController extends ChangeNotifier {
  final ShoppingListsRepository _repository;
  final SessionManager _sessionManager;
  final SignalRClient _signalRClient;

  // ─── Form fields ──────────────────────────────────────────────────────────

  final TextEditingController listNameController = TextEditingController();

  // ─── UI state ─────────────────────────────────────────────────────────────

  bool _isLoading = false;
  bool _isCreatingList = false;
  String? _errorMessage;
  List<ShoppingListResponseDTO> _shoppingLists = [];
  bool _hasCheckedFamily = false;
  bool _needsOnboarding = false; // If true, user should be redirected to onboarding
  final Set<String> _selectedListIds = {}; // IDs of selected shopping lists

  bool get isLoading => _isLoading;
  bool get isCreatingList => _isCreatingList;
  String? get errorMessage => _errorMessage;
  List<ShoppingListResponseDTO> get shoppingLists => _shoppingLists;
  bool get hasCheckedFamily => _hasCheckedFamily;
  bool get needsOnboarding => _needsOnboarding;
  Set<String> get selectedListIds => _selectedListIds;
  bool get isSelectionMode => _selectedListIds.isNotEmpty;

  // ─── Constructor ──────────────────────────────────────────────────────────

  HomeController({
    required ShoppingListsRepository repository,
    required SessionManager sessionManager,
    required SignalRClient signalRClient,
  })  : _repository = repository,
        _sessionManager = sessionManager,
        _signalRClient = signalRClient;

  // ─── Actions ──────────────────────────────────────────────────────────────

  /// Checks if the user has a familyId. Returns true if they have one, false otherwise.
  /// Sets [needsOnboarding] if the user doesn't have a family.
  Future<void> checkFamilyId() async {
    final familyId = await _sessionManager.getFamilyId();
    _needsOnboarding = (familyId == null);
    _hasCheckedFamily = true;
    notifyListeners();
  }

  /// Ensures the SignalR connection is active for realtime updates.
  /// Returns true when connected or false when unavailable.
  Future<bool> ensureRealtimeConnected() async {
    if (_needsOnboarding) return false;
    return _signalRClient.connect();
  }

  /// Fetches active shopping lists. Throws [RepositoryException] on failure.
  Future<void> fetchLists() async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      _shoppingLists = await _repository.fetchActiveLists();
    } on RepositoryException catch (e) {
      _errorMessage = e.message;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  /// Creates a new shopping list. Returns true on success.
  Future<bool> createList() async {
    if (!_validateListName()) return false;

    _errorMessage = null;
    _isCreatingList = true;
    notifyListeners();

    try {
      final newList = await _repository.create(
        listNameController.text.trim(),
      );
      // Add the new list to the beginning of the list
      _shoppingLists.insert(0, newList);
      listNameController.clear();
      return true;
    } on RepositoryException catch (e) {
      _errorMessage = e.message;
      return false;
    } finally {
      _isCreatingList = false;
      notifyListeners();
    }
  }

  /// Toggles selection of a shopping list by its ID.
  void toggleSelection(String listId) {
    if (_selectedListIds.contains(listId)) {
      _selectedListIds.remove(listId);
    } else {
      _selectedListIds.add(listId);
    }
    notifyListeners();
  }

  /// Clears all selected items and exits selection mode.
  void clearSelection() {
    _selectedListIds.clear();
    notifyListeners();
  }

  /// Checks if a list is currently selected.
  bool isListSelected(String listId) {
    return _selectedListIds.contains(listId);
  }

  /// Deletes all selected shopping lists. Returns true if all deletions were successful.
  Future<bool> deleteSelectedLists() async {
    if (_selectedListIds.isEmpty) return false;

    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      // Delete each selected list
      for (final listId in _selectedListIds) {
        await _repository.delete(listId);
      }

      // Remove deleted lists from the local list
      _shoppingLists.removeWhere((list) => _selectedListIds.contains(list.id));
      
      // Clear selection
      _selectedListIds.clear();
      
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

  /// Clears error message (used when dismissing modals).
  void clearError() {
    _errorMessage = null;
    notifyListeners();
  }

  // Returns false and sets [errorMessage] if list name is empty.
  bool _validateListName() {
    if (listNameController.text.trim().isEmpty) {
      _errorMessage = 'El nombre de la lista no puede estar vacío.';
      notifyListeners();
      return false;
    }
    return true;
  }

  @override
  void dispose() {
    listNameController.dispose();
    super.dispose();
  }
}
