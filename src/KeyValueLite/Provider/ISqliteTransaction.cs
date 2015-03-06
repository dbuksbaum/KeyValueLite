using System;

namespace KeyValueLite.Provider
{
  /// <summary>
  /// Implemented to support transactions in SQLite
  /// </summary>
  public interface ISqliteTransaction : IDisposable
  {
    void Commit();
    void Rollback();
  }
}