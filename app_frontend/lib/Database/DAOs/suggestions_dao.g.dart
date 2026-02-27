// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'suggestions_dao.dart';

// ignore_for_file: type=lint
mixin _$SuggestionsDaoMixin on DatabaseAccessor<AppDatabase> {
  $LocalSuggestionsTable get localSuggestions =>
      attachedDatabase.localSuggestions;
  SuggestionsDaoManager get managers => SuggestionsDaoManager(this);
}

class SuggestionsDaoManager {
  final _$SuggestionsDaoMixin _db;
  SuggestionsDaoManager(this._db);
  $$LocalSuggestionsTableTableManager get localSuggestions =>
      $$LocalSuggestionsTableTableManager(
        _db.attachedDatabase,
        _db.localSuggestions,
      );
}
