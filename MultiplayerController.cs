using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using static Globals;
using static System.Formats.Asn1.AsnWriter;

public partial class MultiplayerController : Node
{
	[Signal]
	public delegate void PlayerListUpdateEventHandler();
	[Export]
	public int PlayerReadyCount = 0;

	[Export]
	public Godot.Collections.Dictionary<int, Dictionary> PlayerList
	{
		get => playerList;
		set => SetPlayerList(value);
	}
	private Godot.Collections.Dictionary<int, Dictionary> playerList = new Godot.Collections.Dictionary<int, Dictionary>();

	private void SetPlayerList(Godot.Collections.Dictionary<int, Dictionary> playerList)
	{
		if (!PlayerList.Equals(playerList))
		{
			this.playerList = playerList;
			EmitSignal(SignalName.PlayerListUpdate);
		}
	}

	[Export]
	public int Port = 8910;
	[Export]
	public string Address = "127.0.0.1";
	//private string Address = "151.80.43.66";
	private ENetMultiplayerPeer peer;
	private ENetConnection.CompressionMode compressionMode = ENetConnection.CompressionMode.RangeCoder;
	public override void _Ready()
	{
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;


		Multiplayer.PeerDisconnected += PeerDisconnected;

		if (DisplayServer.GetName() == "headless")
		{
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
			AddWaitingRoom();
			SendPlayerInformation((FindChild("NameLineEdit") as LineEdit).Text, 1);
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
		SendPlayerInformation((FindChild("NameLineEdit") as LineEdit).Text, Multiplayer.GetUniqueId());
		//(FindChild("WaitingRoom") as Control).Show();
		AddWaitingRoom();
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

	public void OnStartGameButtonDown()
	{
		StartGame();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartGame()
	{

		GetNode<Control>("Lobby").Hide();
		GetNode<Control>("WaitingRoom").Hide();
		GetTree().Paused = false;

		if (Multiplayer.IsServer())
		{
			CallDeferred(nameof(ChangeLevel), ResourceLoader.Load<PackedScene>("res://Level.tscn"));
		}
	}

	private void ChangeLevel(PackedScene scene)
	{
		Node level = GetNode("Level");
		foreach (Node child in level.GetChildren())
		{
			level.RemoveChild(child);
			child.QueueFree();
		}
		level.AddChild(scene.Instantiate());
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void SendPlayerInformation(string name, int id)
	{
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(SendPlayerInformation), name, id);
		}
		else
		{
			Dictionary playerInfo = new Dictionary
			{
				{ "Name", name },
				{ "Id", id }
			};
			PlayerList[id] = playerInfo;
			PlayerList = PlayerList.Duplicate(); // Trigger SetPlayerList()
		}
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void OnPlayerReadyCheck(int inc)
	{
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(OnPlayerReadyCheck), inc);
		}
		else
		{
			PlayerReadyCount += inc;
			if( PlayerReadyCount == PlayerList.Count ) {
		
				Rpc(nameof(StartGame));
			}
		}

	}

	public void AddWaitingRoom()
	{
		PackedScene scene = ResourceLoader.Load<PackedScene>("res://WaitingRoom.tscn");
		WaitingRoom waitingRoom = scene.Instantiate() as WaitingRoom;
		AddChild(waitingRoom);
		Connect(SignalName.PlayerListUpdate, new Callable(waitingRoom, nameof(waitingRoom.RefreshPlayers)));
		waitingRoom.Connect(WaitingRoom.SignalName.ReadyButtonDown, new Callable(this, nameof(OnPlayerReadyCheck)));
		waitingRoom.Connect(WaitingRoom.SignalName.CancelButtonDown, new Callable(this, nameof(OnPlayerReadyCheck)));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void PlayerDisconnect(int playerId)
	{
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(PlayerDisconnect), playerId);
		}
		else
		{

			PlayerList.Remove(playerId);
			PlayerList = PlayerList.Duplicate();
			Multiplayer.MultiplayerPeer.DisconnectPeer(playerId);
		}

	}
}
