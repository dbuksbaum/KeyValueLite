using System;

namespace KeyValueLite
{
  /// <summary>
  /// Enhancement to the standard IDisposable interface to allow for the 
  /// state of disposal to be queried.
  /// </summary>
  public interface IDisposable2 : IDisposable
  {
    /// <summary>
    /// Gets a value indicating whether this instance is disposed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
    /// </value>
    bool IsDisposed { get; }
  }
}