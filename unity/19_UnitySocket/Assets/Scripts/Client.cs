using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections;

public class Client : MonoBehaviour {

	const int READ_BUFFER_SIZE = 255;
	const int PORT_NUM = 28000;
	const String IP = "localhost";

	private TcpClient client;
	private byte[] readBuffer = new byte[READ_BUFFER_SIZE];
	public ArrayList lstUsers=new ArrayList();
	public string strMessage=string.Empty;
	public string res=String.Empty;
	private string pUserName;

	string lastMessage;
	public Transform PlayerCoord;
	
	//public Connector(){}
	/*
	void Start () 
	{
		Debug.Log(fnConnectResult(IP,PORT_NUM,System.Environment.MachineName));
		if (res !="")
		{
			Debug.Log(res);
		}
		
	}
*/
	void Update () 
	{
		if (Input.GetKeyDown ("space")) 
		{
			Debug.Log ("space key was pressed");
			fnPacketTest("space key was pressed");
		}
		
		if (Input.GetKeyDown ("escape")) 
		{
			Debug.Log ("escape key was pressed");
			fnPacketTest("escape key was pressed");
		}
		if (strMessage !="JOIN")
		{
			if (res != lastMessage)
			{
				Debug.Log(res);
				lastMessage=res;
			}
		}
		//fnPacketTest(PlayerCoord.position[0]+","+PlayerCoord.position[1]+","+PlayerCoord.position[2]);
	}

	void Awake(){
		fnConnectResult(IP,PORT_NUM,System.Environment.MachineName);
	}

	public string fnConnectResult(string sNetIP, int iPORT_NUM,string sUserName)
	{
		try 
		{
			pUserName=sUserName;
			// The TcpClient is a subclass of Socket, providing higher level 
			// functionality like streaming.
			client = new TcpClient(sNetIP, PORT_NUM);
			// Start an asynchronous read invoking DoRead to avoid lagging the user
			// interface.
			client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoRead), null);
			// Make sure the window is showing before popping up connection dialog.
			
			AttemptLogin(sUserName);
			return "Connection Succeeded";
		} 
		catch(Exception ex)
		{
			return "Server is not active.  Please start server and try again.      " + ex.ToString();
		}
	}

	public void AttemptLogin(string user)
	{
		SendData("CONNECT|"+ user);
	}
	
	public void fnPacketTest(string sInfo)
	{
		SendData("CHAT|" + sInfo);
	}
	
	public void fnDisconnect()
	{
		SendData("DISCONNECT");
	}
	
	public void fnListUsers()
	{
		SendData("REQUESTUSERS");
	}
	
	private void DoRead(IAsyncResult ar)
	{ 
		int BytesRead;
		try
		{
			// Finish asynchronous read into readBuffer and return number of bytes read.
			BytesRead = client.GetStream().EndRead(ar);
			if (BytesRead < 1) 
			{
				// if no bytes were read server has close.  
				res="Disconnected";
				return;
			}
			// Convert the byte array the message was saved into, minus two for the
			// Chr(13) and Chr(10)
			strMessage = Encoding.ASCII.GetString(readBuffer, 0, BytesRead - 2);
			ProcessCommands(strMessage);
			// Start a new asynchronous read into readBuffer.
			client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoRead), null);
			
		} 
		catch
		{
			res="Disconnected";
		}
	}
	
	// Process the command received from the server, and take appropriate action.
	private void ProcessCommands(string strMessage)
	{
		string[] dataArray;
		
		// Message parts are divided by "|"  Break the string into an array accordingly.
		dataArray = strMessage.Split((char) 124);
		// dataArray(0) is the command.
		switch( dataArray[0])
		{
		case "JOIN":
			// Server acknowledged login.
			res= "You have joined the chat";
			break;
		case "CHAT":
			// Received chat message, display it.
			res=  dataArray[1].ToString();
			break;
		case "REFUSE":
			// Server refused login with this user name, try to log in with another.
			AttemptLogin(pUserName);
			res=  "Attempted Re-Login";
			break;
		case "LISTUSERS":
			// Server sent a list of users.
			ListUsers(dataArray);
			break;
		case "BROAD":
			// Server sent a broadcast message
			res=  "ServerMessage: " + dataArray[1].ToString();
			break;
		}
	}
	
	// Use a StreamWriter to send a message to server.
	private void SendData(string data)
	{
		StreamWriter writer = new StreamWriter(client.GetStream());
		writer.Write(data + (char) 13);
		writer.Flush();
	}
	
	private void ListUsers(string[] users)
	{
		int I;
		lstUsers.Clear();
		for (I = 1; I <= (users.Length - 1); I++)
		{
			lstUsers.Add(users[I]);	
		}
	}
}
