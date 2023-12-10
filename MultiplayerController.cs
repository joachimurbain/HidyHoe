using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MultiplayerController : Control
{

	[Export]
	private int Port = 8910;
	[Export]
	private string Address = "127.0.0.1";
	//private string Address = "151.80.43.66";
	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
	private ENetConnection.CompressionMode compressionMode = ENetConnection.CompressionMode.RangeCoder;
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		if (OS.GetCmdlineArgs().Contains("--server"))
		{
			CallDeferred(nameof(HostGame));
		}
	}

	private void HostGame()
	{
		Error error = peer.CreateServer(Port);
		if (error != Error.Ok)
		{
			GD.Print($"ERROR CANNOT HOST! : {error.ToString()}");
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		StartGame();
	}


	private void PeerConnected(long id)
	{
		GD.Print($"Player Connected! : {id.ToString()}");

	}

	private void PeerDisconnected(long id)
	{
		GD.Print($"Player Disconnected! : {id.ToString()}");
		int index = GameManager.Players.FindIndex(player => player.Id == id);
		GameManager.Players.RemoveAt(index);
		Array<Node> players = GetTree().GetNodesInGroup("Player");
		foreach ( Node node in players )
		{
			if(node.Name == id.ToString())
			{
				node.QueueFree();
			}
		}

	}

	private void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
	}

	private void ConnectedToServer()
	{
		
		GD.Print("Connected to Server!");
		RpcId(1,nameof(SendPlayerInformation), GetNode<LineEdit>("VBoxContainer/LineEdit").Text, Multiplayer.GetUniqueId());
	}

	public void OnHostButtonDown() {
		HostGame();
		SendPlayerInformation(GetNode<LineEdit>("VBoxContainer/LineEdit").Text, 1);
	}

	public void OnJoinButtonDown()
	{
		peer= new ENetMultiplayerPeer();
		peer.CreateClient(Address,Port);
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining Game!");

	}

	public void OnStartGameButtonDown()
	{
		Rpc(nameof(StartGame));
		//Rpc("StartGame");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal =true,TransferMode =MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartGame()
	{
		foreach(PlayerInfo player in GameManager.Players)
		{
			GD.Print($"{player.Name} is playing");
		}
		Node scene = ResourceLoader.Load<PackedScene>("res://Main.tscn").Instantiate<Node>();
		GetTree().Root.AddChild(scene);
		this.Hide();
		GetTree().Paused = false;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void SendPlayerInformation(string name,int id)
	{
		PlayerInfo playerInfo = new PlayerInfo()
		{
			Name = name,
			Id = id
		};
		if (!GameManager.Players.Contains(playerInfo)){
			GameManager.Players.Add(playerInfo);
		}

		if (Multiplayer.IsServer())
		{
			foreach(PlayerInfo player  in GameManager.Players)
			{
				Rpc(nameof(SendPlayerInformation),player.Name,player.Id);
			}
		}

	}
}
