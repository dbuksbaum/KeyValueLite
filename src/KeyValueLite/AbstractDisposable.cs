using System;
using System.Diagnostics;

namespace KeyValueLite
{
  /// <summary>
  /// Abstract class that implements <see cref="IDisposable2"/> and <see cref="T:System.IDisposable"/>.
  /// Classes that need disposable behavior should just derive from this class and implement the
  /// DisposeUnmanagedResources and DisposeManagedResources methods. These methods will be called at 
  /// the proper time in the disposal process.
  /// </summary>
  public abstract class AbstractDisposable : IDisposable2
  {
    #region Properties
    /// <summary>
    /// Gets a value indicating whether this instance is disposed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
    /// </value>
    public bool IsDisposed { get; private set; }
    #endregion

    #region Finalizer
    //  NOTE: I am not sure if I want this to be in the base class
#if false
                ~AbstractDisposable()
                {
                  Dispose(false);
                }
#endif
    #endregion

    #region Dispose Pattern
    /// <summary>
    /// Performs the freeing, releasing, or resetting of unmanaged resources. 
    /// Implemented as per the recommendations on:
    /// http://www.bluebytesoftware.com/blog/2005/04/08/DGUpdateDisposeFinalizationAndResourceManagement.aspx
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), DebuggerStepThrough]
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    /// <summary>
    /// Dispose executes in two distinct scenarios.
    /// If disposing equals true, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// If disposing equals false, the method has been called by the 
    /// runtime from inside the finalizer and you should not reference
    /// other objects. Only unmanaged resources can be disposed.
    /// </summary>
    /// <param name="disposing">False when called from the finalizer, True when Dispose() called explicitly.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        { // Dispose managed resources.
          DisposeManagedResources();
        }

        // There are no unmanaged resources to release, but
        // if we add them, they need to be released here.
        DisposeUnmanagedResources();
      }
      IsDisposed = true;

      //  If available, call the base class's Dispose(Boolean) method
      //  base.Dispose(disposing);
    }
    #endregion

    #region Overridables
    /// <summary>
    /// Disposes the unmanaged resources. This is called at the right time in the disposal process.
    /// </summary>
    protected abstract void DisposeUnmanagedResources();
    /// <summary>
    /// Disposes the managed resources. This is called at the right time in the disposal process.
    /// </summary>
    protected abstract void DisposeManagedResources();
    #endregion

    #region Helper Methods
    /// <summary>
    /// Throws if disposed.
    /// </summary>
    [DebuggerStepThrough]
    protected void ThrowIfDisposed()
    {
      if((null == this) || IsDisposed)
        throw new InvalidOperationException("Object already disposed.");
    }
    #endregion
  }
}