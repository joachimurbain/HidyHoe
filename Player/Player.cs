using Godot;
using System;
using System.Data;



public partial class Player : CharacterBody2D
{

	public enum PlayerRole
	{
		None,
		Seeker,
		Hider
	}

	public enum PlayerAnimation
	{
		Crouch,
		Walk,
	}

	[Export]
	public float SpottedDelay = 3.0f;
	[Export]
	public float Friction = 0.5f;
	[Export]
	public int MovementSpeed = 200;
	[Export]
	public PlayerRole Role
	{
		get => _role;
		set
		{
			_role = value;
			OnRoleSet();
		}
	}

	private PlayerRole _role = PlayerRole.None;

	[Export]
	public bool IsVisible = true;
	[Export]
	public bool IsSpotted = false;
	[Export]
	public bool IsCrouching = false;
	[Export]
	public int PlayerId
	{
		get => _player;
		set
		{
			_player = value;
			GetNode<PlayerInput>("PlayerInput").SetMultiplayerAuthority(_player);
		}
	}
	private int _player = 1;
	private PlayerInput input;
	private int boxContact = 0;
	private Timer spottedTimer;
	private AnimatedSprite2D animatedSprite2D;


	public override void _Ready()
	{
		input = GetNode<PlayerInput>("PlayerInput");
		spottedTimer = GetNode<Timer>("SpottedTimer");
		animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}
	private void OnRoleSet()
	{
        if (Multiplayer.GetUniqueId()==this.PlayerId)
        {            
			GetNode<HUD>("HUD").ShowMessage($"YOU ARE A\n{this.Role}");
        }
	}
	public override void _Process(double delta)
	{
		ServerProcess(delta);		
		Rpc(nameof(SetVisibility));
	}

	private void ServerProcess(double delta)
	{
		if (!Multiplayer.IsServer())
		{
			return;
		}
		SetVision();
		IsVisible = !(IsCrouching && IsAgainstObstacle() && !IsSpotted);
	}

	private void SetVision()
	{
		var players = GetTree().GetNodesInGroup("Players");
		foreach (Player player in players)
		{
			if (this.Role == player.Role)
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

	private void UpdateRayCast(RayCast2D rayCast,Player player)
	{
		rayCast.Position = GlobalPosition;
		rayCast.TargetPosition = -1 * player.ToLocal(GlobalPosition);
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
		player.IsSpotted = true;
		if (!player.spottedTimer.IsStopped())
		{
			player.spottedTimer.Stop();
		}
		player.spottedTimer.WaitTime = SpottedDelay;
		player.spottedTimer.Start();
	}

	private bool IsAgainstObstacle()
	{
		return boxContact > 0;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void SetVisibility()
	{
		Color modulateColor = Modulate;
		Player clientContext = GetParent().GetNode<Player>(Multiplayer.GetUniqueId().ToString());
		if (clientContext.Role == this.Role && !IsVisible)
		{
			modulateColor.A = 0.5f;
			Modulate = modulateColor;
		}
		else if (!IsVisible)
		{
			modulateColor.A = 0.0f;
			Modulate = modulateColor;
		}
		else
		{
			modulateColor.A = 1.0f;
			Modulate = modulateColor;
		}
		//TODO TWEEN THIS SHIT UP
	}

	public override void _PhysicsProcess(double delta)
	{
		SetCrouching(delta);
		SetPosition(delta);
	}

	private void SetCrouching(double delta)
	{
		IsCrouching = input.Crouching;
		if (IsCrouching)
		{
			animatedSprite2D.Animation = PlayerAnimation.Crouch.ToString();
		}
		else
		{
			animatedSprite2D.Animation = PlayerAnimation.Walk.ToString();
		}
	}

	private void SetPosition(double delta)
	{
		Vector2 direction = input.Direction.Normalized();
		Velocity = Velocity.Lerp(direction * MovementSpeed, 0.1f);
		Velocity = Velocity * (float)(1 - (Friction * delta));
		MoveAndSlide();
		Position = new Vector2(
			x: Mathf.Clamp(Position.X, 0, Level.ScreenSize.X),
			y: Mathf.Clamp(Position.Y, 0, Level.ScreenSize.Y)
		);
	}

	public void OnEnteringHidingPlace(Node2D body)
	{
		if (body is TileMap)
		{
			boxContact++;
		}
	}

	public void OnLeavingHidingPlace(Node2D body)
	{
		if (body is TileMap)
		{
			boxContact--;
		}
	}
	public void OnSpottedTimerTimeout()
	{
		IsSpotted = false;
	}
}