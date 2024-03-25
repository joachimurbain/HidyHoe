using Godot;
using System;
using System.Text.RegularExpressions;
using static Godot.Projection;

public partial class MultiplayerManager : Node
{
	[Export]
	public int Port = 8910;
	[Export]
	public string Address = "127.0.0.1";
	//public string Address = "151.80.43.66";

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
		if (peer != null)
		{
			peer.Close();
			peer = null;
		}
		peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(Port);
		if (error != Error.Ok)
		{
			GD.PrintErr($"ERROR CANNOT HOST! : {error.ToString()}");
			return;
		}


		//peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting For Players!");
		if (DisplayServer.GetName() != "headless")
		{
			AddLobby();
			mainNode.SendPlayerInformation((FindChild("NameLineEdit") as LineEdit).Text, 1);
		}
		else
		{
			GameMode gamemode = GD.Load<GameMode>("res://GameModes/Bo3.tres");
			mainNode.GameMode = gamemode;
		}
	}




	private void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
	}


	private void PeerDisconnected(long playerId)
	{
		GD.Print("PEER DISCONECTED" + playerId.ToString());

		if (Multiplayer.IsServer())
		{
			mainNode.Players.Remove((int)playerId);

			if(mainNode.Players.Count > 0)
			{
				mainNode.SendPlayerInfo();
			}
			else
			{
				GD.Print("Reseting Game");
				mainNode.ClearGame();
				GetTree().Paused = false;
			}

		}


	}

	private void ConnectedToServer()
	{
		mainNode.SendPlayerInformation((FindChild("NameLineEdit") as LineEdit).Text, Multiplayer.GetUniqueId());
		AddLobby();
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
		AddChild(lobby);
	}
}
