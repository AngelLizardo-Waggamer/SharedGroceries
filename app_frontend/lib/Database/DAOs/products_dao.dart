import 'package:app_frontend/Database/app_db.dart';
import 'package:drift/drift.dart';
import 'package:app_frontend/Database/tables.dart';

part 'products_dao.g.dart';

@DriftAccessor(tables: [LocalProducts])
class ProductsDao extends DatabaseAccessor<AppDatabase>
    with _$ProductsDaoMixin {
  ProductsDao(super.db);
}
