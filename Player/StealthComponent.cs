using Godot;
using System;

public partial class StealthComponent : Node
{

	private int hidingPlacesCollision = 0;
	private Player localPlayer;

	public override void _Ready()
	{
		localPlayer = GetParent<Player>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Multiplayer.IsServer())
		{
			localPlayer.IsVisible = localPlayer.IsSpotted || !(localPlayer.IsCrouching && IsInHiding());
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
		Color modulateColor = localPlayer.Modulate;
		Player clientContext = FindParent("Players").GetNode<Player>(Multiplayer.GetUniqueId().ToString());
		if (clientContext.Role == localPlayer.Role && !localPlayer.IsVisible)
		{
			modulateColor.A = 0.5f;
			localPlayer.Modulate = modulateColor;
		}
		else if (!localPlayer.IsVisible)
		{
			modulateColor.A = 0.0f;
			localPlayer.Modulate = modulateColor;
		}
		else
		{
			modulateColor.A = 1.0f;
			localPlayer.Modulate = modulateColor;
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
