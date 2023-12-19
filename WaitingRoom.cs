using Godot;
using System;
using System.Reflection.Emit;

public partial class WaitingRoom : Control
{

	[Signal]
	public delegate void ReadyButtonDownEventHandler();
	[Signal]
	public delegate void CancelButtonDownEventHandler();


	private ItemList playerList;
	private MultiplayerController mainNode;
	public override void _Ready()
	{
		playerList = FindChild("ItemList") as ItemList;
		playerList.Clear();
		mainNode = FindParent("Main") as MultiplayerController;
	}

	public override void _Process(double delta)
	{
	}

	public void OnCloseButton()
	{
		if((FindChild("CancelButton") as Button).Visible)
		{
			EmitSignal(SignalName.CancelButtonDown, -1);
		}
		mainNode.PlayerDisconnect(Multiplayer.GetUniqueId());
		QueueFree();
	}



	public void RefreshPlayers()
	{
		playerList.Clear();
		foreach (Godot.Collections.Dictionary player in mainNode.PlayerList.Values)
		{
			playerList.AddItem((string)player["Name"]);
		}
	}

	public void OnReadyButtonDown(NodePath readyButtonPath,NodePath cancelButtonPath)
	{
		EmitSignal(SignalName.ReadyButtonDown,1);
		ToggleReadinessButton(readyButtonPath, cancelButtonPath);

	}

	public void OnCancelButtonDown(NodePath readyButtonPath, NodePath cancelButtonPath)
	{
		EmitSignal(SignalName.CancelButtonDown,-1);
		ToggleReadinessButton(readyButtonPath, cancelButtonPath);

	}


	private void ToggleReadinessButton(NodePath readyButtonPath, NodePath cancelButtonPath)
	{

		Control readyButton = GetNode<Control>(readyButtonPath);
		Control cancelButton = GetNode<Control>(cancelButtonPath);

		readyButton.Visible = !readyButton.Visible;
		cancelButton.Visible = !cancelButton.Visible;
	}
}
