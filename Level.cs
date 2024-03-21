using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class Level : Node2D
{

	[Export]
	private PackedScene playerScene;
	public static Vector2 ScreenSize;
	private Main mainNode
	{
		get => FindParent("Main") as Main;
	}
	public override void _Ready()
	{
		SetPlaygroundSize();
		if (Multiplayer.IsServer())
		{
			Multiplayer.PeerConnected += AddPlayer;
			Multiplayer.PeerDisconnected += RemovePlayer;

			foreach(int playerId in mainNode.Players.Keys)
			{
				AddPlayer(playerId);
			}
		}
	}

	private void SetPlaygroundSize()
	{
		ScreenSize = GetViewportRect().Size;
	}



	public override void _ExitTree()
	{
		if (Multiplayer.IsServer())
		{
			Multiplayer.PeerConnected -= AddPlayer;
			Multiplayer.PeerDisconnected -= RemovePlayer;
		}
	}


	private int spawnPointIndex = 0;
public void AddPlayer(long id)
	{
		GD.Print(id);
		Player player = playerScene.Instantiate<Player>();
		player.PlayerId = (int)id;
		player.Name = id.ToString();
		Node2D spawnPoint = GetTree().GetNodesInGroup("PlayerSpawnPoints")[spawnPointIndex] as Node2D;
		player.GlobalPosition = spawnPoint.Position;
		GetNode("Players").AddChild(player);
		spawnPointIndex++;
	}

	public void RemovePlayer(long id)
	{
		if (GetNode("Players").HasNode(id.ToString()))
		{
			GetNode($"Players/{id.ToString()}").QueueFree();
		}
	}




}
