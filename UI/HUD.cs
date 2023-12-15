using Godot;
using System;

public partial class HUD : CanvasLayer
{

	private Player localPlayer;
	private ProgressBar staminaBar;

	public override void _Ready()
	{
		localPlayer = GetParent<Player>();
		staminaBar = GetNode<ProgressBar>("Stamina/ProgressBar");
	}

	public override void _Process(double delta)
	{
		

        if (Multiplayer.GetUniqueId() == localPlayer.PlayerId)
		{

			GD.Print(localPlayer.PlayerId + " | " +localPlayer.CurrentStamina);
			staminaBar.Value = localPlayer.CurrentStamina;
		}
		else
		{
			Hide(); // TMP FIX. Move HUD out of player ? or use sync visibilty ?
		}
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
