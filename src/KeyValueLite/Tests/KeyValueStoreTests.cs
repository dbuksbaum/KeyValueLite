using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SQLite;

namespace KeyValueLite.Tests
{
  [TestFixture, Category("Data")]
  public class KeyValueStoreTests
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public KeyValueStoreTests()
    {
    }

    [Test]
    //[ExpectedException(typeof(NotSupportedException))]
    public void TestInMemoryDatabaseCanBeOpened()
    {
      using (var kvs = new KeyValueStore())
      {
        kvs.Initialize(options =>
        {
          options.InMemory = true;
        });
        kvs.Open();
      }
    }
    [Test]
    public void TestOpenFileDatabaseWithDefaults()
    {
      var dbName = Path.GetTempFileName();
      File.Exists(dbName).Should().BeTrue();

      using (var kvs = new KeyValueStore())
      {
        kvs.Initialize(options =>
        {
          options.DatabaseName = dbName;
        });
        kvs.Open();
      }

      File.Delete(dbName);
      File.Exists(dbName).Should().BeFalse();
    }
    [Test]
    public void TestOpenDatabaseWithoutCreateFirstThrows()
    {
      var dbName = Path.GetTempFileName();
      File.Exists(dbName).Should().BeTrue();
      File.Delete(dbName);
      File.Exists(dbName).Should().BeFalse();

      Assert.Throws<SQLiteException>(() =>
      {
        using (var kvs = new KeyValueStore())
        {
          kvs.Initialize(options =>
          {
            options.DatabaseName = dbName;
            options.CreateIfMissing = false;
          });
          kvs.Open();
        }
      });
    }
    [Test]
    public void TestBasicOperations([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);
        kvs.KeyExists("NotExistantKey").Should().BeFalse();
        kvs.Set("KeyOne", "DataOne");
        kvs.KeyCount().Should().Be(1);
        kvs.KeyExists("KeyOne").Should().BeTrue();
        kvs.Get("KeyOne").Should().Be("DataOne");
        kvs.Set(new KeyValueElement { Key = "KeyTwo", Value = "DataTwo" });


        kvs.QueryAllKeys().Should().HaveCount(2).And.ContainSingle(val => string.Compare(val, "KeyOne", StringComparison.InvariantCultureIgnoreCase) == 0);

        kvs.FetchAllKeys().Should().HaveCount(2).And.ContainSingle(val => string.Compare(val, "KeyOne", StringComparison.InvariantCultureIgnoreCase) == 0);

        kvs.Clear("KeyOne");
        kvs.KeyCount().Should().Be(1);
        kvs.Clear("KeyTwo");
        kvs.KeyCount().Should().Be(0);
      }
    }
    [Test]
    public void TestQueryStartingWithKeyWithoutWildcard([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);
        kvs.Set("KeyOne", "DataOne");
        kvs.KeyCount().Should().Be(1);
        kvs.KeyExists("KeyOne").Should().BeTrue();
        kvs.Get("KeyOne").Should().Be("DataOne");

        kvs.QueryAllKeyValuesStartingWithKey("Key").Should()
          .HaveCount(1).And
          .ContainSingle(val =>
            (string.Compare(val.Key, "KeyOne", StringComparison.InvariantCultureIgnoreCase) == 0) &&
            (string.Compare(val.Value, "DataOne", StringComparison.InvariantCultureIgnoreCase) == 0));
      }
    }
    [Test]
    public void TestQueryStartingWithKeyWithExplicitWildcard([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);
        kvs.Set("KeyOne", "DataOne");
        kvs.KeyCount().Should().Be(1);
        kvs.KeyExists("KeyOne").Should().BeTrue();
        kvs.Get("KeyOne").Should().Be("DataOne");

        kvs.QueryAllKeyValuesStartingWithKey("Key%").Should()
          .HaveCount(1).And
          .ContainSingle(val =>
            (string.Compare(val.Key, "KeyOne", StringComparison.InvariantCultureIgnoreCase) == 0) &&
            (string.Compare(val.Value, "DataOne", StringComparison.InvariantCultureIgnoreCase) == 0));
      }
    }
    [Test]
    public void TestFetchStartingWithKeyWithoutWildcard([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);
        kvs.Set("KeyOne", "DataOne");
        kvs.KeyCount().Should().Be(1);
        kvs.KeyExists("KeyOne").Should().BeTrue();
        kvs.Get("KeyOne").Should().Be("DataOne");

        kvs.FetchAllKeyValuesStartingWithKey("Key%").Should().HaveCount(1).And
          .ContainSingle(kve => (string.Compare(kve.Key, "KeyOne", StringComparison.InvariantCulture) == 0) &&
            (string.Compare(kve.Value, "DataOne", StringComparison.InvariantCulture)) == 0);
      }
    }
    [Test]
    public void TestFetchStartingWithKeyAndExplicitWildcard([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);
        kvs.Set("KeyOne", "DataOne");
        kvs.KeyCount().Should().Be(1);
        kvs.KeyExists("KeyOne").Should().BeTrue();
        kvs.Get("KeyOne").Should().Be("DataOne");

        kvs.FetchAllKeyValuesStartingWithKey("Key%").Should().HaveCount(1).And
          .ContainSingle(kve => (string.Compare(kve.Key, "KeyOne", StringComparison.InvariantCulture) == 0) &&
            (string.Compare(kve.Value, "DataOne", StringComparison.InvariantCulture)) == 0);
      }
    }
    [Test]
    public void TestQueryAllKeys([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);


        kvs.QueryAllKeys().Should().HaveCount(count);
      }
    }
    [Test]
    public void TestFetchAllKeys([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        kvs.FetchAllKeys().Should().HaveCount(count);
      }
    }
    [Test]
    public void TestEnumerableSet([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        const int count = 10;
        var enumerable = GenerateData(count).ToArray();
        enumerable.Should().HaveCount(count);

        kvs.Set(enumerable);
        kvs.KeyCount().Should().Be(count);

        //        var keyValues = kvs.Get(dictionary.Keys);
        //        keyValues.Should().HaveCount(count).And.ContainKeys(dictionary.Keys, "").And.ContainValues(dictionary.Values);
      }
    }
    [Test]
    public void TestClearWithKeyPrefix([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        foreach (var kvi in GenerateData(count, "Foo"))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(2 * count);

        kvs.QueryAllKeys().Should().HaveCount(2 * count);

        kvs.ClearAllKeyValuesStartingWithKey("Foo");
        kvs.KeyCount().Should().Be(count);

        kvs.QueryAllKeys().Should().HaveCount(count);
      }
    }
    [Test]
    public void TestClearAllDocuments([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        foreach (var kvi in GenerateData(count, "Foo"))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(2 * count);

        kvs.QueryAllKeys().Should().HaveCount(2 * count);

        kvs.ClearAllDocuments();
        kvs.KeyCount().Should().Be(0);

        kvs.QueryAllKeys().Should().HaveCount(0);

        //  verify the table still works
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);
        kvs.QueryAllKeys().Should().HaveCount(count);
      }
    }
    [Test]
    public void TestPreventClearAllDocumentsThrows([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      { //  we set preventClearAll: true to ensure we throw on clear all
        Initialize(memoryDb, kvs, preventClearAll: true);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        foreach (var kvi in GenerateData(count, "Foo"))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(2 * count);

        kvs.QueryAllKeys().Should().HaveCount(2 * count);

        Assert.Throws<InvalidOperationException>(() => kvs.ClearAllDocuments());

      }
    }
    [Test]
    public void TestKeyNotFoundThrowsAnExceptionWhenSet([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      { //  we set preventClearAll: true to ensure we throw on clear all
        Initialize(memoryDb, kvs, throwOnKeyNotFound: true);
        kvs.Open();

        Assert.Throws<KeyNotFoundException>(() =>
        {
          var data = kvs.Get("NotAKey");
          data.Should().BeNull();
        });
      }
    }
    [Test]
    public void TestKeyNotFoundReturnsNullWhenNotSet([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      { //  we set preventClearAll: true to ensure we throw on clear all
        Initialize(memoryDb, kvs, throwOnKeyNotFound: false);
        kvs.Open();

        var data = kvs.Get("NotAKey");
        data.Should().BeNull();
      }
    }
    [Test]
    public void TestTransactionDisposalRollsback([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        using (var txn = kvs.CreateTransaction())
        {
          foreach (var kvi in GenerateData(count, "Foo"))
            kvs.Set(kvi.Key, kvi.Value);

          kvs.KeyCount().Should().Be(2 * count);
        }
        kvs.QueryAllKeys().Should().HaveCount(count);
      }
    }
    [Test]
    public void TestTransactionCommit([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        using (var txn = kvs.CreateTransaction())
        {
          foreach (var kvi in GenerateData(count, "Foo"))
            kvs.Set(kvi.Key, kvi.Value);

          kvs.KeyCount().Should().Be(2 * count);
          txn.Commit();
        }
        kvs.QueryAllKeys().Should().HaveCount(2 * count);
      }
    }
    [Test]
    public void TestTransactionCommitReopensTxn([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        using (var txn = kvs.CreateTransaction())
        {
          foreach (var kvi in GenerateData(count, "Foo"))
            kvs.Set(kvi.Key, kvi.Value);

          kvs.KeyCount().Should().Be(2 * count);
          txn.Commit();

          foreach (var kvi in GenerateData(count, "Bar"))
            kvs.Set(kvi.Key, kvi.Value);

          kvs.KeyCount().Should().Be(3 * count);
          txn.Commit();
        }
        kvs.QueryAllKeys().Should().HaveCount(3 * count);
      }
    }
    [Test]
    public void TestTransactionRollback([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        using (var txn = kvs.CreateTransaction())
        {
          foreach (var kvi in GenerateData(count, "Foo"))
            kvs.Set(kvi.Key, kvi.Value);

          kvs.KeyCount().Should().Be(2 * count);
          txn.Rollback();
        }
        kvs.QueryAllKeys().Should().HaveCount(count);
      }
    }
    [Test]
    public void TestTransactionRollbackReopensTxn([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        using (var txn = kvs.CreateTransaction())
        {
          foreach (var kvi in GenerateData(count, "Foo"))
            kvs.Set(kvi.Key, kvi.Value);

          kvs.KeyCount().Should().Be(2 * count);
          txn.Rollback();

          foreach (var kvi in GenerateData(count, "Bar"))
            kvs.Set(kvi.Key, kvi.Value);

          kvs.KeyCount().Should().Be(2 * count);
          txn.Commit();
        }
        kvs.QueryAllKeys().Should().HaveCount(2 * count);
      }
    }
    [Test]
    public void TestTransactionRollbackDisposesWithRollback([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        using (var txn = kvs.CreateTransaction())
        {
          foreach (var kvi in GenerateData(count, "Foo"))
            kvs.Set(kvi.Key, kvi.Value);

          kvs.KeyCount().Should().Be(2 * count);
          txn.Rollback();

          foreach (var kvi in GenerateData(count, "Bar"))
            kvs.Set(kvi.Key, kvi.Value);

          kvs.KeyCount().Should().Be(2 * count);
        }
        kvs.QueryAllKeys().Should().HaveCount(count);
      }
    }
    [Test]
    public void TestTransactionRollsbackWithThrow([Values(false, true)] bool memoryDb)
    {
      using (var kvs = new KeyValueStore())
      {
        Initialize(memoryDb, kvs);
        kvs.Open();

        //  new database should have no keys
        kvs.KeyCount().Should().Be(0);

        var count = 10;
        foreach (var kvi in GenerateData(count))
          kvs.Set(kvi.Key, kvi.Value);
        kvs.KeyCount().Should().Be(count);

        try
        {
          using (var txn = kvs.CreateTransaction())
          {
            foreach (var kvi in GenerateData(count, "Foo"))
              kvs.Set(kvi.Key, kvi.Value);

            kvs.KeyCount().Should().Be(2 * count);
            throw new ArgumentException();
          }
        }
        catch (Exception)
        { //  ignore the exception
        }
        kvs.QueryAllKeys().Should().HaveCount(count);
      }
    }

    #region Private Methods
    private IEnumerable<KeyValueElement> GenerateData(int count, string prefix = "Key")
    {
      for (var idx = 0; idx < count; ++idx)
      {
        var key = string.Format("{0}/{1}", prefix, idx);
        var data = string.Format("Data Item for {0}", key);
        yield return new KeyValueElement { Key = key, Value = data };
      }
    }
    private static void Initialize(bool memoryDb, KeyValueStore kvs, bool preventClearAll = false, bool throwOnKeyNotFound = false)
    {
      if (memoryDb)
      {
        kvs.Initialize(options =>
        {
          options.InMemory = true;
          options.ThrowOnClearAll = preventClearAll;
          options.ThrowOnGetKeyNotFound = throwOnKeyNotFound;
        });
      }
      else
      {
        var dbName = Path.GetTempFileName();
        File.Exists(dbName).Should().BeTrue();
        kvs.Initialize(options =>
        {
          options.DatabaseName = dbName;
          options.ThrowOnClearAll = preventClearAll;
          options.ThrowOnGetKeyNotFound = throwOnKeyNotFound;
        });
      }
    }
    #endregion
  }
}
