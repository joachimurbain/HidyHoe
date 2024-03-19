using Godot;
using System;


public partial class Countdown : Control
{
	//[Signal]
	//public delegate void OnCountdownTimeoutEventHandler();

	[Export]
	public double time =2;
	private int minutes = 0;
	private int seconds = 0;
	private int msec = 0;

	private Label minutesNode;
	private Label secondsNode;
	private Label msecNode;
	private MultiplayerController mainNode
	{
		get => FindParent("Main") as MultiplayerController;
	}

	public override void _Ready()
	{

		minutesNode = FindChild("MinuteLabel") as Label;
		secondsNode = FindChild("SecondLabel") as Label;
		msecNode = FindChild("MsecLabel") as Label;
		SetProcess(GetNode<Player>("../../").PlayerId == Multiplayer.GetUniqueId());
	}

	public override void _Process(double delta)
	{
		

		time -= delta;
		minutes = Math.Max(0,(int)(time / 60));
		seconds = Math.Max(0, (int)(time % 60));
		msec = Math.Max(0, (int)((time * 1000) % 1000));
		minutesNode.Text = $"{minutes:D2}:";
		secondsNode.Text = $"{seconds:D2}:";
		msecNode.Text = $"{msec:D3}";

		if (time < 0)
		{
			stop();
			OnCountdownTimeout();
			//EmitSignal(SignalName.OnCountdownTimeout);
		}

	}

	public void stop()
	{
		SetProcess(false);
	}

	public void start()
	{
		SetProcess(true);
	}

	private void OnCountdownTimeout()
	{
		mainNode.EndRound(Globals.RoundOutcome.HiderVictory);
	}
}
