using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	public static Vector2 ScreenSize;
	public override void _Ready()
	{
		SetPlaygroundSize();


		if (Multiplayer.GetUniqueId() == 1)
		{

			List<PlayerInfo> players = GameManager.Players;
			int seekerIndex = new Random().Next(GameManager.Players.Count);

			int index = 0;
			foreach ( PlayerInfo player in players )
			{
				if(index==seekerIndex)
			}

			//var seeker = GameManager.Players[random.Next(GameManager.Players.Count)];

			//Array<Node> players = GetTree().GetNodesInGroup("Players");
			//Node seeker = (Node)players[random.Next(players.Count)];
			//seeker.Rpc(nameof(Player.SetSeeker), seeker.Name);
		}

	}

	private void SetPlaygroundSize()
	{
		ScreenSize = GetViewportRect().Size;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public override void _Process(double delta)
	{
	}


	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void SetSeeker2(string playerName)
	{
		GetNode<Player>("World/"+playerName).Modulate = new Color(255, 0, 0, 1);
	}

}
