using AsyncClientServerLib.Message;
using SocketServerLib.Message;
using SocketServerLib.SocketHandler;
using System.Net.Security;
using System.Net.Sockets;

namespace AsyncClientServerLib.SocketHandler
{
    /// <summary>
    /// Basic Socket Client Handler. Implements the AbstractAsyncTcpSocketClientHandler class. The message class is BasicMessage.
    /// </summary>
    internal class AsyncSocketClientHandler : AbstractAsyncTcpSocketClientHandler
    {
        /// <summary>
        /// The constructor for SSL connection.
        /// </summary>
        /// <param name="handler">The socket cient handler</param>
        /// <param name="stream">The ssl stream</param>
        public AsyncSocketClientHandler(Socket handler, SslStream stream)
            : base(handler, stream, false)
        {
        }

        /// <summary>
        /// The constructor for not SSL connection.
        /// </summary>
        /// <param name="handler">The socket cient handler</param>
        public AsyncSocketClientHandler(Socket handler)
            : base(handler, null, false)
        {
        }

        /// <summary>
        /// Return a BasicMessage empty instance.
        /// </summary>
        /// <returns>The BasicMessage instance</returns>
        protected override AbstractMessage GetMessageInstance()
        {
            return new BasicMessage();
        }
    }
}
