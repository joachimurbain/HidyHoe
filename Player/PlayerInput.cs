using Godot;

public partial class PlayerInput : MultiplayerSynchronizer
{
	[Export]
	public bool Crouching = false;
	[Export]
	public bool Sprinting = false;
	[Export]
	public Vector2 Direction = new Vector2();
	public override void _Ready()
	{
		SetProcess(GetMultiplayerAuthority()==Multiplayer.GetUniqueId());
	}

	public override void _Process(double delta)
	{
		Direction = Input.GetVector("left", "right", "up", "down");
		Crouching = Input.IsActionPressed("crouch");
		Sprinting = Input.IsActionPressed("sprint");
	}
}
