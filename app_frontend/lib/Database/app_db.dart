import 'dart:io';

import 'package:app_frontend/Database/DAOs/daos.dart';
import 'package:app_frontend/Database/tables.dart';
import 'package:drift/drift.dart';
import 'package:drift/native.dart';
import 'package:path_provider/path_provider.dart';
import 'package:path/path.dart' as p;

part 'app_db.g.dart';

@DriftDatabase(
	tables: [LocalFamilies, LocalShoppingLists, LocalProducts, LocalSuggestions],
	daos: [FamiliesDao, ShoppingListsDao, ProductsDao, SuggestionsDao],
)
class AppDatabase extends _$AppDatabase{
	AppDatabase() : super(_openConnection());

	@override
	int get schemaVersion => 1;
}

LazyDatabase _openConnection() {
	return LazyDatabase(() async {
		final dir = await getApplicationDocumentsDirectory();
		final file = File(p.join(dir.path, 'app_data.sqlite'));
		return NativeDatabase(file);
	});
}