// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'shopping_lists_dao.dart';

// ignore_for_file: type=lint
mixin _$ShoppingListsDaoMixin on DatabaseAccessor<AppDatabase> {
  $LocalShoppingListsTable get localShoppingLists =>
      attachedDatabase.localShoppingLists;
  ShoppingListsDaoManager get managers => ShoppingListsDaoManager(this);
}

class ShoppingListsDaoManager {
  final _$ShoppingListsDaoMixin _db;
  ShoppingListsDaoManager(this._db);
  $$LocalShoppingListsTableTableManager get localShoppingLists =>
      $$LocalShoppingListsTableTableManager(
        _db.attachedDatabase,
        _db.localShoppingLists,
      );
}
