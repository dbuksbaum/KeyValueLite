using System;
using SQLite;

namespace KeyValueLite
{
  /// <summary>
  /// The core data structure use to store items in an <see cref="IKeyValueStore"/>.
  /// </summary>
  [Table("key_value")]
  public class KeyValueElement
  {
    [PrimaryKey]
    [MaxLength(255), Collation("NOCASE")]
    public string Key { get; set; }
    [MaxLength(Int32.MaxValue)]
    public string Value { get; set; }
    [Indexed]
    public DateTime LastUpdateTime { get; set; }
    //    [SQLite.Indexed]
    //    public string TypeName { get; set; }
  }
}