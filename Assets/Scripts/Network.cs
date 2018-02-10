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
	private static string data = null;
	public static LinkedList<string> ReceiveQueue = new LinkedList<string>(); 
	public static LinkedList<string> SendQueue = new LinkedList<string>();
	public bool isServer = true;
	public string serverIP = "";
	public Thread thread = null;
	public Thread receive = null;
	public Thread send = null;
	public bool networkActive = true;

	void Start() {
		if (isServer) {
			thread = new Thread (ServerStartListening);
		} else {
			thread = new Thread (ClientStart);
			StartCoroutine (TestMessage("Testing"));
		}
		thread.Start ();
	}

	IEnumerator TestMessage(string message) {
		yield return new WaitForSeconds (2);
		SendQueue.AddLast (message);
		yield return null;
	}

	void ServerStartListening() {
		TcpListener listener;
		// Get IP address
		IPAddress ipAddress = IPAddress.Parse(serverIP);  
		// Create a listener which will spawn a send.  
		listener = new TcpListener(ipAddress, 11000);  
		listener.Start();
		// Start listening for connections.  
		Debug.Log("Waiting for a read connection...");  
		while (networkActive) {  
			if (listener.Pending()) {
				Debug.Log("Established read connection...");
				Socket socket = listener.AcceptSocket();
				// Spawn the send thread
				Thread send = new Thread(() => Send(socket));
				send.Start();
				SendQueue.AddLast ("connect");
				listener.Stop();
				break;
			}
		}
		// Create a listener which will spawn a write.  
		listener = new TcpListener(ipAddress, 11001);  
		listener.Start();
		// Start listening for connections.  
		Debug.Log("Waiting for a write connection...");  
		while (networkActive) {  
			if (listener.Pending()) {
				Debug.Log("Established write connection...");
				Socket socket = listener.AcceptSocket();
				// Spawn the write thread
				Thread receive = new Thread(() => Receive(socket));
				receive.Start();
				listener.Stop();
				break;
			}
		}
	}

	void Send(Socket socket) {
		// An incoming connection needs to be processed.  
		while (networkActive) {  
			if (SendQueue.Count > 0) {  
				byte[] msg = Encoding.ASCII.GetBytes(SendQueue.First.Value+"!"); 
				socket.Send(msg); 
				SendQueue.RemoveFirst ();
			}
		}
		socket.Close ();
	}

	void Receive(Socket socket) {
		// Data buffer for incoming data.  
		byte[] bytes = new Byte[1024];  
		socket.ReceiveTimeout = 100;
		data = null;  
		// An incoming connection needs to be processed.  
		while (networkActive) {  
			bytes = new byte[1024];  
			int bytesRec = socket.Receive(bytes);  
			data += Encoding.ASCII.GetString(bytes,0,bytesRec);  
			if (data.IndexOf("!") > -1) {  
				Debug.Log(string.Format("Text received : {0}", data.Substring(0, data.Length - 1)));  
				ReceiveQueue.AddLast(data.Substring(0, data.Length - 1));  
			}
		}
		socket.Close ();
	}

	void ClientStart() {  
		// Data buffer for incoming data.  
		byte[] bytes = new Byte[128];  

		// Connect to a remote device.  
		// Establish the remote endpoint for the socket.  
		// This example uses port 11000 on the local computer.  
		IPAddress ipAddress = IPAddress.Parse(serverIP);  
		IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);  

		// Create a TCP/IP  socket.  
		Socket receiver = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp );  

		// Connect the socket to the remote endpoint. Catch any errors.  
		receiver.Connect(remoteEP); 
		Debug.Log(string.Format("Socket connected to {0}", receiver.RemoteEndPoint.ToString()));  

		// Receive the response from the remote device.  
		int bytesRec = receiver.Receive(bytes);  
		if (Encoding.ASCII.GetString (bytes, 0, bytesRec) == "connect!") {
			ipAddress = IPAddress.Parse (serverIP);  
			remoteEP = new IPEndPoint (ipAddress, 11000);  

			// Create a TCP/IP  socket.  
			Socket sender = new Socket (ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  

			// Connect the socket to the remote endpoint. Catch any errors.  
			sender.Connect (remoteEP);
			// Start up the recieve and sender threads
			Thread send = new Thread (() => Send (receiver));
			send.Start ();
			Thread receive = new Thread (() => Send (sender));
			receive.Start();
		} else {
			// Release the socket.  
			receiver.Shutdown(SocketShutdown.Both);  
			receiver.Close();
		}
	}

	void OnApplicationQuit() {
		if (thread != null) {
			networkActive = false;
		}
	}
}
