using System;
using System.Collections;
using System.Collections.Generic;
using Tango;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

public class ServerClient : MonoBehaviour {

	public bool isSonde = false;
	public Button serverButton;
	public Button clientButton;
	public Button sendButton;
	public Button backButton;
	public Button SondeButton;
	public GameObject DevicePosition;
	public GameObject Client3DUI;
	public GameObject Cube;
	
	public InputField serverIpInput;
	public Text textField;
	public Text PositinoDebugText;
	public Text FactorText;
	public Slider MovementFactorSlider;
	
	public GameObject networkUI;
	public const short MSG_ID = 213;

	public float movemendSpeedFactor = 10f;
	
	private Transform offset;
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
		
		SondeButton.gameObject.SetActive(false);
		SondeButton.onClick.AddListener(SondeButtonClicked);
		
		Client3DUI.gameObject.SetActive(false);
		MovementFactorSlider.gameObject.SetActive(false);
		FactorText.gameObject.SetActive(false);
		PositinoDebugText.gameObject.SetActive(false);
		offset = DevicePosition.transform;
	}

	private void SondeButtonClicked()
	{
		isSonde = !isSonde;
		SondeButton.gameObject.GetComponent<Text>().text = isSonde ? "Stop Sonde" : "Start Sonde";
	}

	private void Update()
	{
		if (NetworkServer.active && isSonde)
		{
//			movemendSpeedFactor = MovementFactorSlider.value;
//			FactorText.text = movemendSpeedFactor.ToString();
			PositinoDebugText.text = "x: " + DevicePosition.transform.localPosition.x + "\n y: " + DevicePosition.transform.localPosition.y + "\n z: " + DevicePosition.transform.localPosition.z + "\n Quaternium: " + DevicePosition.transform.localRotation;
			Vector3 localPosition = DevicePosition.transform.localPosition;
			
			TransfromObject transfromObject = new TransfromObject();
			transfromObject.x = localPosition.x * movemendSpeedFactor;
			transfromObject.y = localPosition.y * movemendSpeedFactor;
			transfromObject.z = localPosition.z * movemendSpeedFactor;
			
			Quaternion localRoation = DevicePosition.transform.localRotation;
			transfromObject.qx = localRoation.x;
			transfromObject.qy = localRoation.y;
			transfromObject.qz = localRoation.z;
			transfromObject.qw = localRoation.w;
			
			StringMessage positionMessage = new StringMessage();
			positionMessage.value = JsonUtility.ToJson(transfromObject);
			NetworkServer.SendToAll(MSG_ID, positionMessage);
		}
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
		SondeButton.gameObject.SetActive(true);
		MovementFactorSlider.gameObject.SetActive(true);
		FactorText.gameObject.SetActive(true);
		PositinoDebugText.gameObject.SetActive(true);
		Screen.orientation = ScreenOrientation.Portrait;
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
		Client3DUI.gameObject.SetActive(true);
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
		SondeButton.gameObject.SetActive(false);
		Client3DUI.gameObject.SetActive(false);
		MovementFactorSlider.gameObject.SetActive(false);
		FactorText.gameObject.SetActive(false);
		PositinoDebugText.gameObject.SetActive(false);
		myClient.Disconnect();
		myClient = null;
	}
	
	private void ReceiveMessage(NetworkMessage message)
	{
		//reading message
		string messageValue = message.ReadMessage<StringMessage> ().value;

		TransfromObject transfromObject = JsonUtility.FromJson<TransfromObject>(messageValue);
		Cube.transform.localPosition = new Vector3(transfromObject.x, transfromObject.y, transfromObject.z);
		Cube.transform.localRotation = new Quaternion(transfromObject.qx, transfromObject.qy, transfromObject.qz, transfromObject.qw);
		
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
			SondeButton.gameObject.SetActive(false);
			Client3DUI.gameObject.SetActive(false);
			MovementFactorSlider.gameObject.SetActive(false);
			FactorText.gameObject.SetActive(false);
			PositinoDebugText.gameObject.SetActive(false);
			textField.text = "Server closed";
			Screen.orientation = ScreenOrientation.Landscape;
		}
	}

	private void ChangeNetworkUiVisibility(bool visibility)
	{
		networkUI.SetActive(visibility);
		backButton.gameObject.SetActive(!visibility);
	}
	
	[Serializable]
	public class TransfromObject
	{
		public float x;
		public float y;
		public float z;
		
		public float qx;
		public float qy;
		public float qz;
		public float qw;

	}
	
}
