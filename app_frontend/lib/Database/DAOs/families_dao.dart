import 'package:app_frontend/Database/app_db.dart';
import 'package:drift/drift.dart';
import 'package:app_frontend/Database/tables.dart';

part 'families_dao.g.dart';

@DriftAccessor(tables: [LocalFamilies])
class FamiliesDao extends DatabaseAccessor<AppDatabase>
    with _$FamiliesDaoMixin {
  FamiliesDao(super.db);
}
