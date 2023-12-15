using Godot;
using System;

public partial class HUD : CanvasLayer
{

	private Player localPlayer {
		get => GetParent<Player>();
	}
	private ProgressBar staminaBar;

	public override void _Ready()
	{

		/*
		
		make use of multiplayer sync?

				MultiplayerSynchronizer multiplayerSynchronizer = new MultiplayerSynchronizer();
		multiplayerSynchronizer.AddVisibilityFilter("")
		
		*/

		if (Multiplayer.GetUniqueId() != localPlayer.PlayerId)
		{
			Hide();
		}
		else
		{
			staminaBar = GetNode<ProgressBar>("Stamina/ProgressBar");
		}
		SetProcess(localPlayer.PlayerId == Multiplayer.GetUniqueId());
	}

	public override void _Process(double delta)
	{
			staminaBar.Value = localPlayer.CurrentStamina;
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
