import 'package:app_frontend/API/DTOs/products_dtos.dart';
import 'package:flutter/widgets.dart';

/// Tabs visibles en el detalle de una lista.
enum ShoppingListTab { pending, inCart, paid }

/// Modelo ligero para renderizar ítems en la vista.
class ShoppingListItemVM {
  final String id;
  final String name;
  final ProductStatus status;

  const ShoppingListItemVM({
    required this.id,
    required this.name,
    required this.status,
  });

  ShoppingListItemVM copyWith({
    String? id,
    String? name,
    ProductStatus? status,
  }) {
    return ShoppingListItemVM(
      id: id ?? this.id,
      name: name ?? this.name,
      status: status ?? this.status,
    );
  }
}

class ShoppingListController extends ChangeNotifier {
  final String listId;
  final String listName;

  final bool _isLoading = false;
  String? _errorMessage;
  ShoppingListTab _currentTab = ShoppingListTab.pending;
  final List<ShoppingListItemVM> _items;

  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  ShoppingListTab get currentTab => _currentTab;
  List<ShoppingListItemVM> get items => _items;

  /// Devuelve únicamente los ítems del estado asociado a la tab actual.
  List<ShoppingListItemVM> get visibleItems {
    return _items
        .where((item) => item.status == _tabStatus(_currentTab))
        .toList();
  }

  int get pendingCount =>
      _items.where((item) => item.status == ProductStatus.pending).length;

  int get inCartCount =>
      _items.where((item) => item.status == ProductStatus.inCart).length;

  int get paidCount =>
      _items.where((item) => item.status == ProductStatus.paid).length;

  ShoppingListController({required this.listId, required this.listName})
    : _items = _seedItems();

  /// Cambia la tab activa y refresca la UI.
  void setTab(ShoppingListTab tab) {
    if (_currentTab == tab) return;
    _currentTab = tab;
    notifyListeners();
  }

  void setTabByIndex(int index) {
    setTab(ShoppingListTab.values[index]);
  }

  /// Elimina un ítem de la lista local.
  Future<bool> deleteItem(String itemId) async {
    _errorMessage = null;

    try {
      _items.removeWhere((item) => item.id == itemId);
      notifyListeners();
      return true;
    } catch (_) {
      _errorMessage = 'No se pudo eliminar el producto.';
      notifyListeners();
      return false;
    }
  }

  /// Mueve un ítem a pendientes (acción disponible desde "En carrito").
  Future<bool> moveItemToPending(String itemId) async {
    _errorMessage = null;

    try {
      final index = _items.indexWhere((item) => item.id == itemId);
      if (index == -1) return false;

      _items[index] = _items[index].copyWith(status: ProductStatus.pending);
      notifyListeners();
      return true;
    } catch (_) {
      _errorMessage = 'No se pudo mover el producto a pendientes.';
      notifyListeners();
      return false;
    }
  }

  /// Relación entre tab seleccionada y estado de producto.
  ProductStatus _tabStatus(ShoppingListTab tab) {
    switch (tab) {
      case ShoppingListTab.pending:
        return ProductStatus.pending;
      case ShoppingListTab.inCart:
        return ProductStatus.inCart;
      case ShoppingListTab.paid:
        return ProductStatus.paid;
    }
  }

  /// Datos temporales para maquetar la pantalla mientras se integra backend.
  static List<ShoppingListItemVM> _seedItems() {
    return const [
      ShoppingListItemVM(
        id: '1',
        name: 'Manzanas',
        status: ProductStatus.pending,
      ),
      ShoppingListItemVM(id: '2', name: 'Pan', status: ProductStatus.pending),
      ShoppingListItemVM(id: '3', name: 'Leche', status: ProductStatus.inCart),
      ShoppingListItemVM(id: '4', name: 'Huevos', status: ProductStatus.paid),
    ];
  }
}
