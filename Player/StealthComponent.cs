using Godot;
using System;

public partial class StealthComponent : Node
{


	[Export]
	public float SpottedDuration = 3.0f;

	private int hidingPlacesCollision = 0;
	private Player playerNode;
	private Timer spottedTimer;

	private Main mainNode
	{
		get => FindParent("Main") as Main;
	}

	public override void _Ready()
	{
		playerNode = GetParent<Player>();
		spottedTimer = GetNode<Timer>("SpottedTimer");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (Multiplayer.IsServer())
		{
			if (playerNode.IsSpotted && spottedTimer.IsStopped())
			{
				Spotted();
			}
			playerNode.IsVisible = playerNode.IsSpotted || !(playerNode.IsCrouching && IsInHiding());
			Rpc(nameof(UpdatePlayerVisibility));
		}

	}

	private bool IsInHiding()
	{
		return hidingPlacesCollision > 0;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void UpdatePlayerVisibility()
	{
		AnimatedSprite2D playerNodeSprite = playerNode.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

		if (playerNodeSprite == null)
		{
			return;
		}

		Color modulateColor = playerNodeSprite.Modulate;
		if (mainNode.Players.ContainsKey(Multiplayer.GetUniqueId()) && mainNode.Players[Multiplayer.GetUniqueId()].Role == playerNode.Role && !playerNode.IsVisible)
		{
			modulateColor.A = 0.5f;
			playerNodeSprite.Modulate = modulateColor;
		}
		else if (!playerNode.IsVisible)
		{
			modulateColor.A = 0.0f;
			playerNodeSprite.Modulate = modulateColor;
		}
		else
		{
			modulateColor.A = 1.0f;
			playerNodeSprite.Modulate = modulateColor;
		}
		//TODO TWEEN THIS SHIT UP
	}

	public void OnEnteringHidingPlace(Node2D body)
	{
		if (body is TileMap)
		{
			hidingPlacesCollision++;
		}
	}

	public void OnLeavingHidingPlace(Node2D body)
	{
		if (body is TileMap)
		{
			hidingPlacesCollision--;
		}
	}


	private void Spotted()
	{
		if (!spottedTimer.IsStopped())
		{
			spottedTimer.Stop();
		}
		spottedTimer.WaitTime = SpottedDuration;
		spottedTimer.Start();
	}

	public void OnSpottedTimerTimeout()
	{
		playerNode.IsSpotted = false;
	}


}
