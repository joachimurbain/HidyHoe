using Godot;
using System;

public partial class HUD : CanvasLayer
{
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
