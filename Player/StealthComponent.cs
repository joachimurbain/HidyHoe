using Godot;
using System;

public partial class StealthComponent : Node
{

	private int hidingPlacesCollision = 0;
	private Player playerNode;

	private Main mainNode
	{
		get => FindParent("Main") as Main;
	}

	public override void _Ready()
	{
		playerNode = GetParent<Player>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Multiplayer.IsServer()) // WHAT ? is this old debug stuff ?
		{
			playerNode.IsVisible = playerNode.IsSpotted || !(playerNode.IsCrouching && IsInHiding());
		}

		Rpc(nameof(UpdatePlayerVisibility));
	}

	private bool IsInHiding()
	{
		return hidingPlacesCollision > 0;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void UpdatePlayerVisibility()
	{
		AnimatedSprite2D playerNodeSprite = playerNode.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

		if(playerNodeSprite == null)
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
}
