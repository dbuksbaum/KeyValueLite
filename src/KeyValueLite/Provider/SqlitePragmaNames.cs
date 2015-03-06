namespace KeyValueLite.Provider
{
  /// <summary>
  /// List of constant names of known SQLite pragmas
  /// <seealso cref="http://www.sqlite.org/pragma.html"/>
  /// </summary>
  public static class SqlitePragmaNames
  {
    public const string AutoVacuum = "auto_vacuum";
    public const string AutomaticIndex = "automatic_index";
    public const string BusyTimeout = "busy_timeout";
    public const string CacheSize = "cache_size";
    public const string CaseSensitiveLike = "case_sensitive_like";
    public const string CollationList = "collation_list";
    public const string CompileOptions = "compile_options";
    public const string DatabaseList = "database_list";
    public const string Encoding = "encoding";
    public const string ForeignKeyList = "foreign_key_list";
    public const string ForeignKeys = "foreign_keys";
    public const string FreelistCount = "freelist_count";
    public const string IgnoreCheckConstraints = "ignore_check_constraints";
    public const string IncrementalVacuum = "incremental_vacuum";
    public const string IndexInfo = "index_info";
    public const string IndexList = "index_list";
    public const string IntegrityCheck = "integrity_check";
    public const string JournalMode = "journal_mode";
    public const string JournalSizeLimit = "journal_size_limit";
    public const string LegacyFileFormat = "legacy_file_format";
    public const string LockingMode = "locking_mode";
    public const string MaxPageCount = "max_page_count";
    public const string PageCount = "page_count";
    public const string PageSize = "page_size";
    public const string QuickCheck = "quick_check";
    public const string ReadUncommitted = "read_uncommitted";
    public const string RecursiveTriggers = "recursive_triggers";
    public const string ReverseUnorderedSelects = "reverse_unordered_selects";
    public const string SchemaVersion = "schema_version";
    public const string SecureDelete = "secure_delete";
    public const string ShrinkMemory = "shrink_memory";
    public const string Synchronous = "synchronous";
    public const string TableInfo = "table_info";
    public const string TempStore = "temp_store";
    public const string UserVersion = "user_version";
    public const string WalAutocheckpoint = "wal_autocheckpoint";
    public const string WalCheckpoint = "wal_checkpoint";
    public const string WritableSchema = "writable_schema";
  }
}