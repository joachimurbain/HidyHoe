using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float SpottedDelay = 3.0f;
	[Export]
	public float Friction = 0.5f;
	[Export]
	public int MovementSpeed = 200;

	[Export]
	public int PlayerId { get=>_player; set {
			_player = value;
			GetNode<PlayerInput>("PlayerInput").SetMultiplayerAuthority(_player);
		}
	}
	private int _player = 1;





	private bool Crouched = false;
	private bool IsSpotted = false;
	private Vector2 syncPosition;
	private int boxContact = 0;


	public override void _Ready()
	{
		SetMultiplayerAuthority();
	}

	private void SetMultiplayerAuthority()
	{
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(this.Name));
	}

	public bool IsCurrentPlayer()
	{
		return GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
	}

	public override void _Process(double delta)
	{
		if (IsCurrentPlayer())
		{
			foreach (Player player in GetTree().GetNodesInGroup("Players"))
			{
				if (player.Name == this.Name)
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
					Rpc(nameof(Spotted), player.Name);
				}



			}
		}
		SetCrouchState();
	}


	private void SetCrouchState()
	{
		bool isHidden;
		string animation;
		Color modulateColor = Modulate;
		if (Crouched && IsAgainstObstacle() && !IsSpotted)
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

		//var tween = GetTree().CreateTween();

		SetAnimation(animation);
		if (IsCurrentPlayer())
		{
			//tween.TweenProperty(this, "modulate", modulateColor, 2);
			Modulate = modulateColor;
		}
		else
		{
			//tween.TweenProperty(this, "visible", !isHidden, 2);
			Visible = !isHidden;
		}
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
		if (IsCurrentPlayer())
		{

			if (DevMode)
			{
				if (Multiplayer.GetUniqueId() == 1)
				{
					if (Input.IsActionJustPressed("crouch"))
					{
						Crouched = true;
					}
				}
				else
				{
					Crouched = Input.IsActionPressed("crouch");
				}
			}
			else
			{
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

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	async private void Spotted(string playerName)
	{
		GetParent().GetNode<Player>(playerName).IsSpotted = true;
		await ToSignal(GetTree().CreateTimer(SpottedDelay), SceneTreeTimer.SignalName.Timeout);
		GetParent().GetNode<Player>(playerName).IsSpotted = false;
	}


	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void SetSeeker(string playerName)
	{
		Player playerNode = GetParent().GetNode<Player>(playerName);
		GD.Print($"The Seeker Is : {playerNode.Name}");
		GD.Print($"This player Is : {Multiplayer.GetUniqueId()}");
		playerNode.Modulate = new Color(255, 0, 0, 1);
		//if (IsCurrentPlayer())
		//{

			

			if (playerNode.Name == Multiplayer.GetUniqueId().ToString())
			{
				GetNode<HUD>("HUD").ShowMessage("SEEKER");
			}
			else {
				GetNode<HUD>("HUD").ShowMessage("Hider");
			}
		//}
		

	}


}