import 'package:app_frontend/Database/app_db.dart';
import 'package:drift/drift.dart';
import 'package:app_frontend/Database/tables.dart';

part 'suggestions_dao.g.dart';

@DriftAccessor(tables: [LocalSuggestions])
class SuggestionsDao extends DatabaseAccessor<AppDatabase>
    with _$SuggestionsDaoMixin {
  SuggestionsDao(super.db);
}
