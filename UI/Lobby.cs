using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static Godot.Projection;

public partial class Lobby : Control
{

	//[Signal]
	//public delegate void ReadyButtonDownEventHandler(int inc);
	//[Signal]
	////public delegate void CancelButtonDownEventHandler(int inc);
	//[Signal]
	//public delegate void GamemodeSelectedEventHandler(GameMode gameMode);

	[Export]
	private GameMode[] gameModes;

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

		mainNode.GameMode = gameModes[0];


		RefreshPlayers();
		gameModeDropDown = FindChild("GameModeDropDown") as OptionButton;
		if (Multiplayer.IsServer())
		{
			for (int i = 0; i < gameModes.Length; i++)
			{
				gameModeDropDown.AddItem(gameModes[i].Name);
			}
			OnGameModeSelected(0);
		}
		else { 
			gameModeDropDown.Visible= false;
		}


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

	public void OnGameModeSelected(int index)
	{
		Rpc(nameof(SetGameMode), index);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetGameMode(int index)
	{
		mainNode.GameMode = gameModes[index];
		GD.Print(mainNode.GameMode.Name);
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
		readyButton.Visible = false;
		cancelButton.Visible = true;
		mainNode.RpcId(1, nameof(Main.OnPlayerReadyCheck), 1);
	}

	public void OnCancelButtonDown()
	{
		readyButton.Visible = true;
		cancelButton.Visible = false;
		mainNode.RpcId(1, nameof(Main.OnPlayerReadyCheck), -1);
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
