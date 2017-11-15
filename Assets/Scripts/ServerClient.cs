using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

public class ServerClient : MonoBehaviour {

	public bool isAtStartup = true;
	public Button serverButton;
	public Button clientButton;
	public Button sendButton;
	public Button backButton;
	public InputField serverIpInput;
	public Text textField;
	
	public GameObject networkUI;
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
		
		backButton.gameObject.SetActive(false);
		backButton.onClick.AddListener(BackButtonClicked);
	}
	

	public struct ScoreMessage
	{
		public Vector3 scorePos;
	}

	// Create a server and listen on a port
	public void SetupServer()
	{
		NetworkServer.Listen(8080);
		String serverIp = Network.player.ipAddress;
		textField.text = "Server listen on IP: " + serverIp;
		networkUI.SetActive(false);
		backButton.gameObject.SetActive(true);
		
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
		String serverIp = serverIpInput.text;
		if (serverIp.Length == 0)
		{
			textField.text = "ServerIP eingeben!";
		}
		else
		{
			myClient = new NetworkClient();
			myClient.RegisterHandler(MsgType.Connect, OnConnected);     
			myClient.RegisterHandler(MsgType.Disconnect, OnDisconnected);     
			myClient.Connect(serverIp, 8080);
			myClient.RegisterHandler(MSG_ID, ReceiveMessage);
		}
	}
    
	// client function
	public void OnConnected(NetworkMessage message)
	{
		Debug.Log("Connected to server");
		textField.text = "Connected to server";
		ChangeNetworkUiVisibility(false);
		Camera.main.clearFlags = CameraClearFlags.Skybox;
	}

	public void OnDisconnected(NetworkMessage message)
	{
		Debug.Log("Disconnected from server");
		textField.text = "Disconnected from server";
		ChangeNetworkUiVisibility(true);
		Camera.main.clearFlags = CameraClearFlags.SolidColor;
		myClient = null;
	}

	private void Disconnect()
	{
		Debug.Log("Disconnect client");
		textField.text = "Disconnected from server";
		ChangeNetworkUiVisibility(true);
		Camera.main.clearFlags = CameraClearFlags.SolidColor;
		myClient.Disconnect();
		myClient = null;
	}
	
	private void ReceiveMessage(NetworkMessage message)
	{
		//reading message
		string text = message.ReadMessage<StringMessage> ().value;
		textField.text = text;
	}

	private void BackButtonClicked()
	{
		if (myClient != null)
		{
				Disconnect();
		}
		else if (NetworkServer.active)
		{
				NetworkServer.Shutdown();
				ChangeNetworkUiVisibility(true);
				textField.text = "Server closed";
		}
	}

	private void ChangeNetworkUiVisibility(bool visibility)
	{
		networkUI.SetActive(visibility);
		backButton.gameObject.SetActive(!visibility);
	}
	
}
