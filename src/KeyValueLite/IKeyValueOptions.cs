using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValueLite
{
  /// <summary>
  /// Implemented to provide options for a <see cref="IKeyValueStore"/>.
  /// </summary>
  public interface IKeyValueOptions
  {
    /// <summary>
    /// Name of the database file
    /// </summary>
    string DatabaseName { get; set; }
    /// <summary>
    /// Register the Sqlite provider with the ADO.NET configuration automatically
    /// <remarks>Default: true</remarks>
    /// </summary>
    //bool RegisterProvider { get; set; }
    /// <summary>
    /// Is the database Utf16 or Utf8. 
    /// This is only used when creating a new database.
    /// <remarks>Default: false</remarks>
    /// </summary>
    bool UseUtf16 { get; set; }
    /// <summary>
    /// Opens an in-memory only database.
    /// <remarks>Default: false</remarks>
    /// </summary>
    bool InMemory { get; set; }
    /// <summary>
    /// Deletes existing file if it exists
    /// <remarks>Default: false</remarks>
    /// </summary>
    bool DeleteExisting { get; set; }
    /// <summary>
    /// Creates a new database file if it does not exist
    /// <remarks>Default: true</remarks>
    /// </summary>
    bool CreateIfMissing { get; set; }
    /// <summary>
    /// Run an intergrity check on the database on open
    /// <remarks>Default: true</remarks>
    /// </summary>
    bool VerifyOnOpen { get; set; }
    /// <summary>
    /// Run a quick intergrity check if an integrity check is run
    /// <remarks>Default: false</remarks>
    /// </summary>
    bool QuickVerify { get; set; }
    /// <summary>
    /// Throw an exception when attempting to clear all documents
    /// <remarks>Default: false</remarks>
    /// </summary>
    bool ThrowOnClearAll { get; set; }
    /// <summary>
    /// Throw an exception instead of returning null when Get does not find an element.
    /// <remarks>Default: false</remarks>
    /// </summary>
    bool ThrowOnGetKeyNotFound { get; set; }
  }
}
