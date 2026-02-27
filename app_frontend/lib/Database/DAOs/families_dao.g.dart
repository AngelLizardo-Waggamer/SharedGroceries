// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'families_dao.dart';

// ignore_for_file: type=lint
mixin _$FamiliesDaoMixin on DatabaseAccessor<AppDatabase> {
  $LocalFamiliesTable get localFamilies => attachedDatabase.localFamilies;
  FamiliesDaoManager get managers => FamiliesDaoManager(this);
}

class FamiliesDaoManager {
  final _$FamiliesDaoMixin _db;
  FamiliesDaoManager(this._db);
  $$LocalFamiliesTableTableManager get localFamilies =>
      $$LocalFamiliesTableTableManager(_db.attachedDatabase, _db.localFamilies);
}
