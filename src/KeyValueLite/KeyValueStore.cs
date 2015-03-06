using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KeyValueLite.Provider;
using SQLite;

namespace KeyValueLite
{
  /// <summary>
  /// Implementation of a Key Value storage system using 
  /// SQLite (<see cref="http://www.sqlite.org/index.html"/>)
  /// TODO: Pregenerate queries with table name
  /// </summary>
  public class KeyValueStore : AbstractDisposable, IKeyValueStore
  {
    #region Fields
    private const string WildcardSuffix = "%";
    private const string InMemoryDatabaseName = ":memory:";
    private const int SchemaVersion = 1;
    private KeyValueOptions _options;
    private string _tableName;
    private SQLiteConnection _dbConnection;

    //  pre-generated queries for this table only
    private string _countKeyValues;
    private string _countKeyValuesByKey;
    private string _selectKeyValueValuesStartingWithKey;
    private string _deleteKeyValueValuesStartingWithKey;
    private string _queryKeyValueKeys;
    #endregion

    #region Implementation of IKeyValueStore
    public string DatabaseName { get; private set; }
    public IKeyValueStore Initialize(Action<IKeyValueOptions> options)
    {
      if (_dbConnection != null)
        throw new InvalidOperationException("Cannot call initialize twice.");

      _options = new KeyValueOptions();

      if (options != null)
        options(_options);

      if (_options.InMemory)
      { //  database will be in memory only
        //throw new NotSupportedException("In-Memory Databases Not Supported Yet.");
        DatabaseName = InMemoryDatabaseName;

        //  these defaults are required with memory databases
        _options.DeleteExisting = false;
        _options.CreateIfMissing = true;
      }
      else
      { //  database will be on disk
        if (string.IsNullOrWhiteSpace(_options.DatabaseName))
          throw new InvalidOperationException("You must specify the InMemory option or set the DatabaseName prior to initialization.");

        DatabaseName = _options.DatabaseName;

        //  delete the database if it already exists
        //        var exists = await FileSystem.Current.LocalStorage.CheckExistsAsync(DatabaseName);
        //        if (_options.DeleteExisting && exists == ExistenceCheckResult.FileExists)// File.Exists(DatabaseName))
        //        {
        //          this.Log().Info("Deleting existing database on initialization: " + DatabaseName);
        //          var file = await FileSystem.Current.LocalStorage.GetFileAsync(DatabaseName);
        //          await file.DeleteAsync();
        //        }

        if (_options.DeleteExisting && File.Exists(DatabaseName))
        {
          //this.Log().Info("Deleting existing database on initialization: " + DatabaseName);
          File.Delete(DatabaseName);
        }
      }

      //  create the connection string
      //  Cache Size=2000; Page Size=1024; Max Page Count=5000; Compress=True; Default Timeout=30;
      //  Disable the Journal File: Journal Mode=Off;
      //  Persist the Journal File: Journal Mode=Persist;
      //  Delete the journal file after commit: Journal Mode=Delete;
      //      _connectionString = new SQLiteConnectionStringBuilder()
      //                            {
      //                              DataSource = DatabaseName,                  //  the database name
      //                              Version = 3,                                //  use version 3 file format
      //                              ToFullPath = false,                         //  already have fully qualified path
      //                              UseUTF16Encoding = _options.UseUtf16,       //  UTF-8 or UTF-16
      //                              SyncMode = SynchronizationModes.Full,       //  flush after each write
      //                              BinaryGUID = true,                          //  use a binary GUID or a string
      //                              Enlist = false,                             //  Do not enlist in distributed transactions
      //                              Pooling = false,                            //  use connection pooling
      //                              FailIfMissing = !_options.CreateIfMissing,  //  create if missing
      //                              DateTimeFormat = SQLiteDateFormats.Ticks,   //  ticks are fastest representation of datetime
      //                              DateTimeKind = DateTimeKind.Utc             //  use UTC dates
      //                            }.ToString();

      var flags = SQLiteOpenFlags.ReadWrite;

      if (_options.CreateIfMissing)
        FlagsHelper.Set(ref flags, SQLiteOpenFlags.Create);
      else
        FlagsHelper.Unset(ref flags, SQLiteOpenFlags.Create);

      _dbConnection = new SQLiteConnection(DatabaseName, flags, true);
      //this.Log().Debug("Connection created for database: " + DatabaseName);

      //  set options here

      //  set the UTF
      _dbConnection.Execute(_options.UseUtf16 ? SqlStatements.PragmaEncodingUTF16 : SqlStatements.PragmaEncodingUTF8);

      return this;
    }
    public IKeyValueStore Open()
    {
      try
      {
        //this.Log().Info("Begin database open: " + _dbConnection.DatabasePath);

        //  create the schema if needed
        CreateKeyValueTable();

        //  verify the database
        VerifyDatabase();

        //this.Log().Info("End database open: " + _dbConnection.DatabasePath);
      }
      catch (SQLiteException exception)
      {
        switch (exception.Result)
        {
          case SQLite3.Result.NonDBFile:
            throw new InvalidOperationException("File is not a database.", exception);
          case SQLite3.Result.Corrupt:
            throw new InvalidOperationException("Database file is corrupt.", exception);
          case SQLite3.Result.CannotOpen:
          default:
            throw new InvalidOperationException("Could not open database: " + DatabaseName, exception);
        }
      }

      return this;
    }
    public void Batch(Action<IKeyValueStore> action)
    {
      _dbConnection.RunInTransaction(() => action(this));
    }
    public long KeyCount()
    {
      return _dbConnection.ExecuteScalar<long>(_countKeyValues);
    }
    public bool KeyExists(string key)
    {
      key.MustNotBeEmpty(key);
      return _dbConnection.ExecuteScalar<long>(_countKeyValuesByKey, key) > 0;
    }
    public string Get(string key)
    {
      key.MustNotBeEmpty(key);
      var result = _dbConnection.Find<KeyValueElement>(key);
      if ((result == null) && (_options.ThrowOnGetKeyNotFound))
        throw new KeyNotFoundException();

      return (result != null) ? result.Value : null;
    }
    public void Set(string key, string value)
    {
      Set(new KeyValueElement { Key = key, Value = value });
    }
    public void Set(KeyValueElement element)
    {
      element.MustNotBeNull();
      element.Key.MustNotBeEmpty();
      element.Value.MustNotBeEmpty();
      element.LastUpdateTime = DateTime.UtcNow;
      _dbConnection.InsertOrReplace(element);
    }
    public void Set(IEnumerable<KeyValueElement> keyValues)
    {
      var keyValueElements = keyValues as KeyValueElement[] ?? keyValues.ToArray();
      var now = DateTime.UtcNow;

      //  update the last update time
      foreach (var element in keyValueElements)
        element.LastUpdateTime = now;

      //  then insert them all
      _dbConnection.InsertAll(keyValueElements);
    }
    public void Clear(string key)
    {
      key.MustNotBeEmpty(key);
      _dbConnection.Delete<KeyValueElement>(key);
    }
    public void ClearAllKeyValuesStartingWithKey(string keyPrefix)
    {
      if (!keyPrefix.EndsWith(WildcardSuffix))
        keyPrefix += WildcardSuffix;

      _dbConnection.Execute(_deleteKeyValueValuesStartingWithKey, keyPrefix);
    }
    public void ClearAllDocuments()
    {
      if (_options.ThrowOnClearAll)
        throw new InvalidOperationException("Clear All Documents is prevented by an option set on open.");

      _dbConnection.BeginTransaction();
      try
      {
        _dbConnection.DropTable<KeyValueElement>();
        _dbConnection.CreateTable<KeyValueElement>();
        _dbConnection.Commit();
      }
      catch (Exception)
      {
        _dbConnection.Rollback();
        throw;
      }
    }
    public IEnumerable<string> QueryAllKeys()
    {
      return (from kve in _dbConnection.Query<KeyValueElement>(_queryKeyValueKeys)
              select kve.Key);
    }
    public IEnumerable<string> FetchAllKeys()
    {
      return (from kve in _dbConnection.Query<KeyValueElement>(_queryKeyValueKeys)
              select kve.Key).ToList();
    }
    public IEnumerable<KeyValueElement> QueryAllKeyValuesStartingWithKey(string keyPrefix)
    {
      if (!keyPrefix.EndsWith(WildcardSuffix))
        keyPrefix += WildcardSuffix;

      return (from kve in _dbConnection.Query<KeyValueElement>(_selectKeyValueValuesStartingWithKey, keyPrefix)
              select kve);
    }
    public IEnumerable<KeyValueElement> FetchAllKeyValuesStartingWithKey(string keyPrefix)
    {
      if (!keyPrefix.EndsWith(WildcardSuffix))
        keyPrefix += WildcardSuffix;

      return (from kve in _dbConnection.Query<KeyValueElement>(_selectKeyValueValuesStartingWithKey, keyPrefix)
              select kve).ToList();
    }
    public IEnumerable<KeyValueElement> QueryByKeys(params string[] keys)
    { //  TODO: This can be optimized to retrieve pages of keys at a time
      return (from kvElement in _dbConnection.Table<KeyValueElement>()
              join k in keys on kvElement.Key equals k
              //where keys.Contains(kvElement.Key)
              select kvElement);
    }
    public IEnumerable<KeyValueElement> FetchByKeys(params string[] keys)
    { //  TODO: This can be optimized to retrieve pages of keys at a time
      return (from kvElement in _dbConnection.Table<KeyValueElement>()
              join k in keys on kvElement.Key equals k
              //where keys.Contains(kvElement.Key)
              select kvElement).ToList();
    }
    public ISqliteTransaction CreateTransaction()
    {
      return new SqliteTransaction(_dbConnection);
    }
    #endregion

