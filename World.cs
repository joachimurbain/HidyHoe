using Godot;
using System;

public partial class World : Node2D
{

	[Export]
	private PackedScene playerScene;

	public override void _Ready()
	{
		int index = 0;
		foreach (PlayerInfo player in GameManager.Players)
		{
			Player currentPlayer = playerScene.Instantiate<Player>();
			currentPlayer.Name = player.Id.ToString();
			//currentPlayer.SetUpPlayer(player.Name);
			foreach (Node2D spawnpoint in GetTree().GetNodesInGroup("PlayerSpawnPoints"))
			{
				if (int.Parse(spawnpoint.Name) == index)
				{
					currentPlayer.GlobalPosition = spawnpoint.Position;
				}
			}
			AddChild(currentPlayer);
			index++;
		}
	}

}
