using Godot;
using System;

public partial class CatchComponent : Node
{

	[Export]
	public int CatchRadius = 50;
	private CollisionShape2D catchAreaCollisionShape;
	private Main mainNode
	{
		get => FindParent("Main") as Main;
	}

	private Player playerNode
	{
		get => GetParent<Player>();
	}

	public override void _Ready()
	{
		catchAreaCollisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
		((CircleShape2D)catchAreaCollisionShape.Shape).Radius = CatchRadius;		
	}

	public void OnCatchRadiusEntered(Node2D body)
	{
		if (body is Player  && ((Player)body).Role == PlayerInfo.PlayerRole.Hider && ((Player)body).IsSpotted )
		{
			mainNode.EndRound(Globals.RoundOutcome.SeekerVictory);
		}
	}

	public override void _Process(double delta)
	{	
		catchAreaCollisionShape.Disabled = !playerNode.IsInChaseMode;
	}

}