    #region Private Methods
    private void CreateKeyValueTable()
    {
      //  get the table attribute
      //var attribute = typeof(KeyValueElement).GetAttributes<SQLite.TableAttribute>().FirstOrDefault();
      var typeInfo = typeof(KeyValueElement).GetTypeInfo();
      var attribute = typeInfo.GetCustomAttribute<SQLite.TableAttribute>();
      if (attribute == null)
        throw new InvalidOperationException("KeyValueElement is missing Table attribute.");
      _tableName = attribute.Name;

      //  pre-generate queries
      PregenerateQueries(_tableName);

      //  check if the table exists
      var tableExists = TableExists(_tableName);

      //  create tables
      _dbConnection.CreateTable<KeyValueElement>();

      //  if we had to make a change, reset the schema version
      if (!tableExists)
        _dbConnection.Execute(string.Format(SqlStatements.PragmaUserVersion, SchemaVersion));
    }
    private bool TableExists(string tableName)
    {
      return (string.Compare(tableName, _dbConnection.ExecuteScalar<string>(SqlStatements.CheckIfTableExists, tableName),
                             StringComparison.Ordinal) == 0);
    }
    private void PregenerateQueries(string tableName)
    {
      _countKeyValues = string.Format(SqlStatements.CountKeyValues, tableName);
      _countKeyValuesByKey = string.Format(SqlStatements.CountKeyValuesByKey, tableName);
      _selectKeyValueValuesStartingWithKey = string.Format(SqlStatements.SelectKeyValueValuesStartingWithKey, tableName);
      _deleteKeyValueValuesStartingWithKey = string.Format(SqlStatements.DeleteKeyValueValuesStartingWithKey, tableName);
      _queryKeyValueKeys = string.Format(SqlStatements.QueryKeyValueKeys, tableName);
    }
    private void VerifyDatabase()
    {
      var engineVersion = _dbConnection.ExecuteScalar<string>("SELECT sqlite_version();	");
      var revisionVersion = _dbConnection.ExecuteScalar<long>("PRAGMA schema_version;");
      var userVersion = _dbConnection.ExecuteScalar<long>("PRAGMA user_version;");
      //this.Log().Debug("\tEngine Version: " + engineVersion);
      //this.Log().Debug("\tSchema Revision Count: " + revisionVersion);
      //this.Log().Debug("\tUser Version: " + userVersion);

      if (userVersion < SchemaVersion)
      { //  disk is less than library
        throw new InvalidOperationException("The database version (" + userVersion + ") is older than the library version (" +
                                            SchemaVersion + ")." +
                                            Environment.NewLine +
                                            "Please upgrade the disk file.");
      }

      if (userVersion > SchemaVersion)
      { //  disk is greater than the library
        throw new InvalidOperationException("The database version (" + userVersion + ") is from a new version of the library than the current version (" +
                                            SchemaVersion + ")." +
                                            Environment.NewLine +
                                            "Please upgrade the application.");
      }

      if (_options.VerifyOnOpen)
      { //  run integrity check
        if (_options.QuickVerify)
        {
          //this.Log().Info("\tStart Quick Integrity Verification");
          var results = _dbConnection.Query<IntegrityCheck>("PRAGMA quick_check;");

          if (string.Compare("ok", results.First().Result, StringComparison.OrdinalIgnoreCase) != 0)
          { //  check failed
            //foreach (var issue in results)
            //  this.Log().Warn("\t" + issue);
          }
          //this.Log().Info("\tEnd Quick Integrity Verification");
        }
        else
        {
          //this.Log().Info("\tStart Full Integrity Verification");
          var results = _dbConnection.Query<IntegrityCheck>("PRAGMA integrity_check;");
          if (string.Compare("ok", results.First().Result, StringComparison.OrdinalIgnoreCase) != 0)
          { //  check failed
            //foreach (var issue in results)
            //  this.Log().Warn("\t" + issue);
          }
          //this.Log().Info("\tEnd Full Integrity Verification");
        }
      }
    }
    #endregion

    #region Overrides of AbstractDisposable
    /// <summary>
    /// Disposes the unmanaged resources. This is called at the right time in the disposal process.
    /// </summary>
    protected override void DisposeUnmanagedResources()
    {
    }
    /// <summary>
    /// Disposes the managed resources. This is called at the right time in the disposal process.
    /// </summary>
    protected override void DisposeManagedResources()
    {
      if (_dbConnection != null)
        _dbConnection.Close();
      _dbConnection = null;
    }
    #endregion
  }
}