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

	[Export]
	public float SpottedDelay = 3.0f;
	[Export]
	public float Friction = 0.5f;
	[Export]
	public int MovementSpeed = 200;
	[Export]
	public PlayerRole Role = PlayerRole.None;
	[Export]
	public bool IsVisible = true;
	[Export]
	public bool IsSpotted = false;
	[Export]
	public bool IsCrouching = false;
	

	[Export]
	public int PlayerId { get=>_player; set {
			_player = value;
			GetNode<PlayerInput>("PlayerInput").SetMultiplayerAuthority(_player);
		}
	}
	private int _player = 1;



	private PlayerInput input;
	private int boxContact = 0;



	public override void _Ready()
	{
		input = GetNode<PlayerInput>("PlayerInput");
	}



	public override void _Process(double delta)
	{
		SetVision();

		//if (Multiplayer.GetUniqueId() == PlayerId) { 
		//}
		IsVisible = !(IsCrouching && IsAgainstObstacle() && !IsSpotted);

		Rpc(nameof(SetVisibility));

		//if (!IsVisible)
		//{
		//	Hide();
		//}
		//else
		//{
		//	Show();
		//}

	}

	private void SetVision()
	{
		if (Multiplayer.IsServer())
		{
			var players = GetTree().GetNodesInGroup("Players");
			foreach (Player player in players)
			{
				bool spotted = false;
				if (this.Name == player.Name || this.Role == player.Role)
				{
					continue;
				}
				RayCast2D rayCast = GetNodeOrNull<RayCast2D>("Raycast_" + player.Name);
				if (rayCast == null)
				{
					rayCast = new RayCast2D();
					rayCast.Name = "Raycast_" + player.Name;
					rayCast.TopLevel = true;
					rayCast.Enabled = true;
					AddChild(rayCast);
				}
				rayCast.Position = GlobalPosition;
				rayCast.TargetPosition = -1 * player.ToLocal(GlobalPosition);

				if (HasClearLineOfSight(rayCast))
				{
					Spotted(player);
					spotted = true;
				}
				if (!spotted)// TMP FIX => delayed ispotted reset is causing issues. replace scene timer with real timer and reset timer if spotted could be a fix
				{
					IsSpotted = false;
				}
			}
		}
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

	private bool DevMode = true;



	public override void _PhysicsProcess(double delta)
	{
		if (input.Crouching)
		{
			IsCrouching = true;
			SetAnimation("crouch");
		}
		else { 
			IsCrouching=false;
			SetAnimation("walk");
		}
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

	private bool IsAgainstObstacle()
	{
		return boxContact > 0;
	}

	private void SetAnimation(string animation)
	{
		AnimatedSprite2D animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animatedSprite2D.Animation = animation;
	}

	//[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	async private void Spotted(Player player)
	{
		player.IsSpotted = true;
		await ToSignal(GetTree().CreateTimer(SpottedDelay), SceneTreeTimer.SignalName.Timeout);
		//player.IsSpotted = false;
	}




}