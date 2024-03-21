using Godot;
using System;

public partial class VisionComponent : Node
{

	[Export]
	public float SpottedDelay = 3.0f;
	private Player playerNode
	{
		get => GetParent<Player>();
	}
	private Main mainNode
	{
		get => FindParent("Main") as Main;
	}
	public override void _Ready()
	{
		
	}

	public override void _Process(double delta)
	{
		if (Multiplayer.IsServer())
		{
			SetVision();
		}
	}

	private void SetVision()
	{
		var players = GetTree().GetNodesInGroup("Players");
		foreach (Player player in players)
		{
			//playerNode.Role != PlayerInfo.PlayerRole.Seeker ||
			if (playerNode.Role == player.Role)
			{
				continue;
			}
			RayCast2D rayCast = GetOrSetRayCast(player);
			UpdateRayCast(rayCast, player);
			if (HasClearLineOfSight(rayCast))
			{
				Spotted(player);
				if(player.Role == PlayerInfo.PlayerRole.Hider) {
					mainNode.EndRound(Globals.RoundOutcome.SeekerVictory);
				}
			}
		}
	}

	private RayCast2D GetOrSetRayCast(Player player)
	{
		RayCast2D rayCast = GetNodeOrNull<RayCast2D>("Raycast_" + player.Name);
		if (rayCast == null)
		{
			rayCast = DrawRayCast(player);
		}
		return rayCast;
	}
	private RayCast2D DrawRayCast(Player player)
	{
		RayCast2D rayCast = new RayCast2D();
		rayCast.Name = "Raycast_" + player.Name;
		rayCast.TopLevel = true;
		rayCast.Enabled = true;
		AddChild(rayCast);
		return rayCast;
	}

	private void UpdateRayCast(RayCast2D rayCast, Player player)
	{
		rayCast.Position = playerNode.GlobalPosition;
		rayCast.TargetPosition = -1 * player.ToLocal(playerNode.GlobalPosition);
	}
	private bool HasClearLineOfSight(RayCast2D rayCast)
	{
		if (rayCast.IsColliding())
		{
			Node2D collision = (Node2D)rayCast.GetCollider();
			if (collision is Player)
			{
				return true;
			}
		}
		return false;
	}

	private void Spotted(Player player)
	{
		Timer spottedTimer = player.GetNode<Timer>("VisionComponent/SpottedTimer");
		player.IsSpotted = true;
		if (!spottedTimer.IsStopped())
		{
			spottedTimer.Stop();
		}
		spottedTimer.WaitTime = SpottedDelay;
		spottedTimer.Start();
	}

	public void OnSpottedTimerTimeout()
	{
		playerNode.IsSpotted = false;
	}

}
