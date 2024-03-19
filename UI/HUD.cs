using Godot;
using System;

public partial class HUD : CanvasLayer
{

	private Player localPlayer {
		get => GetParent<Player>();
	}

	private bool initialized = false;

	public override void _Ready()
	{
		SetProcess(localPlayer.PlayerId == Multiplayer.GetUniqueId());
		Hide();		
	}

	public override void _Process(double delta)
	{
		if(!initialized)
		{
			initialized = true;
			Show();
		}
		GetNode<ProgressBar>("Stamina/ProgressBar").Value = localPlayer.CurrentStamina;
	}


	public void ShowMessage(string text)
	{
		var message = GetNode<Label>("Message");
		message.Text = text;
		message.Show();
		GetNode<Timer>("MessageTimer").Start();
	}

	private void OnMessageTimerTimeout()
	{
		GetNode<Label>("Message").Hide();
	}



}
