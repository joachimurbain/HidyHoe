using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class Main : Node
{

	[Signal] 
	public delegate void PlayerListUpdateEventHandler();
	public GameMode GameMode { get; set; }
	private int playerReady { get; set; } = 0;
	public Dictionary<int, PlayerInfo> Players { get; set; } = new Dictionary<int, PlayerInfo>();

	public override void _Ready()
	{
	}


	public void OnStartGameButtonDown()
	{
		StartGame();
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void StartGame()
	{

		GetNode<Control>("MultiplayerManager/MainMenu").Hide();
		Lobby lobby = (Lobby)GetNode<Control>("MultiplayerManager/Lobby");
		lobby.Hide();
		lobby.OnCancelButtonDown();
		GetTree().Paused = false;
		if (Multiplayer.IsServer())
		{
			SetPlayerRoles();
			SendPlayerInfo();
			CallDeferred(nameof(ChangeLevel), ResourceLoader.Load<PackedScene>("res://Level.tscn"));
		}
	}

	public void SetPlayerRoles()
	{

		int seekerIndex = new Random().Next(Players.Count);
		int index = 0;
		foreach (PlayerInfo player in Players.Values)
		{
			if (index == seekerIndex)
			{
				player.Role = PlayerInfo.PlayerRole.Seeker;
			}
			else
			{
				player.Role = PlayerInfo.PlayerRole.Hider;
			}
			index++;
		}
	}

	private void ChangeLevel(PackedScene scene)
	{
		Node level = GetNode("LevelContainer");
		foreach (Node child in level.GetChildren())
		{
			level.RemoveChild(child);
			child.QueueFree();

		}
		level.AddChild(scene.Instantiate());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void SendPlayerInformation(string name, int id)
	{
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(SendPlayerInformation), name, id);
		}
		else
		{
			PlayerInfo playerInfo = new PlayerInfo()
			{
				Name = name,
				Id = id,
			};

			Players[id] = playerInfo;
			SendPlayerInfo();
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void PlayerDisconnect(int playerId)
	{
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(PlayerDisconnect), playerId);
		}
		else
		{
			Players.Remove(playerId);
			Multiplayer.MultiplayerPeer.DisconnectPeer(playerId);
		}

	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RequestPlayerInfos(string callback = "",int id = 0)
	{
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(RequestPlayerInfos), callback);
		}
		else
		{
			SendPlayerInfo(callback);
		}
	}

	public void SendPlayerInfo(string callback = "",int id=0)
	{
		if (!Multiplayer.IsServer())
		{
			return;
		}
		Array<string> playerinfos = new Array<string>();

		foreach (PlayerInfo playerInfo in Players.Values)
		{
			playerinfos.Add(playerInfo.ToJsonString());
		}
		Rpc(nameof(ReceivePlayerInfo), playerinfos, callback,id);

	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReceivePlayerInfo(Array<string> jsonPlayerInfo, string callback, int id = 0)
	{
		for (int i = 0; i < jsonPlayerInfo.Count; i++)
		{
			PlayerInfo playerInfo = PlayerInfo.FromJsonString(jsonPlayerInfo[i]);
			Players[playerInfo.Id] = playerInfo;
		}
		EmitSignal(SignalName.PlayerListUpdate);
		if (callback != "")
		{
			if (id != 0)
			{
				RpcId(id, callback);
			}
			else {
				Rpc(callback);
			}
		}
	}

	public void EndRound(Globals.RoundOutcome outcome)
	{
		if (!Multiplayer.IsServer())
		{
			return;
		}
		int seekerScore = outcome == Globals.RoundOutcome.SeekerVictory ? 1 : 0;
		int hiderScore = outcome == Globals.RoundOutcome.HiderVictory ? 1 : 0;
		foreach (PlayerInfo playerInfo in Players.Values)
		{
			int score = playerInfo.Role == PlayerInfo.PlayerRole.Seeker ? seekerScore : hiderScore;
			playerInfo.Score.Add(score);
		}
		if (IsGameOver())
		{
			RequestPlayerInfos(nameof(GameOver));
		}
		else
		{
			SetPlayerRoles();
			SendPlayerInfo(nameof(NextRound),1);
		}
	}

	private bool IsGameOver()
	{
		return GameMode.AmountOfRounds == Players.First().Value.Score.Count;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	async private void GameOver()
	{
		GetTree().Paused = true;
		Array<Node> playerNodes = GetTree().GetNodesInGroup("Players");
		foreach (Player player in playerNodes)
		{
			if (player.PlayerId != Multiplayer.GetUniqueId())
			{
				continue;
			}
			player.GetNode<HUD>("HUD").ShowMessage($"Game Over");
		}
		await ToSignal(GetTree().CreateTimer(4.0), SceneTreeTimer.SignalName.Timeout);
		FixPlayerInputThrowingError();
		RpcId(1, nameof(PrepareToReturnToLobby));
	}

	async private void FixPlayerInputThrowingError()
	{
		Array<Node> synchronizers = GetTree().GetNodesInGroup("Multiplayer Synchronizer");
		foreach (MultiplayerSynchronizer synchronizer in synchronizers)
		{
			synchronizer.PublicVisibility = false;
		}
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void PrepareToReturnToLobby()
	{	
		if (!Multiplayer.IsServer())
		{
			return;
		}
		playerReady++;
		if (playerReady == Players.Count)
		{
			Node level = GetNode("LevelContainer");
			foreach (Node child in level.GetChildren())
			{
				level.RemoveChild(child);
				child.QueueFree();
			}

			foreach (PlayerInfo playerInfo in Players.Values)
			{
				playerInfo.Score.Clear();
				playerInfo.GameOver = false;
				playerInfo.Role = PlayerInfo.PlayerRole.None;
			}
			SendPlayerInfo(nameof(ShowLobby));
			playerReady = 0;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ShowLobby()
	{
		GetTree().Paused = false;
		GetNode<Control>("MultiplayerManager/MainMenu").Show();
		GetNode<Control>("MultiplayerManager/Lobby").Show();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	async public void NextRound()
	{
		if (!Multiplayer.IsServer())
		{
			return;
		}
		playerReady++;
		if (playerReady == Players.Count)
		{
			await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
			CallDeferred(nameof(ChangeLevel), ResourceLoader.Load<PackedScene>("res://Level.tscn"));
			playerReady = 0;
		}
	}
}
