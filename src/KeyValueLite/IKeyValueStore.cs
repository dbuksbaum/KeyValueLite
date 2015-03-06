using System;
using System.Collections.Generic;
using KeyValueLite.Provider;

namespace KeyValueLite
{
  /// <summary>
  /// Implemented to provide a storage system for key values.
  /// </summary>
  public interface IKeyValueStore
  {
    string DatabaseName { get; }
    IKeyValueStore Initialize(Action<IKeyValueOptions> options);
    IKeyValueStore Open();

    void Batch(Action<IKeyValueStore> action);

    long KeyCount();
    bool KeyExists(string key);

    string Get(string key);
    void Set(string key, string value);
    void Set(IEnumerable<KeyValueElement> keyValues);
    void Clear(string key);
    void ClearAllKeyValuesStartingWithKey(string keyPrefix);
    void ClearAllDocuments();

    IEnumerable<string> QueryAllKeys();
    IEnumerable<string> FetchAllKeys();

    IEnumerable<KeyValueElement> QueryAllKeyValuesStartingWithKey(string keyPrefix);
    IEnumerable<KeyValueElement> FetchAllKeyValuesStartingWithKey(string keyPrefix);

    IEnumerable<KeyValueElement> QueryByKeys(params string[] keys);
    IEnumerable<KeyValueElement> FetchByKeys(params string[] keys);

    ISqliteTransaction CreateTransaction();

    //  TODO: Add query/fetchs for keys in a range eg: [Key-1,Key-9]
    //IEnumerable<string> QueryForKeysInRange(params string[] keys);
    //IList<string> FetchForKeysInRange(params string[] keys);
  }
}