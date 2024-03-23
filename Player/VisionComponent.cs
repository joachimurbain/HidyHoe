using Godot;
using System;
using System.Linq;

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
		var seekers = GetTree().GetNodesInGroup("Players").OfType<Player>().Where(player => player.Role == PlayerInfo.PlayerRole.Seeker);
		var hiders = GetTree().GetNodesInGroup("Players").OfType<Player>().Where(player => player.Role == PlayerInfo.PlayerRole.Hider);
		foreach (Player seeker in seekers)
		{

			foreach(Player hider in hiders)
			{
				RayCast2D rayCast = GetOrSetRayCast(seeker,hider);
				UpdateRayCast(rayCast, seeker,hider);

				if (HasClearLineOfSight(rayCast))
				{
					seeker.IsInChaseMode = true;	
					hider.IsSpotted = true;
					seeker.IsSpotted = true;
				}
				else if (playerNode.Role == PlayerInfo.PlayerRole.Seeker)
				{
					playerNode.IsInChaseMode = false;
				}
			}

		}
	}

	private RayCast2D GetOrSetRayCast(Player seeker, Player hider)
	{
		RayCast2D rayCast = GetNodeOrNull<RayCast2D>("Raycast_" + seeker.PlayerId + "_" + hider.PlayerId);
		if (rayCast == null)
		{
			rayCast = DrawRayCast(seeker, hider);
		}
		return rayCast;
	}
	private RayCast2D DrawRayCast(Player seeker, Player hider)
	{
		RayCast2D rayCast = new RayCast2D();
		rayCast.Name = "Raycast_" + seeker.PlayerId + "_" + hider.PlayerId;
		rayCast.TopLevel = true;
		rayCast.Enabled = true;
		AddChild(rayCast);
		return rayCast;
	}

	private void UpdateRayCast(RayCast2D rayCast, Player seeker, Player hider)
	{
		rayCast.Position = seeker.GlobalPosition;
		rayCast.TargetPosition = -1 * hider.ToLocal(seeker.GlobalPosition);
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
}
