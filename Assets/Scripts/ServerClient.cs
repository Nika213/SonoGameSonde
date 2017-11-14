using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ServerClient : MonoBehaviour {

	public bool isAtStartup = true;
	public Button serverButton;
	public Button clientButton;
	public Button sendButton;
	public Text textField;
	public const short MSG_ID = 213;
	Vector3 scorePos = new Vector3(12,0,0);
    
	NetworkClient myClient;
	
	void Start()
	{
		serverButton = serverButton.GetComponent<Button>();
		serverButton.onClick.AddListener(SetupServer);
		
		clientButton = clientButton.GetComponent<Button>();
		clientButton.onClick.AddListener(SetupClient);
		
		sendButton = sendButton.GetComponent<Button>();
		sendButton.onClick.AddListener(SendMessage);
		
	}
	

	public struct ScoreMessage
	{
		public Vector3 scorePos;
	}

	// Create a server and listen on a port
	public void SetupServer()
	{
		NetworkServer.Listen(8080);
		textField.text = "Server listen";	
	}

	public void SendMessage()
	{
		StringMessage myMessage = new StringMessage ();
		myMessage.value = "test:" + scorePos;
		NetworkServer.SendToAll(MSG_ID,myMessage);
		textField.text = "Message send";	
	}
    
	// Create a client and connect to the server port
	public void SetupClient()
	{
		myClient = new NetworkClient();
		myClient.RegisterHandler(MsgType.Connect, OnConnected);     
		myClient.Connect("192.168.178.44", 8080);
		myClient.RegisterHandler(MSG_ID,ReceiveMessage);
	}
    
	// client function
	public void OnConnected(NetworkMessage netMsg)
	{
		Debug.Log("Connected to server");
		textField.text = "Connected to server";
	}
	
	private void ReceiveMessage(NetworkMessage message)
	{
		//reading message
		string text = message.ReadMessage<StringMessage> ().value;
		textField.text = text;
	}
}
