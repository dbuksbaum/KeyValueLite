namespace KeyValueLite
{
  /// <summary>
  /// Used for the result of an integrity checki with 
  /// the underlying SQLite data store.
  /// <see cref="http://www.sqlite.org/pragma.html#pragma_integrity_check"/>
  /// </summary>
  internal class IntegrityCheck
  {
    [SQLite.Column("integrity_check")]
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public string Result { get; set; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
  }
}