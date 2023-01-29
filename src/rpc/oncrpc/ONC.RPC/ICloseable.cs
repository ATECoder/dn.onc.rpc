namespace cc.isr.ONC.RPC
{
    internal interface ICloseable : IDisposable
    {

        /// <summary>
        /// Closes the connection to an ONC/RPC server and frees all network-related resources.
        /// </summary>
        /// <remarks> This implementation of close and dispose follows the implementation of
        /// the <see cref="System.Net.Sockets.TcpClient"/> at
        /// <see href="https://github.com/microsoft/referencesource/blob/master/System/net/System/Net/Sockets/TCPClient.cs"/>
        /// with the following modifications:
        /// <list type="bullet"> <item>
        /// <see cref="Close()"/> is not <see langword="virtual"/> </item><item>
        /// <see cref="Close()"/> calls <see cref="IDisposable.Dispose()"/> </item><item>
        /// Consequently, <see cref="Close()"/> need not be overridden. </item><item>
        /// <see cref="Close()"/> does not hide any exception that might be thrown by <see cref="IDisposable.Dispose()"/> </item></list>
        /// <list type="bullet"> <item>
        /// The <see cref="IDisposable.Dispose()"/> method skips if <see cref="ICloseable.IsDisposed"/> is <see langword="true"/>; </item><item>
        /// The <see cref="XdrEncodingStreamBase.Dispose(bool)"/> accumulates and throws an aggregate exception </item><item>
        /// The <see cref="IDisposable.Dispose()"/> method throws the aggregate exception from <see cref="XdrEncodingStreamBase.Dispose(bool)"/>. </item></list>
        /// </remarks>
        void Close();

        /// <summary>   Gets or sets a value indicating whether this object is disposed. </summary>
        /// <value> True if this object is disposed, false if not. </value>
        bool IsDisposed { get; }

    }
}
