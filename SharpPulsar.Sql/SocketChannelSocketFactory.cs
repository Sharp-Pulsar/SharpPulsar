/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Net.Sockets;

namespace SharpPulsar.Sql
{


	/// <summary>
	/// Workaround for JDK IPv6 bug on Mac. Sockets created with the basic socket
	/// API often cannot connect to IPv6 destinations due to JDK-8131133. However,
	/// NIO sockets do not have this problem, even if used in blocking mode.
	/// </summary>
	public class SocketChannelSocketFactory : SocketFactory
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.net.Socket createSocket() throws java.io.IOException
		public override Socket CreateSocket()
		{
			return SocketChannel.open().socket();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.net.Socket createSocket(String host, int port) throws java.io.IOException
		public override Socket CreateSocket(string Host, int Port)
		{
			return SocketChannel.open(new InetSocketAddress(Host, Port)).socket();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.net.Socket createSocket(String host, int port, java.net.InetAddress localAddress, int localPort) throws java.io.IOException
		public override Socket CreateSocket(string Host, int Port, InetAddress LocalAddress, int LocalPort)
		{
			throw new SocketException("not supported");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.net.Socket createSocket(java.net.InetAddress address, int port) throws java.io.IOException
		public override Socket CreateSocket(InetAddress Address, int Port)
		{
			return SocketChannel.open(new InetSocketAddress(Address, Port)).socket();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.net.Socket createSocket(java.net.InetAddress address, int port, java.net.InetAddress localAddress, int localPort) throws java.io.IOException
		public override Socket CreateSocket(InetAddress Address, int Port, InetAddress LocalAddress, int LocalPort)
		{
			throw new SocketException("not supported");
		}
	}

}