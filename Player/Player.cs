using Godot;
using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Friction = 0.5f;
	[Export]
	public int MovementSpeed = 200;
	[Export]
	public int DetectRadius = 300;
	[Export]
	public bool ForceCrouched = false;

	private bool Crouched = false;
	private Vector2 syncPosition;
	private CollisionShape2D lineOfSightCollision;
	private RayCast2D lineOfSight;
	private Player otherPlayer;
	private int boxContact = 0;


	public override void _Ready()
	{
		SetMultiplayerAuthority();
		SetDetectionRadius(DetectRadius);
	}

	private void SetMultiplayerAuthority()
	{
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(this.Name));
	}

	private void SetDetectionRadius(float radius)
	{
		CircleShape2D shape = new CircleShape2D();
		shape.Radius = radius;
		GetNode<CollisionShape2D>("Visibility/CollisionShape2D").Shape = shape;
		lineOfSight = GetNode<RayCast2D>("Visibility/RayCast2D");
	}

	public bool IsCurrentPlayer()
	{
		return GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
	}

	public void debug()
	{
		if (Input.IsActionJustPressed("debug")  && IsCurrentPlayer())
		{
			GD.Print(otherPlayer.Name);
		}
	}

	public override void _Process(double delta)
	{

		debug();





		if (otherPlayer != null)
		{
			lineOfSight.TargetPosition = -1 * otherPlayer.ToLocal(GlobalPosition);
			if (HasClearLineOfSight() && IsCurrentPlayer() && Multiplayer.GetUniqueId() != 1)
			{
				GD.Print($"{this.Name} found {otherPlayer.Name} !");
				otherPlayer.Crouched = false;
			}
		}

		SetCrouchState();


	}
	

	private void SetCrouchState()
	{
		bool isHidden;
		string animation;
		Color modulateColor = Modulate;
		if (Crouched && IsAgainstObstacle())
		{
			animation = "crouch";
			modulateColor.A = 0.5f;
			isHidden = true;
		}
		else if (Crouched)
		{
			animation = "crouch";
			modulateColor.A = 1f;
			isHidden = false;
		}
		else
		{
			animation = "walk";
			modulateColor.A = 1f;
			isHidden = false;
		}
		SetAnimation(animation);
		if (IsCurrentPlayer())
		{
			Modulate = modulateColor;
		}
		else {
			Visible = !isHidden;
		}
	}

	private bool HasClearLineOfSight()
	{
		if (lineOfSight.IsColliding())
		{
			Node2D collision = (Node2D)lineOfSight.GetCollider();
			if (collision is Player)
			{
				return true;
			}
		}
		return false;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsCurrentPlayer())
		{
			//Crouched = Input.IsActionPressed("crouch");


			if (Multiplayer.GetUniqueId() == 1)
			{
				if (Input.IsActionJustPressed("crouch"))
				{
					Crouched = true;
				}
			}
			else { 
				Crouched = Input.IsActionPressed("crouch");
			}

			Vector2 direction = new Vector2(
				Input.GetActionStrength("right") - Input.GetActionStrength("left"),
				Input.GetActionStrength("down") - Input.GetActionStrength("up")
			);
			direction = direction.Normalized();
			Velocity = Velocity.Lerp(direction * MovementSpeed, 0.1f);
			Velocity = Velocity * (float)(1 - (Friction * delta));
			MoveAndSlide();
			Position = new Vector2(
				x: Mathf.Clamp(Position.X, 0, Main.ScreenSize.X),
				y: Mathf.Clamp(Position.Y, 0, Main.ScreenSize.Y)
			);
			syncPosition = GlobalPosition;
		}
		else
		{
			GlobalPosition = GlobalPosition.Lerp(syncPosition, 0.25f);
		}
	}

	public void OnVisibilityBodyEntered(Node2D body)
	{
		if (body is Player && body != this)
		{
			otherPlayer = (Player)body;
			GD.Print($"Other player is set to {otherPlayer.Name} for {this.Name}");
		}
	}

	public void OnVisibilityBodyExited(Node2D body)
	{
		if(body == otherPlayer)
		{
			otherPlayer = null;
			lineOfSight.TargetPosition = Vector2.Zero;
		}
	}

	public void OnEnteringHidingPlace(Node2D body)
	{
		if(body is TileMap)
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



}
