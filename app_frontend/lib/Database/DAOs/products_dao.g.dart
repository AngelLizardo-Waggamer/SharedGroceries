// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'products_dao.dart';

// ignore_for_file: type=lint
mixin _$ProductsDaoMixin on DatabaseAccessor<AppDatabase> {
  $LocalShoppingListsTable get localShoppingLists =>
      attachedDatabase.localShoppingLists;
  $LocalProductsTable get localProducts => attachedDatabase.localProducts;
  ProductsDaoManager get managers => ProductsDaoManager(this);
}

class ProductsDaoManager {
  final _$ProductsDaoMixin _db;
  ProductsDaoManager(this._db);
  $$LocalShoppingListsTableTableManager get localShoppingLists =>
      $$LocalShoppingListsTableTableManager(
        _db.attachedDatabase,
        _db.localShoppingLists,
      );
  $$LocalProductsTableTableManager get localProducts =>
      $$LocalProductsTableTableManager(_db.attachedDatabase, _db.localProducts);
}
