using Godot;
using Godot.Collections;
using System;
using System.Reflection.Emit;

public partial class Lobby : Control
{

	[Signal]
	public delegate void ReadyButtonDownEventHandler();
	[Signal]
	public delegate void CancelButtonDownEventHandler();
	[Signal]
	public delegate void GamemodeSelectedEventHandler();

	[Export]
	private GameMode[] gameModes;

	private ItemList playerList;
	private MultiplayerController mainNode;

	private Button readyButton;
	private Button cancelButton;

	private OptionButton gamemodeDropDown;
	public override void _Ready()
	{
		playerList = FindChild("ItemList") as ItemList;
		playerList.Clear();
		mainNode = FindParent("Main") as MultiplayerController;
		readyButton = FindChild("ReadyButton") as Button;
		cancelButton = FindChild("CancelButton") as Button;

		RefreshPlayers();
		// Make the drop down visible only for host/first or make it a vote?
		if (Multiplayer.IsServer()) { 
			gamemodeDropDown = FindChild("GamemodeDropDown") as OptionButton;
			for (int i = 0; i < gameModes.Length; i++)
			{
				gamemodeDropDown.AddItem(gameModes[i].Name);
			}
			OnGamemodeSelected(0);
		}
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

	public void OnGamemodeSelected(int index)
	{	
		EmitSignal(SignalName.GamemodeSelected, gameModes[index]);
	}


	public void RefreshPlayers()
	{
		playerList.Clear();

		foreach (PlayerInfo player in mainNode.Players.Values)
		{
			playerList.AddItem((string)player.Name);
		}
	}

	public void OnReadyButtonDown()
	{
		EmitSignal(SignalName.ReadyButtonDown,1);
		ToggleReadinessButton();

	}

	public void OnCancelButtonDown()
	{
		EmitSignal(SignalName.CancelButtonDown,-1);
		ToggleReadinessButton();

	}


	private void ToggleReadinessButton()
	{



		readyButton.Visible = !readyButton.Visible;
		cancelButton.Visible = !cancelButton.Visible;
	}
}
