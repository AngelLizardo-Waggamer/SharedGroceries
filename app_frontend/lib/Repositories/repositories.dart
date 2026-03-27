import 'package:app_frontend/API/api_client.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Repositories/auth_repository.dart';
import 'package:app_frontend/Repositories/families_repository.dart';
import 'package:app_frontend/Repositories/products_repository.dart';
import 'package:app_frontend/Repositories/shopping_lists_repository.dart';

/// Single entry point for all repositories.
///
/// Place one instance in the widget tree via [Provider<Repositories>].
/// Controllers access the repository they need through the named fields
/// (e.g. `ctx.read<Repositories>().auth`).
class Repositories {
  final AuthRepository auth;
  final FamiliesRepository families;
  final ProductsRepository products;
  final ShoppingListsRepository shoppingLists;

  Repositories({
    required ApiClient apiClient,
    required SessionManager sessionManager,
  }) : auth = AuthRepository(
         apiClient: apiClient,
         sessionManager: sessionManager,
       ),
       families = FamiliesRepository(apiClient: apiClient),
       products = ProductsRepository(apiClient: apiClient),
       shoppingLists = ShoppingListsRepository(apiClient: apiClient);
}
