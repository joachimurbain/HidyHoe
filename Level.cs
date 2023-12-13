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
	public override void _Ready()
	{
		SetPlaygroundSize();

		if (Multiplayer.IsServer())
		{
			Multiplayer.PeerConnected += AddPlayer;
			Multiplayer.PeerDisconnected += RemovePlayer;
			foreach(int id in Multiplayer.GetPeers()){
				AddPlayer(id);
			}

			if (!OS.GetCmdlineArgs().Contains("--server"))
			{
				AddPlayer(1);
			}

			Array<Node> players = GetTree().GetNodesInGroup("Players");
			int seekerIndex = new Random().Next(players.Count);
			int index = 0;
			foreach (Player player in players)
			{
				if (index == seekerIndex)
				{
					player.Role = Player.PlayerRole.Seeker;
				}
				else
				{
					player.Role = Player.PlayerRole.Hider;
				}
				index++;
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
