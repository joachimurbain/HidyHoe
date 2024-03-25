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
	public bool IsBoosted = false;

	[Export]
	public bool IsInChaseMode = false;

	[Export]
	public double CurrentStamina;
	[Export]
	public int PlayerId
	{
		get => _playerId;
		set => SetPlayerid(value);
	}
	private int _playerId = 1;

	private Main mainNode
	{
		get => FindParent("Main") as Main;
	}

	[Export]
	public PlayerInfo.PlayerRole Role
	{
		get => _role;
		set => SetRole(value);
	}
	private PlayerInfo.PlayerRole _role = PlayerInfo.PlayerRole.None;



	public override void _Ready()
	{
		Role = mainNode.Players[PlayerId].Role;
		GetNode<Control>("HUD/Countdown").SetProcess(PlayerId == Multiplayer.GetUniqueId());
		GetNode<Control>("HUD/RoundCounter").SetProcess(PlayerId == Multiplayer.GetUniqueId());
		GetNode<Node>("CatchComponent").SetProcess(Role == PlayerInfo.PlayerRole.Seeker);
	}


	public void SetPlayerid(int playerId)
	{
		_playerId = playerId;
		GetNode<PlayerInput>("MovementComponent/PlayerInput").SetMultiplayerAuthority(playerId);
		//if(Multiplayer != null)
		//{
		//	GetNode<Countdown>("HUD/Countdown").SetProcess(PlayerId == Multiplayer.GetUniqueId());
		//}
	}

	public void SetRole(PlayerInfo.PlayerRole role)
	{
		_role = role;
		GetNode<HUD>("HUD").ShowMessage($"YOU ARE A\n{this.Role}");
	}

	public override void _Process(double delta)
	{		
	}

	public override void _PhysicsProcess(double delta)
	{
	}



}