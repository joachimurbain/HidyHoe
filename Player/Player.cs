using Godot;
using System;
using System.Data;



public partial class Player : CharacterBody2D
{
	[Export]
	public bool IsCrouching = false;
	[Export]
	public bool IsVisible = true;
	[Export]
	public bool IsSpotted = false;
	[Export]
	public bool IsRunning = false;
	[Export]
	public int PlayerId
	{
		get => _playerId;
		set => SetPlayerid(value);
	}
	private int _playerId = 1;
	[Export]
	public Globals.PlayerRole Role
	{
		get => _role;
		set => SetRole(value);
	}
	private Globals.PlayerRole _role = Globals.PlayerRole.None;



	public override void _Ready()
	{
	}


	public void SetPlayerid(int playerId)
	{
		_playerId = playerId;
		GetNode<PlayerInput>("MovementComponent/PlayerInput").SetMultiplayerAuthority(playerId);
	}

	public void SetRole(Globals.PlayerRole role)
	{
		_role = role;
		if (Multiplayer.GetUniqueId() == this.PlayerId)
		{
			GetNode<HUD>("HUD").ShowMessage($"YOU ARE A\n{this.Role}");
		}
	}

	public override void _Process(double delta)
	{		
	}

	public override void _PhysicsProcess(double delta)
	{
	}



}