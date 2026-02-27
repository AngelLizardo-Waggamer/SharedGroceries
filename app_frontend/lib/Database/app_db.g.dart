// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_db.dart';

// ignore_for_file: type=lint
class $LocalFamiliesTable extends LocalFamilies
    with TableInfo<$LocalFamiliesTable, LocalFamily> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $LocalFamiliesTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _idMeta = const VerificationMeta('id');
  @override
  late final GeneratedColumn<String> id = GeneratedColumn<String>(
    'id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _nameMeta = const VerificationMeta('name');
  @override
  late final GeneratedColumn<String> name = GeneratedColumn<String>(
    'name',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 100,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _inviteCodeMeta = const VerificationMeta(
    'inviteCode',
  );
  @override
  late final GeneratedColumn<String> inviteCode = GeneratedColumn<String>(
    'invite_code',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 6,
      maxTextLength: 6,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  @override
  List<GeneratedColumn> get $columns => [id, name, inviteCode];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'local_families';
  @override
  VerificationContext validateIntegrity(
    Insertable<LocalFamily> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('id')) {
      context.handle(_idMeta, id.isAcceptableOrUnknown(data['id']!, _idMeta));
    } else if (isInserting) {
      context.missing(_idMeta);
    }
    if (data.containsKey('name')) {
      context.handle(
        _nameMeta,
        name.isAcceptableOrUnknown(data['name']!, _nameMeta),
      );
    } else if (isInserting) {
      context.missing(_nameMeta);
    }
    if (data.containsKey('invite_code')) {
      context.handle(
        _inviteCodeMeta,
        inviteCode.isAcceptableOrUnknown(data['invite_code']!, _inviteCodeMeta),
      );
    } else if (isInserting) {
      context.missing(_inviteCodeMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {id};
  @override
  LocalFamily map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return LocalFamily(
      id: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}id'],
      )!,
      name: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}name'],
      )!,
      inviteCode: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}invite_code'],
      )!,
    );
  }

  @override
  $LocalFamiliesTable createAlias(String alias) {
    return $LocalFamiliesTable(attachedDatabase, alias);
  }
}

