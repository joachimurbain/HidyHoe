using Godot;
using System;
using static PlayerInfo;

public partial class MovementComponent : Node
{
	[Export]
	public float Friction = 0.5f;
	[Export]
	public float MovementSpeed = 200f;
	[Export]
	public float MovementSpeedMultiplier = 1f;
	[Export]
	public float CrouchSpeedMultiplier = 0.25f;
	[Export]
	public float SprintSpeedMultiplier = 2f;
	[Export]
	public float SpeedBoostMultiplier = 1.15f;
	[Export]
	public float SpeedBoostDuration = 1f;


	private PlayerInput input { get; set; }
	private AnimatedSprite2D animatedSprite2D;
	private GpuParticles2D dustTrail;
	private float movementSpeedMultiplier;
	private float calculatedMovementSpeed { get; set; }

	private Player playerNode
	{
		get => GetParent<Player>();
	}


	public override void _Ready()
	{
		animatedSprite2D = playerNode.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		dustTrail = GetNode<GpuParticles2D>("Effects/DustTrail");
		input = GetNode<PlayerInput>("PlayerInput");
	}

	public override void _Process(double delta)
	{
		//if(playerNode.IsRunning && !playerNode.Velocity.IsZeroApprox() && playerNode.CurrentStamina>0)
		//{
		//	if (!dustTrail.Emitting)
		//	{
		//		dustTrail.Restart();
		//	}
		//}
		//else
		//{
		//	dustTrail.Emitting = false;
		//}
	}

	public override void _PhysicsProcess(double delta)
	{
		UpdateCrouchingState(delta);
		UpdateSprintingState(delta);
		UpdateChaseModeState(delta);
		SetPlayerSpeed();
		UpdatePlayerPosition(delta);
	}

	private void UpdateCrouchingState(double delta)
	{
		playerNode.IsCrouching = input.Crouching;
		if (playerNode.IsCrouching)
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

		if (playerNode.CurrentStamina < 1  || ( Math.Min(Math.Abs(playerNode.Velocity.X),1) < 1 && Math.Min(Math.Abs(playerNode.Velocity.Y),1) < 1))
		{
			playerNode.IsRunning = false;
		}
		else { 
			playerNode.IsRunning = input.Running;
		}
		if (playerNode.IsRunning)
		{
			//Set animation for running
			if (!dustTrail.Emitting)
			{
				dustTrail.Restart();
			}
		}
		else
		{
			//Unset animation for running
			dustTrail.Emitting = false;
		}
	}

	private void UpdateChaseModeState(double delta)
	{
		if(playerNode.Role != PlayerInfo.PlayerRole.Seeker)
		{
			return;
		}
		if(playerNode.IsInChaseMode)
		{
			SpeedBoost();
		}
	}

	private void SetPlayerSpeed()
	{
		movementSpeedMultiplier = MovementSpeedMultiplier;
		if (playerNode.IsRunning)
		{
			movementSpeedMultiplier *= SprintSpeedMultiplier;
		}
		if (playerNode.IsCrouching)
		{
			movementSpeedMultiplier *= CrouchSpeedMultiplier;
		}
		if (playerNode.IsBoosted)
		{
			movementSpeedMultiplier *= SpeedBoostMultiplier;
		}
		calculatedMovementSpeed = MovementSpeed * movementSpeedMultiplier;
	}

	private void UpdatePlayerPosition(double delta)
	{
		Vector2 direction = input.Direction.Normalized();
		playerNode.Velocity = playerNode.Velocity.Lerp(direction * calculatedMovementSpeed, 0.1f);
		playerNode.Velocity = playerNode.Velocity * (float)(1 - (Friction * delta));
		playerNode.MoveAndSlide();
		playerNode.Position = new Vector2(
			x: Mathf.Clamp(playerNode.Position.X, 0, Level.ScreenSize.X),
			y: Mathf.Clamp(playerNode.Position.Y, 0, Level.ScreenSize.Y)
		);
	}

	public void SpeedBoost()
	{
		Timer speedBoostTimer = GetNode<Timer>("SpeedBoostTimer");
		playerNode.IsBoosted = true;
		if (!speedBoostTimer.IsStopped())
		{
			speedBoostTimer.Stop();
		}
		speedBoostTimer.WaitTime = SpeedBoostDuration;
		speedBoostTimer.Start();
	}

	public void OnSpeedBoostTimerTimeout()
	{
		playerNode.IsBoosted = false;
	}

}
