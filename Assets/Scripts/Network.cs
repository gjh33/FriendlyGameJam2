using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Net.Sockets;  
using System.Text;  
using UnityEngine;

public class Network : MonoBehaviour {

	// Incoming data from the client.  
	public static string data = null;
	public bool isServer = true;
	public Thread thread = null;

	void Start() {
		if (isServer) {
			thread = new Thread (ServerStartListening);
		} else {
			thread = new Thread (ClientStart);
		}
		thread.Start ();
	}

	void ServerStartListening() {
		// Data buffer for incoming data.  
		byte[] bytes = new Byte[1024];  

		// Establish the local endpoint for the socket.  
		// Dns.GetHostName returns the name of the   
		// host running the application.  
		IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());  
		IPAddress ipAddress = ipHostInfo.AddressList[0];  
		IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);  

		// Create a TCP/IP socket.  
		Socket listener = new Socket(ipAddress.AddressFamily,  
			SocketType.Stream, ProtocolType.Tcp );  

		// Bind the socket to the local endpoint and   
		// listen for incoming connections.  
		try {  
			listener.Bind(localEndPoint);  
			listener.Listen(1);  

			// Start listening for connections.  
			while (true) {  
				Console.WriteLine("Waiting for a connection...");  
				// Program is suspended while waiting for an incoming connection.  
				Socket handler = listener.Accept();  
				data = null;  

				// An incoming connection needs to be processed.  
				while (true) {  
					bytes = new byte[1024];  
					int bytesRec = handler.Receive(bytes);  
					data += Encoding.ASCII.GetString(bytes,0,bytesRec);  
					if (data.IndexOf("<EOF>") > -1) {  
						break;  
					} 
				}  

				// Show the data on the console.  
				Console.WriteLine( "Text received : {0}", data);  

				// Echo the data back to the client.  
				byte[] msg = Encoding.ASCII.GetBytes(data);  

				handler.Send(msg);  
				handler.Shutdown(SocketShutdown.Both);  
				handler.Close();
			}  

		} catch (Exception e) {  
			Debug.LogWarning(e.ToString());  
		}  
	}  

	void ClientStart() {  
		// Data buffer for incoming data.  
		byte[] bytes = new byte[1024];  

		// Connect to a remote device.  
		try {  
			// Establish the remote endpoint for the socket.  
			// This example uses port 11000 on the local computer.  
			IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());  
			IPAddress ipAddress = ipHostInfo.AddressList[0];  
			IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);  

			// Create a TCP/IP  socket.  
			Socket sender = new Socket(ipAddress.AddressFamily,   
				SocketType.Stream, ProtocolType.Tcp );  

			// Connect the socket to the remote endpoint. Catch any errors.  
			try {  
				sender.Connect(remoteEP);  

				Console.WriteLine("Socket connected to {0}",  
					sender.RemoteEndPoint.ToString());  

				// Encode the data string into a byte array.  
				byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");  

				// Send the data through the socket.  
				sender.Send(msg);  

				// Receive the response from the remote device.  
				int bytesRec = sender.Receive(bytes);  
				Console.WriteLine("Echoed test = {0}",  
					Encoding.ASCII.GetString(bytes,0,bytesRec));  

				// Release the socket.  
				sender.Shutdown(SocketShutdown.Both);  
				sender.Close();  

			} catch (ArgumentNullException ane) {  
				Console.WriteLine("ArgumentNullException : {0}",ane.ToString());  
			} catch (SocketException se) {  
				Console.WriteLine("SocketException : {0}",se.ToString());  
			} catch (Exception e) {  
				Console.WriteLine("Unexpected exception : {0}", e.ToString());  
			}  

		} catch (Exception e) {  
			Console.WriteLine( e.ToString());  
		}  
	}
}