class LocalFamily extends DataClass implements Insertable<LocalFamily> {
  final String id;
  final String name;
  final String inviteCode;
  const LocalFamily({
    required this.id,
    required this.name,
    required this.inviteCode,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['id'] = Variable<String>(id);
    map['name'] = Variable<String>(name);
    map['invite_code'] = Variable<String>(inviteCode);
    return map;
  }

  LocalFamiliesCompanion toCompanion(bool nullToAbsent) {
    return LocalFamiliesCompanion(
      id: Value(id),
      name: Value(name),
      inviteCode: Value(inviteCode),
    );
  }

  factory LocalFamily.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return LocalFamily(
      id: serializer.fromJson<String>(json['id']),
      name: serializer.fromJson<String>(json['name']),
      inviteCode: serializer.fromJson<String>(json['inviteCode']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'id': serializer.toJson<String>(id),
      'name': serializer.toJson<String>(name),
      'inviteCode': serializer.toJson<String>(inviteCode),
    };
  }

  LocalFamily copyWith({String? id, String? name, String? inviteCode}) =>
      LocalFamily(
        id: id ?? this.id,
        name: name ?? this.name,
        inviteCode: inviteCode ?? this.inviteCode,
      );
  LocalFamily copyWithCompanion(LocalFamiliesCompanion data) {
    return LocalFamily(
      id: data.id.present ? data.id.value : this.id,
      name: data.name.present ? data.name.value : this.name,
      inviteCode: data.inviteCode.present
          ? data.inviteCode.value
          : this.inviteCode,
    );
  }

  @override
  String toString() {
    return (StringBuffer('LocalFamily(')
          ..write('id: $id, ')
          ..write('name: $name, ')
          ..write('inviteCode: $inviteCode')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(id, name, inviteCode);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is LocalFamily &&
          other.id == this.id &&
          other.name == this.name &&
          other.inviteCode == this.inviteCode);
}

class LocalFamiliesCompanion extends UpdateCompanion<LocalFamily> {
  final Value<String> id;
  final Value<String> name;
  final Value<String> inviteCode;
  final Value<int> rowid;
  const LocalFamiliesCompanion({
    this.id = const Value.absent(),
    this.name = const Value.absent(),
    this.inviteCode = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  LocalFamiliesCompanion.insert({
    required String id,
    required String name,
    required String inviteCode,
    this.rowid = const Value.absent(),
  }) : id = Value(id),
       name = Value(name),
       inviteCode = Value(inviteCode);
  static Insertable<LocalFamily> custom({
    Expression<String>? id,
    Expression<String>? name,
    Expression<String>? inviteCode,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (id != null) 'id': id,
      if (name != null) 'name': name,
      if (inviteCode != null) 'invite_code': inviteCode,
      if (rowid != null) 'rowid': rowid,
    });
  }

  LocalFamiliesCompanion copyWith({
    Value<String>? id,
    Value<String>? name,
    Value<String>? inviteCode,
    Value<int>? rowid,
  }) {
    return LocalFamiliesCompanion(
      id: id ?? this.id,
      name: name ?? this.name,
      inviteCode: inviteCode ?? this.inviteCode,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (id.present) {
      map['id'] = Variable<String>(id.value);
    }
    if (name.present) {
      map['name'] = Variable<String>(name.value);
    }
    if (inviteCode.present) {
      map['invite_code'] = Variable<String>(inviteCode.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('LocalFamiliesCompanion(')
          ..write('id: $id, ')
          ..write('name: $name, ')
          ..write('inviteCode: $inviteCode, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $LocalShoppingListsTable extends LocalShoppingLists
    with TableInfo<$LocalShoppingListsTable, LocalShoppingList> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $LocalShoppingListsTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _idMeta = const VerificationMeta('id');
  @override
  late final GeneratedColumn<String> id = GeneratedColumn<String>(
    'id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _nameMeta = const VerificationMeta('name');
  @override
  late final GeneratedColumn<String> name = GeneratedColumn<String>(
    'name',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 100,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _isActiveMeta = const VerificationMeta(
    'isActive',
  );
  @override
  late final GeneratedColumn<bool> isActive = GeneratedColumn<bool>(
    'is_active',
    aliasedName,
    false,
    type: DriftSqlType.bool,
    requiredDuringInsert: false,
    defaultConstraints: GeneratedColumn.constraintIsAlways(
      'CHECK ("is_active" IN (0, 1))',
    ),
    defaultValue: const Constant(true),
  );
  static const VerificationMeta _familyIdMeta = const VerificationMeta(
    'familyId',
  );
  @override
  late final GeneratedColumn<String> familyId = GeneratedColumn<String>(
    'family_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _updatedAtMeta = const VerificationMeta(
    'updatedAt',
  );
  @override
  late final GeneratedColumn<DateTime> updatedAt = GeneratedColumn<DateTime>(
    'updated_at',
    aliasedName,
    false,
    type: DriftSqlType.dateTime,
    requiredDuringInsert: true,
  );
  @override
  late final GeneratedColumnWithTypeConverter<SyncStatus, int> syncStatus =
      GeneratedColumn<int>(
        'sync_status',
        aliasedName,
        false,
        type: DriftSqlType.int,
        requiredDuringInsert: false,
        defaultValue: Constant(SyncStatus.synced.index),
      ).withConverter<SyncStatus>(
        $LocalShoppingListsTable.$convertersyncStatus,
      );
  @override
  List<GeneratedColumn> get $columns => [
    id,
    name,
    isActive,
    familyId,
    updatedAt,
    syncStatus,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'local_shopping_lists';
  @override
  VerificationContext validateIntegrity(
    Insertable<LocalShoppingList> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('id')) {
      context.handle(_idMeta, id.isAcceptableOrUnknown(data['id']!, _idMeta));
    } else if (isInserting) {
      context.missing(_idMeta);
    }
    if (data.containsKey('name')) {
      context.handle(
        _nameMeta,
        name.isAcceptableOrUnknown(data['name']!, _nameMeta),
      );
    } else if (isInserting) {
      context.missing(_nameMeta);
    }
    if (data.containsKey('is_active')) {
      context.handle(
        _isActiveMeta,
        isActive.isAcceptableOrUnknown(data['is_active']!, _isActiveMeta),
      );
    }
    if (data.containsKey('family_id')) {
      context.handle(
        _familyIdMeta,
        familyId.isAcceptableOrUnknown(data['family_id']!, _familyIdMeta),
      );
    } else if (isInserting) {
      context.missing(_familyIdMeta);
    }
    if (data.containsKey('updated_at')) {
      context.handle(
        _updatedAtMeta,
        updatedAt.isAcceptableOrUnknown(data['updated_at']!, _updatedAtMeta),
      );
    } else if (isInserting) {
      context.missing(_updatedAtMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {id};
  @override
  LocalShoppingList map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return LocalShoppingList(
      id: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}id'],
      )!,
      name: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}name'],
      )!,
      isActive: attachedDatabase.typeMapping.read(
        DriftSqlType.bool,
        data['${effectivePrefix}is_active'],
      )!,
      familyId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}family_id'],
      )!,
      updatedAt: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}updated_at'],
      )!,
      syncStatus: $LocalShoppingListsTable.$convertersyncStatus.fromSql(
        attachedDatabase.typeMapping.read(
          DriftSqlType.int,
          data['${effectivePrefix}sync_status'],
        )!,
      ),
    );
  }

  @override
  $LocalShoppingListsTable createAlias(String alias) {
    return $LocalShoppingListsTable(attachedDatabase, alias);
  }

  static JsonTypeConverter2<SyncStatus, int, int> $convertersyncStatus =
      const EnumIndexConverter<SyncStatus>(SyncStatus.values);
}

class LocalShoppingList extends DataClass
    implements Insertable<LocalShoppingList> {
  final String id;
  final String name;
  final bool isActive;
  final String familyId;
  final DateTime updatedAt;
  final SyncStatus syncStatus;
  const LocalShoppingList({
    required this.id,
    required this.name,
    required this.isActive,
    required this.familyId,
    required this.updatedAt,
    required this.syncStatus,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['id'] = Variable<String>(id);
    map['name'] = Variable<String>(name);
    map['is_active'] = Variable<bool>(isActive);
    map['family_id'] = Variable<String>(familyId);
    map['updated_at'] = Variable<DateTime>(updatedAt);
    {
      map['sync_status'] = Variable<int>(
        $LocalShoppingListsTable.$convertersyncStatus.toSql(syncStatus),
      );
    }
    return map;
  }

  LocalShoppingListsCompanion toCompanion(bool nullToAbsent) {
    return LocalShoppingListsCompanion(
      id: Value(id),
      name: Value(name),
      isActive: Value(isActive),
      familyId: Value(familyId),
      updatedAt: Value(updatedAt),
      syncStatus: Value(syncStatus),
    );
  }

  factory LocalShoppingList.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return LocalShoppingList(
      id: serializer.fromJson<String>(json['id']),
      name: serializer.fromJson<String>(json['name']),
      isActive: serializer.fromJson<bool>(json['isActive']),
      familyId: serializer.fromJson<String>(json['familyId']),
      updatedAt: serializer.fromJson<DateTime>(json['updatedAt']),
      syncStatus: $LocalShoppingListsTable.$convertersyncStatus.fromJson(
        serializer.fromJson<int>(json['syncStatus']),
      ),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'id': serializer.toJson<String>(id),
      'name': serializer.toJson<String>(name),
      'isActive': serializer.toJson<bool>(isActive),
      'familyId': serializer.toJson<String>(familyId),
      'updatedAt': serializer.toJson<DateTime>(updatedAt),
      'syncStatus': serializer.toJson<int>(
        $LocalShoppingListsTable.$convertersyncStatus.toJson(syncStatus),
      ),
    };
  }

  LocalShoppingList copyWith({
    String? id,
    String? name,
    bool? isActive,
    String? familyId,
    DateTime? updatedAt,
    SyncStatus? syncStatus,
  }) => LocalShoppingList(
    id: id ?? this.id,
    name: name ?? this.name,
    isActive: isActive ?? this.isActive,
    familyId: familyId ?? this.familyId,
    updatedAt: updatedAt ?? this.updatedAt,
    syncStatus: syncStatus ?? this.syncStatus,
  );
  LocalShoppingList copyWithCompanion(LocalShoppingListsCompanion data) {
    return LocalShoppingList(
      id: data.id.present ? data.id.value : this.id,
      name: data.name.present ? data.name.value : this.name,
      isActive: data.isActive.present ? data.isActive.value : this.isActive,
      familyId: data.familyId.present ? data.familyId.value : this.familyId,
      updatedAt: data.updatedAt.present ? data.updatedAt.value : this.updatedAt,
      syncStatus: data.syncStatus.present
          ? data.syncStatus.value
          : this.syncStatus,
    );
  }

  @override
  String toString() {
    return (StringBuffer('LocalShoppingList(')
          ..write('id: $id, ')
          ..write('name: $name, ')
          ..write('isActive: $isActive, ')
          ..write('familyId: $familyId, ')
          ..write('updatedAt: $updatedAt, ')
          ..write('syncStatus: $syncStatus')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode =>
      Object.hash(id, name, isActive, familyId, updatedAt, syncStatus);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is LocalShoppingList &&
          other.id == this.id &&
          other.name == this.name &&
          other.isActive == this.isActive &&
          other.familyId == this.familyId &&
          other.updatedAt == this.updatedAt &&
          other.syncStatus == this.syncStatus);
}

class LocalShoppingListsCompanion extends UpdateCompanion<LocalShoppingList> {
  final Value<String> id;
  final Value<String> name;
  final Value<bool> isActive;
  final Value<String> familyId;
  final Value<DateTime> updatedAt;
  final Value<SyncStatus> syncStatus;
  final Value<int> rowid;
  const LocalShoppingListsCompanion({
    this.id = const Value.absent(),
    this.name = const Value.absent(),
    this.isActive = const Value.absent(),
    this.familyId = const Value.absent(),
    this.updatedAt = const Value.absent(),
    this.syncStatus = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  LocalShoppingListsCompanion.insert({
    required String id,
    required String name,
    this.isActive = const Value.absent(),
    required String familyId,
    required DateTime updatedAt,
    this.syncStatus = const Value.absent(),
    this.rowid = const Value.absent(),
  }) : id = Value(id),
       name = Value(name),
       familyId = Value(familyId),
       updatedAt = Value(updatedAt);
  static Insertable<LocalShoppingList> custom({
    Expression<String>? id,
    Expression<String>? name,
    Expression<bool>? isActive,
    Expression<String>? familyId,
    Expression<DateTime>? updatedAt,
    Expression<int>? syncStatus,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (id != null) 'id': id,
      if (name != null) 'name': name,
      if (isActive != null) 'is_active': isActive,
      if (familyId != null) 'family_id': familyId,
      if (updatedAt != null) 'updated_at': updatedAt,
      if (syncStatus != null) 'sync_status': syncStatus,
      if (rowid != null) 'rowid': rowid,
    });
  }

  LocalShoppingListsCompanion copyWith({
    Value<String>? id,
    Value<String>? name,
    Value<bool>? isActive,
    Value<String>? familyId,
    Value<DateTime>? updatedAt,
    Value<SyncStatus>? syncStatus,
    Value<int>? rowid,
  }) {
    return LocalShoppingListsCompanion(
      id: id ?? this.id,
      name: name ?? this.name,
      isActive: isActive ?? this.isActive,
      familyId: familyId ?? this.familyId,
      updatedAt: updatedAt ?? this.updatedAt,
      syncStatus: syncStatus ?? this.syncStatus,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (id.present) {
      map['id'] = Variable<String>(id.value);
    }
    if (name.present) {
      map['name'] = Variable<String>(name.value);
    }
    if (isActive.present) {
      map['is_active'] = Variable<bool>(isActive.value);
    }
    if (familyId.present) {
      map['family_id'] = Variable<String>(familyId.value);
    }
    if (updatedAt.present) {
      map['updated_at'] = Variable<DateTime>(updatedAt.value);
    }
    if (syncStatus.present) {
      map['sync_status'] = Variable<int>(
        $LocalShoppingListsTable.$convertersyncStatus.toSql(syncStatus.value),
      );
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('LocalShoppingListsCompanion(')
          ..write('id: $id, ')
          ..write('name: $name, ')
          ..write('isActive: $isActive, ')
          ..write('familyId: $familyId, ')
          ..write('updatedAt: $updatedAt, ')
          ..write('syncStatus: $syncStatus, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $LocalProductsTable extends LocalProducts
    with TableInfo<$LocalProductsTable, LocalProduct> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $LocalProductsTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _idMeta = const VerificationMeta('id');
  @override
  late final GeneratedColumn<String> id = GeneratedColumn<String>(
    'id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _nameMeta = const VerificationMeta('name');
  @override
  late final GeneratedColumn<String> name = GeneratedColumn<String>(
    'name',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 200,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _quantityMeta = const VerificationMeta(
    'quantity',
  );
  @override
  late final GeneratedColumn<String> quantity = GeneratedColumn<String>(
    'quantity',
    aliasedName,
    true,
    type: DriftSqlType.string,
    requiredDuringInsert: false,
  );
  static const VerificationMeta _statusMeta = const VerificationMeta('status');
  @override
  late final GeneratedColumn<int> status = GeneratedColumn<int>(
    'status',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _listIdMeta = const VerificationMeta('listId');
  @override
  late final GeneratedColumn<String> listId = GeneratedColumn<String>(
    'list_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
    defaultConstraints: GeneratedColumn.constraintIsAlways(
      'REFERENCES local_shopping_lists (id)',
    ),
  );
  static const VerificationMeta _updatedAtMeta = const VerificationMeta(
    'updatedAt',
  );
  @override
  late final GeneratedColumn<DateTime> updatedAt = GeneratedColumn<DateTime>(
    'updated_at',
    aliasedName,
    false,
    type: DriftSqlType.dateTime,
    requiredDuringInsert: true,
  );
  @override
  late final GeneratedColumnWithTypeConverter<SyncStatus, int> syncStatus =
      GeneratedColumn<int>(
        'sync_status',
        aliasedName,
        false,
        type: DriftSqlType.int,
        requiredDuringInsert: false,
        defaultValue: Constant(SyncStatus.synced.index),
      ).withConverter<SyncStatus>($LocalProductsTable.$convertersyncStatus);
  @override
  List<GeneratedColumn> get $columns => [
    id,
    name,
    quantity,
    status,
    listId,
    updatedAt,
    syncStatus,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'local_products';
  @override
  VerificationContext validateIntegrity(
    Insertable<LocalProduct> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('id')) {
      context.handle(_idMeta, id.isAcceptableOrUnknown(data['id']!, _idMeta));
    } else if (isInserting) {
      context.missing(_idMeta);
    }
    if (data.containsKey('name')) {
      context.handle(
        _nameMeta,
        name.isAcceptableOrUnknown(data['name']!, _nameMeta),
      );
    } else if (isInserting) {
      context.missing(_nameMeta);
    }
    if (data.containsKey('quantity')) {
      context.handle(
        _quantityMeta,
        quantity.isAcceptableOrUnknown(data['quantity']!, _quantityMeta),
      );
    }
    if (data.containsKey('status')) {
      context.handle(
        _statusMeta,
        status.isAcceptableOrUnknown(data['status']!, _statusMeta),
      );
    } else if (isInserting) {
      context.missing(_statusMeta);
    }
    if (data.containsKey('list_id')) {
      context.handle(
        _listIdMeta,
        listId.isAcceptableOrUnknown(data['list_id']!, _listIdMeta),
      );
    } else if (isInserting) {
      context.missing(_listIdMeta);
    }
    if (data.containsKey('updated_at')) {
      context.handle(
        _updatedAtMeta,
        updatedAt.isAcceptableOrUnknown(data['updated_at']!, _updatedAtMeta),
      );
    } else if (isInserting) {
      context.missing(_updatedAtMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {id};
  @override
  LocalProduct map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return LocalProduct(
      id: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}id'],
      )!,
      name: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}name'],
      )!,
      quantity: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}quantity'],
      ),
      status: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}status'],
      )!,
      listId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}list_id'],
      )!,
      updatedAt: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}updated_at'],
      )!,
      syncStatus: $LocalProductsTable.$convertersyncStatus.fromSql(
        attachedDatabase.typeMapping.read(
          DriftSqlType.int,
          data['${effectivePrefix}sync_status'],
        )!,
      ),
    );
  }

  @override
  $LocalProductsTable createAlias(String alias) {
    return $LocalProductsTable(attachedDatabase, alias);
  }

  static JsonTypeConverter2<SyncStatus, int, int> $convertersyncStatus =
      const EnumIndexConverter<SyncStatus>(SyncStatus.values);
}

class LocalProduct extends DataClass implements Insertable<LocalProduct> {
  final String id;
  final String name;
  final String? quantity;
  final int status;
  final String listId;
  final DateTime updatedAt;
  final SyncStatus syncStatus;
  const LocalProduct({
    required this.id,
    required this.name,
    this.quantity,
    required this.status,
    required this.listId,
    required this.updatedAt,
    required this.syncStatus,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['id'] = Variable<String>(id);
    map['name'] = Variable<String>(name);
    if (!nullToAbsent || quantity != null) {
      map['quantity'] = Variable<String>(quantity);
    }
    map['status'] = Variable<int>(status);
    map['list_id'] = Variable<String>(listId);
    map['updated_at'] = Variable<DateTime>(updatedAt);
    {
      map['sync_status'] = Variable<int>(
        $LocalProductsTable.$convertersyncStatus.toSql(syncStatus),
      );
    }
    return map;
  }

  LocalProductsCompanion toCompanion(bool nullToAbsent) {
    return LocalProductsCompanion(
      id: Value(id),
      name: Value(name),
      quantity: quantity == null && nullToAbsent
          ? const Value.absent()
          : Value(quantity),
      status: Value(status),
      listId: Value(listId),
      updatedAt: Value(updatedAt),
      syncStatus: Value(syncStatus),
    );
  }

  factory LocalProduct.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return LocalProduct(
      id: serializer.fromJson<String>(json['id']),
      name: serializer.fromJson<String>(json['name']),
      quantity: serializer.fromJson<String?>(json['quantity']),
      status: serializer.fromJson<int>(json['status']),
      listId: serializer.fromJson<String>(json['listId']),
      updatedAt: serializer.fromJson<DateTime>(json['updatedAt']),
      syncStatus: $LocalProductsTable.$convertersyncStatus.fromJson(
        serializer.fromJson<int>(json['syncStatus']),
      ),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'id': serializer.toJson<String>(id),
      'name': serializer.toJson<String>(name),
      'quantity': serializer.toJson<String?>(quantity),
      'status': serializer.toJson<int>(status),
      'listId': serializer.toJson<String>(listId),
      'updatedAt': serializer.toJson<DateTime>(updatedAt),
      'syncStatus': serializer.toJson<int>(
        $LocalProductsTable.$convertersyncStatus.toJson(syncStatus),
      ),
    };
  }

  LocalProduct copyWith({
    String? id,
    String? name,
    Value<String?> quantity = const Value.absent(),
    int? status,
    String? listId,
    DateTime? updatedAt,
    SyncStatus? syncStatus,
  }) => LocalProduct(
    id: id ?? this.id,
    name: name ?? this.name,
    quantity: quantity.present ? quantity.value : this.quantity,
    status: status ?? this.status,
    listId: listId ?? this.listId,
    updatedAt: updatedAt ?? this.updatedAt,
    syncStatus: syncStatus ?? this.syncStatus,
  );
  LocalProduct copyWithCompanion(LocalProductsCompanion data) {
    return LocalProduct(
      id: data.id.present ? data.id.value : this.id,
      name: data.name.present ? data.name.value : this.name,
      quantity: data.quantity.present ? data.quantity.value : this.quantity,
      status: data.status.present ? data.status.value : this.status,
      listId: data.listId.present ? data.listId.value : this.listId,
      updatedAt: data.updatedAt.present ? data.updatedAt.value : this.updatedAt,
      syncStatus: data.syncStatus.present
          ? data.syncStatus.value
          : this.syncStatus,
    );
  }

  @override
  String toString() {
    return (StringBuffer('LocalProduct(')
          ..write('id: $id, ')
          ..write('name: $name, ')
          ..write('quantity: $quantity, ')
          ..write('status: $status, ')
          ..write('listId: $listId, ')
          ..write('updatedAt: $updatedAt, ')
          ..write('syncStatus: $syncStatus')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode =>
      Object.hash(id, name, quantity, status, listId, updatedAt, syncStatus);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is LocalProduct &&
          other.id == this.id &&
          other.name == this.name &&
          other.quantity == this.quantity &&
          other.status == this.status &&
          other.listId == this.listId &&
          other.updatedAt == this.updatedAt &&
          other.syncStatus == this.syncStatus);
}

class LocalProductsCompanion extends UpdateCompanion<LocalProduct> {
  final Value<String> id;
  final Value<String> name;
  final Value<String?> quantity;
  final Value<int> status;
  final Value<String> listId;
  final Value<DateTime> updatedAt;
  final Value<SyncStatus> syncStatus;
  final Value<int> rowid;
  const LocalProductsCompanion({
    this.id = const Value.absent(),
    this.name = const Value.absent(),
    this.quantity = const Value.absent(),
    this.status = const Value.absent(),
    this.listId = const Value.absent(),
    this.updatedAt = const Value.absent(),
    this.syncStatus = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  LocalProductsCompanion.insert({
    required String id,
    required String name,
    this.quantity = const Value.absent(),
    required int status,
    required String listId,
    required DateTime updatedAt,
    this.syncStatus = const Value.absent(),
    this.rowid = const Value.absent(),
  }) : id = Value(id),
       name = Value(name),
       status = Value(status),
       listId = Value(listId),
       updatedAt = Value(updatedAt);
  static Insertable<LocalProduct> custom({
    Expression<String>? id,
    Expression<String>? name,
    Expression<String>? quantity,
    Expression<int>? status,
    Expression<String>? listId,
    Expression<DateTime>? updatedAt,
    Expression<int>? syncStatus,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (id != null) 'id': id,
      if (name != null) 'name': name,
      if (quantity != null) 'quantity': quantity,
      if (status != null) 'status': status,
      if (listId != null) 'list_id': listId,
      if (updatedAt != null) 'updated_at': updatedAt,
      if (syncStatus != null) 'sync_status': syncStatus,
      if (rowid != null) 'rowid': rowid,
    });
  }

  LocalProductsCompanion copyWith({
    Value<String>? id,
    Value<String>? name,
    Value<String?>? quantity,
    Value<int>? status,
    Value<String>? listId,
    Value<DateTime>? updatedAt,
    Value<SyncStatus>? syncStatus,
    Value<int>? rowid,
  }) {
    return LocalProductsCompanion(
      id: id ?? this.id,
      name: name ?? this.name,
      quantity: quantity ?? this.quantity,
      status: status ?? this.status,
      listId: listId ?? this.listId,
      updatedAt: updatedAt ?? this.updatedAt,
      syncStatus: syncStatus ?? this.syncStatus,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (id.present) {
      map['id'] = Variable<String>(id.value);
    }
    if (name.present) {
      map['name'] = Variable<String>(name.value);
    }
    if (quantity.present) {
      map['quantity'] = Variable<String>(quantity.value);
    }
    if (status.present) {
      map['status'] = Variable<int>(status.value);
    }
    if (listId.present) {
      map['list_id'] = Variable<String>(listId.value);
    }
    if (updatedAt.present) {
      map['updated_at'] = Variable<DateTime>(updatedAt.value);
    }
    if (syncStatus.present) {
      map['sync_status'] = Variable<int>(
        $LocalProductsTable.$convertersyncStatus.toSql(syncStatus.value),
      );
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('LocalProductsCompanion(')
          ..write('id: $id, ')
          ..write('name: $name, ')
          ..write('quantity: $quantity, ')
          ..write('status: $status, ')
          ..write('listId: $listId, ')
          ..write('updatedAt: $updatedAt, ')
          ..write('syncStatus: $syncStatus, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $LocalSuggestionsTable extends LocalSuggestions
    with TableInfo<$LocalSuggestionsTable, LocalSuggestion> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $LocalSuggestionsTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _productNameMeta = const VerificationMeta(
    'productName',
  );
  @override
  late final GeneratedColumn<String> productName = GeneratedColumn<String>(
    'product_name',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 200,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  @override
  List<GeneratedColumn> get $columns => [productName];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'local_suggestions';
  @override
  VerificationContext validateIntegrity(
    Insertable<LocalSuggestion> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('product_name')) {
      context.handle(
        _productNameMeta,
        productName.isAcceptableOrUnknown(
          data['product_name']!,
          _productNameMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_productNameMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {productName};
  @override
  LocalSuggestion map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return LocalSuggestion(
      productName: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}product_name'],
      )!,
    );
  }

  @override
  $LocalSuggestionsTable createAlias(String alias) {
    return $LocalSuggestionsTable(attachedDatabase, alias);
  }
}

class LocalSuggestion extends DataClass implements Insertable<LocalSuggestion> {
  final String productName;
  const LocalSuggestion({required this.productName});
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['product_name'] = Variable<String>(productName);
    return map;
  }

  LocalSuggestionsCompanion toCompanion(bool nullToAbsent) {
    return LocalSuggestionsCompanion(productName: Value(productName));
  }

  factory LocalSuggestion.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return LocalSuggestion(
      productName: serializer.fromJson<String>(json['productName']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'productName': serializer.toJson<String>(productName),
    };
  }

  LocalSuggestion copyWith({String? productName}) =>
      LocalSuggestion(productName: productName ?? this.productName);
  LocalSuggestion copyWithCompanion(LocalSuggestionsCompanion data) {
    return LocalSuggestion(
      productName: data.productName.present
          ? data.productName.value
          : this.productName,
    );
  }

  @override
  String toString() {
    return (StringBuffer('LocalSuggestion(')
          ..write('productName: $productName')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => productName.hashCode;
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is LocalSuggestion && other.productName == this.productName);
}

class LocalSuggestionsCompanion extends UpdateCompanion<LocalSuggestion> {
  final Value<String> productName;
  final Value<int> rowid;
  const LocalSuggestionsCompanion({
    this.productName = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  LocalSuggestionsCompanion.insert({
    required String productName,
    this.rowid = const Value.absent(),
  }) : productName = Value(productName);
  static Insertable<LocalSuggestion> custom({
    Expression<String>? productName,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (productName != null) 'product_name': productName,
      if (rowid != null) 'rowid': rowid,
    });
  }

  LocalSuggestionsCompanion copyWith({
    Value<String>? productName,
    Value<int>? rowid,
  }) {
    return LocalSuggestionsCompanion(
      productName: productName ?? this.productName,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (productName.present) {
      map['product_name'] = Variable<String>(productName.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('LocalSuggestionsCompanion(')
          ..write('productName: $productName, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

abstract class _$AppDatabase extends GeneratedDatabase {
  _$AppDatabase(QueryExecutor e) : super(e);
  $AppDatabaseManager get managers => $AppDatabaseManager(this);
  late final $LocalFamiliesTable localFamilies = $LocalFamiliesTable(this);
  late final $LocalShoppingListsTable localShoppingLists =
      $LocalShoppingListsTable(this);
  late final $LocalProductsTable localProducts = $LocalProductsTable(this);
  late final $LocalSuggestionsTable localSuggestions = $LocalSuggestionsTable(
    this,
  );
  @override
  Iterable<TableInfo<Table, Object?>> get allTables =>
      allSchemaEntities.whereType<TableInfo<Table, Object?>>();
  @override
  List<DatabaseSchemaEntity> get allSchemaEntities => [
    localFamilies,
    localShoppingLists,
    localProducts,
    localSuggestions,
  ];
}

typedef $$LocalFamiliesTableCreateCompanionBuilder =
    LocalFamiliesCompanion Function({
      required String id,
      required String name,
      required String inviteCode,
      Value<int> rowid,
    });
typedef $$LocalFamiliesTableUpdateCompanionBuilder =
    LocalFamiliesCompanion Function({
      Value<String> id,
      Value<String> name,
      Value<String> inviteCode,
      Value<int> rowid,
    });

class $$LocalFamiliesTableFilterComposer
    extends Composer<_$AppDatabase, $LocalFamiliesTable> {
  $$LocalFamiliesTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get inviteCode => $composableBuilder(
    column: $table.inviteCode,
    builder: (column) => ColumnFilters(column),
  );
}

class $$LocalFamiliesTableOrderingComposer
    extends Composer<_$AppDatabase, $LocalFamiliesTable> {
  $$LocalFamiliesTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get inviteCode => $composableBuilder(
    column: $table.inviteCode,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$LocalFamiliesTableAnnotationComposer
    extends Composer<_$AppDatabase, $LocalFamiliesTable> {
  $$LocalFamiliesTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get id =>
      $composableBuilder(column: $table.id, builder: (column) => column);

  GeneratedColumn<String> get name =>
      $composableBuilder(column: $table.name, builder: (column) => column);

  GeneratedColumn<String> get inviteCode => $composableBuilder(
    column: $table.inviteCode,
    builder: (column) => column,
  );
}

class $$LocalFamiliesTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $LocalFamiliesTable,
          LocalFamily,
          $$LocalFamiliesTableFilterComposer,
          $$LocalFamiliesTableOrderingComposer,
          $$LocalFamiliesTableAnnotationComposer,
          $$LocalFamiliesTableCreateCompanionBuilder,
          $$LocalFamiliesTableUpdateCompanionBuilder,
          (
            LocalFamily,
            BaseReferences<_$AppDatabase, $LocalFamiliesTable, LocalFamily>,
          ),
          LocalFamily,
          PrefetchHooks Function()
        > {
  $$LocalFamiliesTableTableManager(_$AppDatabase db, $LocalFamiliesTable table)
    : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$LocalFamiliesTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$LocalFamiliesTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$LocalFamiliesTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<String> id = const Value.absent(),
                Value<String> name = const Value.absent(),
                Value<String> inviteCode = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => LocalFamiliesCompanion(
                id: id,
                name: name,
                inviteCode: inviteCode,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String id,
                required String name,
                required String inviteCode,
                Value<int> rowid = const Value.absent(),
              }) => LocalFamiliesCompanion.insert(
                id: id,
                name: name,
                inviteCode: inviteCode,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$LocalFamiliesTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $LocalFamiliesTable,
      LocalFamily,
      $$LocalFamiliesTableFilterComposer,
      $$LocalFamiliesTableOrderingComposer,
      $$LocalFamiliesTableAnnotationComposer,
      $$LocalFamiliesTableCreateCompanionBuilder,
      $$LocalFamiliesTableUpdateCompanionBuilder,
      (
        LocalFamily,
        BaseReferences<_$AppDatabase, $LocalFamiliesTable, LocalFamily>,
      ),
      LocalFamily,
      PrefetchHooks Function()
    >;
typedef $$LocalShoppingListsTableCreateCompanionBuilder =
    LocalShoppingListsCompanion Function({
      required String id,
      required String name,
      Value<bool> isActive,
      required String familyId,
      required DateTime updatedAt,
      Value<SyncStatus> syncStatus,
      Value<int> rowid,
    });
typedef $$LocalShoppingListsTableUpdateCompanionBuilder =
    LocalShoppingListsCompanion Function({
      Value<String> id,
      Value<String> name,
      Value<bool> isActive,
      Value<String> familyId,
      Value<DateTime> updatedAt,
      Value<SyncStatus> syncStatus,
      Value<int> rowid,
    });

final class $$LocalShoppingListsTableReferences
    extends
        BaseReferences<
          _$AppDatabase,
          $LocalShoppingListsTable,
          LocalShoppingList
        > {
  $$LocalShoppingListsTableReferences(
    super.$_db,
    super.$_table,
    super.$_typedResult,
  );

  static MultiTypedResultKey<$LocalProductsTable, List<LocalProduct>>
  _localProductsRefsTable(_$AppDatabase db) => MultiTypedResultKey.fromTable(
    db.localProducts,
    aliasName: $_aliasNameGenerator(
      db.localShoppingLists.id,
      db.localProducts.listId,
    ),
  );

  $$LocalProductsTableProcessedTableManager get localProductsRefs {
    final manager = $$LocalProductsTableTableManager(
      $_db,
      $_db.localProducts,
    ).filter((f) => f.listId.id.sqlEquals($_itemColumn<String>('id')!));

    final cache = $_typedResult.readTableOrNull(_localProductsRefsTable($_db));
    return ProcessedTableManager(
      manager.$state.copyWith(prefetchedData: cache),
    );
  }
}

class $$LocalShoppingListsTableFilterComposer
    extends Composer<_$AppDatabase, $LocalShoppingListsTable> {
  $$LocalShoppingListsTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<bool> get isActive => $composableBuilder(
    column: $table.isActive,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get familyId => $composableBuilder(
    column: $table.familyId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get updatedAt => $composableBuilder(
    column: $table.updatedAt,
    builder: (column) => ColumnFilters(column),
  );

  ColumnWithTypeConverterFilters<SyncStatus, SyncStatus, int> get syncStatus =>
      $composableBuilder(
        column: $table.syncStatus,
        builder: (column) => ColumnWithTypeConverterFilters(column),
      );

  Expression<bool> localProductsRefs(
    Expression<bool> Function($$LocalProductsTableFilterComposer f) f,
  ) {
    final $$LocalProductsTableFilterComposer composer = $composerBuilder(
      composer: this,
      getCurrentColumn: (t) => t.id,
      referencedTable: $db.localProducts,
      getReferencedColumn: (t) => t.listId,
      builder:
          (
            joinBuilder, {
            $addJoinBuilderToRootComposer,
            $removeJoinBuilderFromRootComposer,
          }) => $$LocalProductsTableFilterComposer(
            $db: $db,
            $table: $db.localProducts,
            $addJoinBuilderToRootComposer: $addJoinBuilderToRootComposer,
            joinBuilder: joinBuilder,
            $removeJoinBuilderFromRootComposer:
                $removeJoinBuilderFromRootComposer,
          ),
    );
    return f(composer);
  }
}

class $$LocalShoppingListsTableOrderingComposer
    extends Composer<_$AppDatabase, $LocalShoppingListsTable> {
  $$LocalShoppingListsTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<bool> get isActive => $composableBuilder(
    column: $table.isActive,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get familyId => $composableBuilder(
    column: $table.familyId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get updatedAt => $composableBuilder(
    column: $table.updatedAt,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get syncStatus => $composableBuilder(
    column: $table.syncStatus,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$LocalShoppingListsTableAnnotationComposer
    extends Composer<_$AppDatabase, $LocalShoppingListsTable> {
  $$LocalShoppingListsTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get id =>
      $composableBuilder(column: $table.id, builder: (column) => column);

  GeneratedColumn<String> get name =>
      $composableBuilder(column: $table.name, builder: (column) => column);

  GeneratedColumn<bool> get isActive =>
      $composableBuilder(column: $table.isActive, builder: (column) => column);

  GeneratedColumn<String> get familyId =>
      $composableBuilder(column: $table.familyId, builder: (column) => column);

  GeneratedColumn<DateTime> get updatedAt =>
      $composableBuilder(column: $table.updatedAt, builder: (column) => column);

  GeneratedColumnWithTypeConverter<SyncStatus, int> get syncStatus =>
      $composableBuilder(
        column: $table.syncStatus,
        builder: (column) => column,
      );

  Expression<T> localProductsRefs<T extends Object>(
    Expression<T> Function($$LocalProductsTableAnnotationComposer a) f,
  ) {
    final $$LocalProductsTableAnnotationComposer composer = $composerBuilder(
      composer: this,
      getCurrentColumn: (t) => t.id,
      referencedTable: $db.localProducts,
      getReferencedColumn: (t) => t.listId,
      builder:
          (
            joinBuilder, {
            $addJoinBuilderToRootComposer,
            $removeJoinBuilderFromRootComposer,
          }) => $$LocalProductsTableAnnotationComposer(
            $db: $db,
            $table: $db.localProducts,
            $addJoinBuilderToRootComposer: $addJoinBuilderToRootComposer,
            joinBuilder: joinBuilder,
            $removeJoinBuilderFromRootComposer:
                $removeJoinBuilderFromRootComposer,
          ),
    );
    return f(composer);
  }
}

class $$LocalShoppingListsTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $LocalShoppingListsTable,
          LocalShoppingList,
          $$LocalShoppingListsTableFilterComposer,
          $$LocalShoppingListsTableOrderingComposer,
          $$LocalShoppingListsTableAnnotationComposer,
          $$LocalShoppingListsTableCreateCompanionBuilder,
          $$LocalShoppingListsTableUpdateCompanionBuilder,
          (LocalShoppingList, $$LocalShoppingListsTableReferences),
          LocalShoppingList,
          PrefetchHooks Function({bool localProductsRefs})
        > {
  $$LocalShoppingListsTableTableManager(
    _$AppDatabase db,
    $LocalShoppingListsTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$LocalShoppingListsTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$LocalShoppingListsTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$LocalShoppingListsTableAnnotationComposer(
                $db: db,
                $table: table,
              ),
          updateCompanionCallback:
              ({
                Value<String> id = const Value.absent(),
                Value<String> name = const Value.absent(),
                Value<bool> isActive = const Value.absent(),
                Value<String> familyId = const Value.absent(),
                Value<DateTime> updatedAt = const Value.absent(),
                Value<SyncStatus> syncStatus = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => LocalShoppingListsCompanion(
                id: id,
                name: name,
                isActive: isActive,
                familyId: familyId,
                updatedAt: updatedAt,
                syncStatus: syncStatus,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String id,
                required String name,
                Value<bool> isActive = const Value.absent(),
                required String familyId,
                required DateTime updatedAt,
                Value<SyncStatus> syncStatus = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => LocalShoppingListsCompanion.insert(
                id: id,
                name: name,
                isActive: isActive,
                familyId: familyId,
                updatedAt: updatedAt,
                syncStatus: syncStatus,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map(
                (e) => (
                  e.readTable(table),
                  $$LocalShoppingListsTableReferences(db, table, e),
                ),
              )
              .toList(),
          prefetchHooksCallback: ({localProductsRefs = false}) {
            return PrefetchHooks(
              db: db,
              explicitlyWatchedTables: [
                if (localProductsRefs) db.localProducts,
              ],
              addJoins: null,
              getPrefetchedDataCallback: (items) async {
                return [
                  if (localProductsRefs)
                    await $_getPrefetchedData<
                      LocalShoppingList,
                      $LocalShoppingListsTable,
                      LocalProduct
                    >(
                      currentTable: table,
                      referencedTable: $$LocalShoppingListsTableReferences
                          ._localProductsRefsTable(db),
                      managerFromTypedResult: (p0) =>
                          $$LocalShoppingListsTableReferences(
                            db,
                            table,
                            p0,
                          ).localProductsRefs,
                      referencedItemsForCurrentItem: (item, referencedItems) =>
                          referencedItems.where((e) => e.listId == item.id),
                      typedResults: items,
                    ),
                ];
              },
            );
          },
        ),
      );
}

typedef $$LocalShoppingListsTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $LocalShoppingListsTable,
      LocalShoppingList,
      $$LocalShoppingListsTableFilterComposer,
      $$LocalShoppingListsTableOrderingComposer,
      $$LocalShoppingListsTableAnnotationComposer,
      $$LocalShoppingListsTableCreateCompanionBuilder,
      $$LocalShoppingListsTableUpdateCompanionBuilder,
      (LocalShoppingList, $$LocalShoppingListsTableReferences),
      LocalShoppingList,
      PrefetchHooks Function({bool localProductsRefs})
    >;
typedef $$LocalProductsTableCreateCompanionBuilder =
    LocalProductsCompanion Function({
      required String id,
      required String name,
      Value<String?> quantity,
      required int status,
      required String listId,
      required DateTime updatedAt,
      Value<SyncStatus> syncStatus,
      Value<int> rowid,
    });
typedef $$LocalProductsTableUpdateCompanionBuilder =
    LocalProductsCompanion Function({
      Value<String> id,
      Value<String> name,
      Value<String?> quantity,
      Value<int> status,
      Value<String> listId,
      Value<DateTime> updatedAt,
      Value<SyncStatus> syncStatus,
      Value<int> rowid,
    });

final class $$LocalProductsTableReferences
    extends BaseReferences<_$AppDatabase, $LocalProductsTable, LocalProduct> {
  $$LocalProductsTableReferences(
    super.$_db,
    super.$_table,
    super.$_typedResult,
  );

  static $LocalShoppingListsTable _listIdTable(_$AppDatabase db) =>
      db.localShoppingLists.createAlias(
        $_aliasNameGenerator(db.localProducts.listId, db.localShoppingLists.id),
      );

  $$LocalShoppingListsTableProcessedTableManager get listId {
    final $_column = $_itemColumn<String>('list_id')!;

    final manager = $$LocalShoppingListsTableTableManager(
      $_db,
      $_db.localShoppingLists,
    ).filter((f) => f.id.sqlEquals($_column));
    final item = $_typedResult.readTableOrNull(_listIdTable($_db));
    if (item == null) return manager;
    return ProcessedTableManager(
      manager.$state.copyWith(prefetchedData: [item]),
    );
  }
}

class $$LocalProductsTableFilterComposer
    extends Composer<_$AppDatabase, $LocalProductsTable> {
  $$LocalProductsTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get quantity => $composableBuilder(
    column: $table.quantity,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get status => $composableBuilder(
    column: $table.status,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get updatedAt => $composableBuilder(
    column: $table.updatedAt,
    builder: (column) => ColumnFilters(column),
  );

  ColumnWithTypeConverterFilters<SyncStatus, SyncStatus, int> get syncStatus =>
      $composableBuilder(
        column: $table.syncStatus,
        builder: (column) => ColumnWithTypeConverterFilters(column),
      );

  $$LocalShoppingListsTableFilterComposer get listId {
    final $$LocalShoppingListsTableFilterComposer composer = $composerBuilder(
      composer: this,
      getCurrentColumn: (t) => t.listId,
      referencedTable: $db.localShoppingLists,
      getReferencedColumn: (t) => t.id,
      builder:
          (
            joinBuilder, {
            $addJoinBuilderToRootComposer,
            $removeJoinBuilderFromRootComposer,
          }) => $$LocalShoppingListsTableFilterComposer(
            $db: $db,
            $table: $db.localShoppingLists,
            $addJoinBuilderToRootComposer: $addJoinBuilderToRootComposer,
            joinBuilder: joinBuilder,
            $removeJoinBuilderFromRootComposer:
                $removeJoinBuilderFromRootComposer,
          ),
    );
    return composer;
  }
}

class $$LocalProductsTableOrderingComposer
    extends Composer<_$AppDatabase, $LocalProductsTable> {
  $$LocalProductsTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get quantity => $composableBuilder(
    column: $table.quantity,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get status => $composableBuilder(
    column: $table.status,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get updatedAt => $composableBuilder(
    column: $table.updatedAt,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get syncStatus => $composableBuilder(
    column: $table.syncStatus,
    builder: (column) => ColumnOrderings(column),
  );

  $$LocalShoppingListsTableOrderingComposer get listId {
    final $$LocalShoppingListsTableOrderingComposer composer = $composerBuilder(
      composer: this,
      getCurrentColumn: (t) => t.listId,
      referencedTable: $db.localShoppingLists,
      getReferencedColumn: (t) => t.id,
      builder:
          (
            joinBuilder, {
            $addJoinBuilderToRootComposer,
            $removeJoinBuilderFromRootComposer,
          }) => $$LocalShoppingListsTableOrderingComposer(
            $db: $db,
            $table: $db.localShoppingLists,
            $addJoinBuilderToRootComposer: $addJoinBuilderToRootComposer,
            joinBuilder: joinBuilder,
            $removeJoinBuilderFromRootComposer:
                $removeJoinBuilderFromRootComposer,
          ),
    );
    return composer;
  }
}

class $$LocalProductsTableAnnotationComposer
    extends Composer<_$AppDatabase, $LocalProductsTable> {
  $$LocalProductsTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get id =>
      $composableBuilder(column: $table.id, builder: (column) => column);

  GeneratedColumn<String> get name =>
      $composableBuilder(column: $table.name, builder: (column) => column);

  GeneratedColumn<String> get quantity =>
      $composableBuilder(column: $table.quantity, builder: (column) => column);

  GeneratedColumn<int> get status =>
      $composableBuilder(column: $table.status, builder: (column) => column);

  GeneratedColumn<DateTime> get updatedAt =>
      $composableBuilder(column: $table.updatedAt, builder: (column) => column);

  GeneratedColumnWithTypeConverter<SyncStatus, int> get syncStatus =>
      $composableBuilder(
        column: $table.syncStatus,
        builder: (column) => column,
      );

  $$LocalShoppingListsTableAnnotationComposer get listId {
    final $$LocalShoppingListsTableAnnotationComposer composer =
        $composerBuilder(
          composer: this,
          getCurrentColumn: (t) => t.listId,
          referencedTable: $db.localShoppingLists,
          getReferencedColumn: (t) => t.id,
          builder:
              (
                joinBuilder, {
                $addJoinBuilderToRootComposer,
                $removeJoinBuilderFromRootComposer,
              }) => $$LocalShoppingListsTableAnnotationComposer(
                $db: $db,
                $table: $db.localShoppingLists,
                $addJoinBuilderToRootComposer: $addJoinBuilderToRootComposer,
                joinBuilder: joinBuilder,
                $removeJoinBuilderFromRootComposer:
                    $removeJoinBuilderFromRootComposer,
              ),
        );
    return composer;
  }
}

class $$LocalProductsTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $LocalProductsTable,
          LocalProduct,
          $$LocalProductsTableFilterComposer,
          $$LocalProductsTableOrderingComposer,
          $$LocalProductsTableAnnotationComposer,
          $$LocalProductsTableCreateCompanionBuilder,
          $$LocalProductsTableUpdateCompanionBuilder,
          (LocalProduct, $$LocalProductsTableReferences),
          LocalProduct,
          PrefetchHooks Function({bool listId})
        > {
  $$LocalProductsTableTableManager(_$AppDatabase db, $LocalProductsTable table)
    : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$LocalProductsTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$LocalProductsTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$LocalProductsTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<String> id = const Value.absent(),
                Value<String> name = const Value.absent(),
                Value<String?> quantity = const Value.absent(),
                Value<int> status = const Value.absent(),
                Value<String> listId = const Value.absent(),
                Value<DateTime> updatedAt = const Value.absent(),
                Value<SyncStatus> syncStatus = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => LocalProductsCompanion(
                id: id,
                name: name,
                quantity: quantity,
                status: status,
                listId: listId,
                updatedAt: updatedAt,
                syncStatus: syncStatus,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String id,
                required String name,
                Value<String?> quantity = const Value.absent(),
                required int status,
                required String listId,
                required DateTime updatedAt,
                Value<SyncStatus> syncStatus = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => LocalProductsCompanion.insert(
                id: id,
                name: name,
                quantity: quantity,
                status: status,
                listId: listId,
                updatedAt: updatedAt,
                syncStatus: syncStatus,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map(
                (e) => (
                  e.readTable(table),
                  $$LocalProductsTableReferences(db, table, e),
                ),
              )
              .toList(),
          prefetchHooksCallback: ({listId = false}) {
            return PrefetchHooks(
              db: db,
              explicitlyWatchedTables: [],
              addJoins:
                  <
                    T extends TableManagerState<
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic
                    >
                  >(state) {
                    if (listId) {
                      state =
                          state.withJoin(
                                currentTable: table,
                                currentColumn: table.listId,
                                referencedTable: $$LocalProductsTableReferences
                                    ._listIdTable(db),
                                referencedColumn: $$LocalProductsTableReferences
                                    ._listIdTable(db)
                                    .id,
                              )
                              as T;
                    }

                    return state;
                  },
              getPrefetchedDataCallback: (items) async {
                return [];
              },
            );
          },
        ),
      );
}

typedef $$LocalProductsTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $LocalProductsTable,
      LocalProduct,
      $$LocalProductsTableFilterComposer,
      $$LocalProductsTableOrderingComposer,
      $$LocalProductsTableAnnotationComposer,
      $$LocalProductsTableCreateCompanionBuilder,
      $$LocalProductsTableUpdateCompanionBuilder,
      (LocalProduct, $$LocalProductsTableReferences),
      LocalProduct,
      PrefetchHooks Function({bool listId})
    >;
typedef $$LocalSuggestionsTableCreateCompanionBuilder =
    LocalSuggestionsCompanion Function({
      required String productName,
      Value<int> rowid,
    });
typedef $$LocalSuggestionsTableUpdateCompanionBuilder =
    LocalSuggestionsCompanion Function({
      Value<String> productName,
      Value<int> rowid,
    });

class $$LocalSuggestionsTableFilterComposer
    extends Composer<_$AppDatabase, $LocalSuggestionsTable> {
  $$LocalSuggestionsTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get productName => $composableBuilder(
    column: $table.productName,
    builder: (column) => ColumnFilters(column),
  );
}

class $$LocalSuggestionsTableOrderingComposer
    extends Composer<_$AppDatabase, $LocalSuggestionsTable> {
  $$LocalSuggestionsTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get productName => $composableBuilder(
    column: $table.productName,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$LocalSuggestionsTableAnnotationComposer
    extends Composer<_$AppDatabase, $LocalSuggestionsTable> {
  $$LocalSuggestionsTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get productName => $composableBuilder(
    column: $table.productName,
    builder: (column) => column,
  );
}

class $$LocalSuggestionsTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $LocalSuggestionsTable,
          LocalSuggestion,
          $$LocalSuggestionsTableFilterComposer,
          $$LocalSuggestionsTableOrderingComposer,
          $$LocalSuggestionsTableAnnotationComposer,
          $$LocalSuggestionsTableCreateCompanionBuilder,
          $$LocalSuggestionsTableUpdateCompanionBuilder,
          (
            LocalSuggestion,
            BaseReferences<
              _$AppDatabase,
              $LocalSuggestionsTable,
              LocalSuggestion
            >,
          ),
          LocalSuggestion,
          PrefetchHooks Function()
        > {
  $$LocalSuggestionsTableTableManager(
    _$AppDatabase db,
    $LocalSuggestionsTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$LocalSuggestionsTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$LocalSuggestionsTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$LocalSuggestionsTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<String> productName = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => LocalSuggestionsCompanion(
                productName: productName,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String productName,
                Value<int> rowid = const Value.absent(),
              }) => LocalSuggestionsCompanion.insert(
                productName: productName,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$LocalSuggestionsTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $LocalSuggestionsTable,
      LocalSuggestion,
      $$LocalSuggestionsTableFilterComposer,
      $$LocalSuggestionsTableOrderingComposer,
      $$LocalSuggestionsTableAnnotationComposer,
      $$LocalSuggestionsTableCreateCompanionBuilder,
      $$LocalSuggestionsTableUpdateCompanionBuilder,
      (
        LocalSuggestion,
        BaseReferences<_$AppDatabase, $LocalSuggestionsTable, LocalSuggestion>,
      ),
      LocalSuggestion,
      PrefetchHooks Function()
    >;

class $AppDatabaseManager {
  final _$AppDatabase _db;
  $AppDatabaseManager(this._db);
  $$LocalFamiliesTableTableManager get localFamilies =>
      $$LocalFamiliesTableTableManager(_db, _db.localFamilies);
  $$LocalShoppingListsTableTableManager get localShoppingLists =>
      $$LocalShoppingListsTableTableManager(_db, _db.localShoppingLists);
  $$LocalProductsTableTableManager get localProducts =>
      $$LocalProductsTableTableManager(_db, _db.localProducts);
  $$LocalSuggestionsTableTableManager get localSuggestions =>
      $$LocalSuggestionsTableTableManager(_db, _db.localSuggestions);
}
