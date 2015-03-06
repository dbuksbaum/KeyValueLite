using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValueLite.Provider
{
  internal static partial class SqlStatements
  {
    #region Pragmas
    public const string PragmaGet = "PRAGMA {0};";
    public const string PragmaGetWithParam = "PRAGMA {0}({1});";
    public const string PragmaUserVersion = "PRAGMA user_version={0};";
    public const string PragmaEncodingUTF16 = "PRAGMA encoding = \"UTF-16\"";
    public const string PragmaEncodingUTF8 = "PRAGMA encoding = \"UTF-16\"";
    #endregion

    #region KeyValue Table
    //  IF NOT EXISTS 
    public const string CheckIfTableExists = "SELECT name FROM sqlite_master WHERE type='table' AND name=?;";
    public const string CountKeyValues = "SELECT COUNT(*) FROM [{0}];";
    public const string CountKeyValuesByKey = "SELECT COUNT(*) FROM [{0}] WHERE [key]=?;";
    public const string SelectKeyValueValuesStartingWithKey = "SELECT [key], [value] FROM [{0}] WHERE [key] LIKE ?;";
    public const string DeleteKeyValueValuesStartingWithKey = "DELETE FROM [{0}] WHERE [key] LIKE ?;";
    public const string QueryKeyValueKeys = "SELECT [key] FROM [{0}];";
    #endregion
  }
}
