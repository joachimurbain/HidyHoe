using Godot;
using System;

public partial class MultiplayerManager : Node
{
	[Export]
	public int Port = 8910;
	[Export]
	public string Address = "127.0.0.1";
	//private string Address = "151.80.43.66";

	private ENetMultiplayerPeer peer;
	private ENetConnection.CompressionMode compressionMode = ENetConnection.CompressionMode.RangeCoder;

	private Main mainNode
	{
		get => FindParent("Main") as Main;
	}

	public override void _Ready()
	{
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		Multiplayer.PeerDisconnected += PeerDisconnected;

		if (DisplayServer.GetName() == "headless")
		{
			GD.Print("Headless Mode !");
			CallDeferred(nameof(HostGame));
		}
	}

	private void HostGame()
	{
		//	FIXME: Cant reset hosting
		//if(peer != null) 
		//{
		//	peer.Close();
		//	peer = null;
		//	Multiplayer.MultiplayerPeer.Close();
		//	GetTree().SetMultiplayer(MultiplayerApi.CreateDefaultInterface());
		//}

		peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(Port);
		if (error != Error.Ok)
		{
			GD.PrintErr($"ERROR CANNOT HOST! : {error.ToString()}");
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting For Players!");
		if (DisplayServer.GetName() != "headless")
		{
			AddLobby();
			mainNode.SendPlayerInformation((FindChild("NameLineEdit") as LineEdit).Text, 1);
		}
	}

	private void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
	}


	private void PeerDisconnected(long id)
	{
		GD.Print("PEER DISCONECTED" + id.ToString());
	}

	private void ConnectedToServer()
	{
		AddLobby();
		mainNode.SendPlayerInformation((FindChild("NameLineEdit") as LineEdit).Text, Multiplayer.GetUniqueId());
		GD.Print("Connected to Server!");
	}

	public void OnHostButtonDown()
	{
		HostGame();
	}

	public void OnJoinButtonDown()
	{
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(Address, Port);
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining Game!");
	}

	public void AddLobby()
	{
		PackedScene scene = ResourceLoader.Load<PackedScene>("res://UI/Lobby.tscn");
		Lobby lobby = scene.Instantiate() as Lobby;


		//mainNode.PlayerListUpdate += lobby.RefreshPlayers;
		//lobby.ReadyButtonDown += mainNode.OnPlayerReadyCheck;
		//lobby.CancelButtonDown += mainNode.OnPlayerReadyCheck;
		//lobby.GamemodeSelected += mainNode.OnGameModeSelected;

		//mainNode.Connect(MultiplayerController.SignalName.PlayerListUpdate, new Callable(lobby, nameof(lobby.RefreshPlayers)));
		//lobby.Connect(Lobby.SignalName.ReadyButtonDown, new Callable(this, nameof(mainNode.OnPlayerReadyCheck)));
		//lobby.Connect(Lobby.SignalName.CancelButtonDown, new Callable(this, nameof(mainNode.OnPlayerReadyCheck)));
		//lobby.Connect(Lobby.SignalName.GamemodeSelected, new Callable(this, nameof(mainNode.OnGameModeSelected)));
		AddChild(lobby);
	}
}
