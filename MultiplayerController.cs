using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MultiplayerController : Node
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
	}

	private void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
	}

	private void ConnectedToServer()
	{
		GD.Print("Connected to Server!");
	}

	public void OnHostButtonDown() {
		HostGame();
	}

	public void OnJoinButtonDown()
	{
		peer= new ENetMultiplayerPeer();
		peer.CreateClient(Address,Port);
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining Game!");
		StartGame();
	}

	public void OnStartGameButtonDown()
	{
		StartGame();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal =true,TransferMode =MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartGame()
	{
		GetNode<Control>("Multiplayer Controller").Hide();
		GetTree().Paused = false;

		if (Multiplayer.IsServer())
		{
			CallDeferred(nameof(ChangeLevel), ResourceLoader.Load<PackedScene>("res://Level.tscn"));
		}
	}

	private void ChangeLevel(PackedScene scene)
	{
		Node level = GetNode("Level");
		foreach(Node child in level.GetChildren())
		{
			level.RemoveChild(child);
			child.QueueFree();
		}
		level.AddChild(scene.Instantiate());
	}

}
