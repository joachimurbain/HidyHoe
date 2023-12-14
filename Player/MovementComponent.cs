using Godot;
using System;

public partial class MovementComponent : Node
{
	[Export]
	public float Friction = 0.5f;
	[Export]
	public float MovementSpeed = 200f;
	[Export]
	public float SprintSpeedMultiplier = 1.5f;


	private PlayerInput input;
	private AnimatedSprite2D animatedSprite2D;
	private float baseMovementSpeed;
	private Player localPlayer;


	public override void _Ready()
	{
		localPlayer = GetParent<Player>();
		animatedSprite2D = localPlayer.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		input = GetNode<PlayerInput>("PlayerInput");
		baseMovementSpeed = MovementSpeed;
	}

	public override void _Process(double delta)
	{

	}

	public override void _PhysicsProcess(double delta)
	{
		UpdateCrouchingState(delta);
		UpdateSprintingState(delta);
		UpdatePlayerPosition(delta);
	}

	private void UpdateCrouchingState(double delta)
	{
		localPlayer.IsCrouching = input.Crouching;
		if (localPlayer.IsCrouching)
		{
			animatedSprite2D.Animation = Globals.PlayerAnimation.Crouch.ToString();
		}
		else
		{
			animatedSprite2D.Animation = Globals.PlayerAnimation.Walk.ToString();
		}
	}

	private void UpdateSprintingState(double delta)
	{
		localPlayer.IsRunning = input.Running;
		if (localPlayer.IsRunning)
		{
			MovementSpeed = baseMovementSpeed * SprintSpeedMultiplier;

			//animatedSprite2D.Animation = Globals.PlayerAnimation.Crouch.ToString();
		}
		else
		{
			MovementSpeed = baseMovementSpeed * 1;

			//animatedSprite2D.Animation = Globals.PlayerAnimation.Walk.ToString();
		}
	}

	private void UpdatePlayerPosition(double delta)
	{
		Vector2 direction = input.Direction.Normalized();
		localPlayer.Velocity = localPlayer.Velocity.Lerp(direction * MovementSpeed, 0.1f);
		localPlayer.Velocity = localPlayer.Velocity * (float)(1 - (Friction * delta));
		localPlayer.MoveAndSlide();
		localPlayer.Position = new Vector2(
			x: Mathf.Clamp(localPlayer.Position.X, 0, Level.ScreenSize.X),
			y: Mathf.Clamp(localPlayer.Position.Y, 0, Level.ScreenSize.Y)
		);
	}

}
