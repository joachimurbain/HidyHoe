using Godot;
using System;


public partial class Countdown : Control
{


	private double time =30;
	private int minutes = 0;
	private int seconds = 0;
	private int msec = 0;

	private Label minutesNode;
	private Label secondsNode;
	private Label msecNode;
	private Main mainNode
	{
		get => FindParent("Main") as Main;
	}

	public override void _Ready()
	{
		minutesNode = FindChild("MinuteLabel") as Label;
		secondsNode = FindChild("SecondLabel") as Label;
		msecNode = FindChild("MsecLabel") as Label;

		time = mainNode.GameMode.RoundDuration;

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
