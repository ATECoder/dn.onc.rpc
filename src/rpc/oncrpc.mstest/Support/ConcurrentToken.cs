using System.Diagnostics;

namespace cc.isr.ONC.RPC.MSTest.Support;

/// <summary> A Thread safe token. </summary>
/// <remarks>
/// (c) 2017 Integrated Scientific Resources, Inc. All rights reserved.<para>
/// Licensed under The MIT License.</para><para>
/// David, 2017-03-29 </para>
/// </remarks>
[DebuggerDisplay( "Value = {Value}" )]
public class ConcurrentToken<T> : IDisposable
{

    #region " CONSTRUCTOR and CLEANUP "

    /// <summary> Initializes a new instance of the <see cref="T:System.Object" /> class. </summary>
    /// <remarks> David, 2020-08-20. </remarks>
    public ConcurrentToken() : base()
    {
        this._slimLock = new ReaderWriterLockSlim();
    }

    /// <summary> Constructor. </summary>
    /// <remarks> David, 2020-08-20. </remarks>
    /// <param name="value"> The value. </param>
    public ConcurrentToken( T value ) : this()
    {
        this._value = value;
    }

    /// <summary> Gets or sets the sentinel to detect redundant calls. </summary>
    /// <value> The sentinel to detect redundant calls. </value>
    protected bool IsDisposed { get; private set; }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <remarks> 
    /// Takes account of and updates <see cref="IsDisposed"/>.
    /// Encloses <see cref="Dispose(bool)"/> within a try...finaly block.
    /// </remarks>
    public void Dispose()
    {
        if ( this.IsDisposed ) { return; }
        try
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            this.Dispose( true );

            // uncomment the following line if Finalize() is overridden above.
            GC.SuppressFinalize( this );
        }
        catch ( Exception ex ) { Console.WriteLine( ex.ToString() ); }
        finally
        {
            this.IsDisposed = true;
        }

    }

    /// <summary>
    /// Releases the unmanaged resources used by the isr.Std.Models.ThreadSafeToken{T} and
    /// optionally releases the managed resources.
    /// </summary>
    /// <remarks> David, 2020-09-22. </remarks>
    /// <param name="disposing"> True to release both managed and unmanaged resources; false to
    /// release only unmanaged resources. </param>
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // dispose managed state (managed objects)
        }

        // free unmanaged resources and override finalizer
        // i am assuming the slim lock depends on unmanaged resources.
        this._slimLock?.Dispose();

        // set large fields to null
    }

    /// <summary>
    /// This destructor will Runs only if the Dispose method does not get called. It gives the base
    /// class the opportunity to finalize. Do not provide destructors in types derived from this
    /// class.
    /// </summary>
    /// <remarks> David, 2020-09-22. </remarks>
    ~ConcurrentToken()
    {
        if ( this.IsDisposed ) { return; }
        // Do not re-create Dispose clean-up code here.
        // Calling Dispose(false) is optimal for readability and maintainability.
        this.Dispose( false );
    }

    #endregion

    /// <summary> The slim lock. </summary>
    private readonly ReaderWriterLockSlim _slimLock;

    /// <summary> The value. </summary>
    private T? _value;

    /// <summary> Gets or sets the value. </summary>
    /// <value> The value. </value>
    public T? Value
    {
        get {
            this._slimLock.EnterReadLock();
            try
            {
                return this._value;
            }
            finally
            {
                this._slimLock.ExitReadLock();
            }
        }

        set {
            this._slimLock.EnterWriteLock();
            try
            {
                this._value = value;
            }
            finally
            {
                this._slimLock.ExitWriteLock();
            }
        }
    }
}
