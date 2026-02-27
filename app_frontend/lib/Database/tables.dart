import 'package:drift/drift.dart';

// Enum to keep track of the sync status of the local data with the server.
enum SyncStatus {
  synced,
  created,
  updated,
  deleted,
}

class LocalFamilies extends Table {
  TextColumn get id => text()(); // Guid (Just that Drift do not support Guids natively)
  TextColumn get name => text().withLength(min: 1, max: 100)();
  TextColumn get inviteCode => text().withLength(min: 6, max: 6)();

  @override
  Set<Column> get primaryKey => {id};
}

class LocalShoppingLists extends Table {
  TextColumn get id => text()(); // Guid (Just that Drift do not support Guids natively)
  TextColumn get name => text().withLength(min: 1, max: 100)();
  BoolColumn get isActive => boolean().withDefault(const Constant(true))();
  TextColumn get familyId => text()();
  DateTimeColumn get updatedAt => dateTime()();
  IntColumn get syncStatus => intEnum<SyncStatus>().withDefault(Constant(SyncStatus.synced.index))();

  @override
  Set<Column> get primaryKey => {id};
}

class LocalProducts extends Table {
  TextColumn get id => text()(); // Guid (Just that Drift do not support Guids natively)
  TextColumn get name => text().withLength(min: 1, max: 200)();
  TextColumn get quantity => text().nullable()();
  IntColumn get status => integer()(); 
  TextColumn get listId => text().references(LocalShoppingLists, #id)();
  DateTimeColumn get updatedAt => dateTime()();
  IntColumn get syncStatus => intEnum<SyncStatus>().withDefault(Constant(SyncStatus.synced.index))();

  @override
  Set<Column> get primaryKey => {id};
}

class LocalSuggestions extends Table {
  // We use the name as the primary key to avoid duplicates automatically
  TextColumn get productName => text().withLength(min: 1, max: 200)();

  @override
  Set<Column> get primaryKey => {productName};
}