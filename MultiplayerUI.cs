using Godot;
using System;

public partial class MultiplayerUI : Control
{
	[Signal]
	public delegate void HostButtonPressedEventHandler();
	[Signal]
	public delegate void JoinButtonPressedEventHandler();
	[Signal]
	public delegate void StartGameButtonPressedEventHandler();




	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnHostButtonDown()
	{
		EmitSignal(SignalName.HostButtonPressed);
	}

	public void OnJoinButtonDown()
	{
		EmitSignal(SignalName.JoinButtonPressed);
	}

	public void OnStartGameButtonDown()
	{
		EmitSignal(SignalName.StartGameButtonPressed);
	}
}
