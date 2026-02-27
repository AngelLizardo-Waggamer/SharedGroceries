import 'package:app_frontend/Database/app_db.dart';
import 'package:drift/drift.dart';
import 'package:app_frontend/Database/tables.dart';

part 'shopping_lists_dao.g.dart';

@DriftAccessor(tables: [LocalShoppingLists])
class ShoppingListsDao extends DatabaseAccessor<AppDatabase> with _$ShoppingListsDaoMixin {
	ShoppingListsDao(super.db);
}