using Godot;
using System;

public partial class PlayerInput : MultiplayerSynchronizer
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public bool Crouching = false;
	[Export]
	public Vector2 Direction = new Vector2();
	public override void _Ready()
	{
		SetProcess(GetMultiplayerAuthority()==Multiplayer.GetUniqueId());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Direction = Input.GetVector("left", "right", "up", "down");
		if (Input.IsActionJustPressed("crouch"))
		{
			Rpc(nameof(Crouch));
		}



	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Crouch()
	{
		Crouching=true;
	}

}
