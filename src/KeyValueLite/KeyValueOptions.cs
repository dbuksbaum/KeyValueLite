namespace KeyValueLite
{
  /// <summary>
  /// Implementation of a <see cref="IKeyValueOptions"/>.
  /// </summary>
  internal class KeyValueOptions : IKeyValueOptions
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public KeyValueOptions()
    {
      RegisterProvider = true;
      UseUtf16 = false;
      InMemory = false;
      DeleteExisting = false;
      CreateIfMissing = true;
      VerifyOnOpen = true;
      QuickVerify = false;
      ThrowOnClearAll = false;
      ThrowOnGetKeyNotFound = false;
    }
    #endregion

    #region Implementation of IKeyValueOptions
    /// <summary>
    /// Name of the database file
    /// </summary>
    public string DatabaseName { get; set; }
    /// <summary>
    /// Register the Sqlite provider with the ADO.NET configuration automatically
    /// <remarks>Default: true</remarks>
    /// </summary>
    public bool RegisterProvider { get; set; }
    /// <summary>
    /// Is the database Utf16 or Utf8. 
    /// This is only used when creating a new database.
    /// <remarks>Default: false</remarks>
    /// </summary>
    public bool UseUtf16 { get; set; }
    /// <summary>
    /// Opens an in-memory only database.
    /// <remarks>Default: false</remarks>
    /// </summary>
    public bool InMemory { get; set; }
    /// <summary>
    /// Deletes existing file if it exists
    /// <remarks>Default: false</remarks>
    /// </summary>
    public bool DeleteExisting { get; set; }
    /// <summary>
    /// Creates a new database file if it does not exist
    /// <remarks>Default: true</remarks>
    /// </summary>
    public bool CreateIfMissing { get; set; }
    /// <summary>
    /// Run an intergrity check on the database on open
    /// <remarks>Default: true</remarks>
    /// </summary>
    public bool VerifyOnOpen { get; set; }
    /// <summary>
    /// Run a quick intergrity check if an integrity check is run
    /// <remarks>Default: false</remarks>
    /// </summary>
    public bool QuickVerify { get; set; }
    /// <summary>
    /// Throw an exception when attempting to clear all documents
    /// <remarks>Default: false</remarks>
    /// </summary>
    public bool ThrowOnClearAll { get; set; }
    /// <summary>
    /// Throw an exception instead of returning null when Get does not find an element.
    /// <remarks>Default: false</remarks>
    /// </summary>
    public bool ThrowOnGetKeyNotFound { get; set; }
    #endregion
  }
}