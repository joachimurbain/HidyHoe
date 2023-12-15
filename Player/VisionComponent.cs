using Godot;
using System;

public partial class VisionComponent : Node
{

	[Export]
	public float SpottedDelay = 3.0f;
	private Player localPlayer
	{
		get => GetParent<Player>();
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
			if (localPlayer.Role == player.Role)
			{
				continue;
			}
			RayCast2D rayCast = GetOrSetRayCast(player);
			UpdateRayCast(rayCast, player);
			if (HasClearLineOfSight(rayCast))
			{
				Spotted(player);
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
		rayCast.Position = localPlayer.GlobalPosition;
		rayCast.TargetPosition = -1 * player.ToLocal(localPlayer.GlobalPosition);
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
		localPlayer.IsSpotted = false;
	}

}
