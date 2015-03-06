using SQLite;

namespace KeyValueLite.Provider
{
  internal class SqliteTransaction : AbstractDisposable, ISqliteTransaction
  {
    #region Fields
    private SQLiteConnection _dbConnection;
    #endregion

    #region Constructors
    public SqliteTransaction(SQLiteConnection dbConnection)
    {
      _dbConnection = dbConnection;
      _dbConnection.BeginTransaction();
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
      if ((_dbConnection != null) && (_dbConnection.IsInTransaction))
        _dbConnection.Rollback();

      _dbConnection = null;
    }
    #endregion

    #region Implementation of ISqliteTransaction
    public void Commit()
    {
      _dbConnection.Commit();
      _dbConnection.BeginTransaction();
    }

    public void Rollback()
    {
      _dbConnection.Rollback();
      _dbConnection.BeginTransaction();
    }
    #endregion
  }
}