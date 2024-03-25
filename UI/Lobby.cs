using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static Godot.Projection;

public partial class Lobby : Control
{
	private int playerReadyCount = 0;
	private ItemList playerList;
	private Main mainNode;
	private Button readyButton;
	private Button cancelButton;
	private OptionButton gameModeDropDown;
	public override void _Ready()
	{
		playerList = FindChild("ItemList") as ItemList;
		playerList.Clear();
		mainNode = FindParent("Main") as Main;
		readyButton = FindChild("ReadyButton") as Button;
		cancelButton = FindChild("CancelButton") as Button;
		mainNode.PlayerListUpdate += RefreshPlayers;
		RefreshPlayers();
		gameModeDropDown = FindChild("GameModeDropDown") as OptionButton;
		for (int i = 0; i < mainNode.GameModes.Length; i++)
		{
			gameModeDropDown.AddItem(mainNode.GameModes[i].Name, (int)mainNode.GameModes[i].Id);
		}
		OnGameModeSelected(0);
	}

	public override void _Process(double delta)
	{
	}

	public void OnCloseButton()
	{
		if ((FindChild("CancelButton") as Button).Visible)
		{
			OnCancelButtonDown();
		}
		mainNode.PlayerDisconnect(Multiplayer.GetUniqueId());
		QueueFree();
	}

	public void OnGameModeSelected(int gameModeId)
	{
		mainNode.RpcId(1, nameof(mainNode.CastGameModeVote), gameModeId);
	}

	public void RefreshPlayers()
	{
		GD.Print("Refreshing for "+Multiplayer.GetUniqueId());
		playerList.Clear();

		foreach (PlayerInfo player in mainNode.Players.Values)
		{
			playerList.AddItem((string)player.Name);
		}
	}

	public void OnReadyButtonDown()
	{
		readyButton.Visible = false;
		cancelButton.Visible = true;
		mainNode.RpcId(1, nameof(Main.OnPlayerReadyCheck), 1);
	}

	public void OnCancelButtonDown()
	{
		if(cancelButton.Visible)
		{
			mainNode.RpcId(1, nameof(Main.OnPlayerReadyCheck), -1);
		}
		readyButton.Visible = true;
		cancelButton.Visible = false;
	}

	public void ResetReadyButton()
	{
		readyButton.Visible = true;
		cancelButton.Visible = false;
	}

	public override void _ExitTree()
	{
		mainNode.PlayerListUpdate -= RefreshPlayers;
		base._ExitTree();
	}



}
