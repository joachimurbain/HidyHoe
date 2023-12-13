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
