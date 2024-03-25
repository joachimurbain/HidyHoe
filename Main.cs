using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Reflection;

public partial class Main : Node
{

	[Signal]
	public delegate void PlayerListUpdateEventHandler();

	[Export]
	public GameMode[] GameModes;

	public GameMode GameMode { get; set; }
	public Dictionary<int, PlayerInfo> Players { get; set; } = new Dictionary<int, PlayerInfo>();
	private int playerReady { get; set; } = 0;

	private Dictionary<int, int> playerVotes = new Dictionary<int, int>();

	public override void _Ready()
	{
	}


	public void OnStartGameButtonDown()
	{
		StartGame();
	}

	private int playerReadyCount = 0;
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void OnPlayerReadyCheck(int inc)
	{
		if (Multiplayer.IsServer())
		{
			playerReadyCount += inc;
			if (playerReadyCount == Players.Count)
			{
				GetMostVotedGameMode();
				Rpc(nameof(StartGame));
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void StartGame()
	{

		GetNode<Control>("MultiplayerManager/MainMenu").Hide();
		Lobby lobby = (Lobby)GetNodeOrNull<Control>("MultiplayerManager/Lobby");
		if (lobby != null)
		{
			lobby.Hide();
			lobby.ResetReadyButton();
		}
		playerReadyCount = 0;

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
			Multiplayer.MultiplayerPeer.DisconnectPeer(playerId);	
		}

	}



	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RequestPlayerInfos(string callback = "", int id = 0)
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

	public void SendPlayerInfo(string callback = "", int id = 0)
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
		Rpc(nameof(ReceivePlayerInfo), playerinfos, callback, id);

	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReceivePlayerInfo(Array<string> jsonPlayerInfo, string callback, int id = 0)
	{
		Players.Clear();
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
			else
			{
				Rpc(callback);
			}
		}
	}


	private bool endRoundExecuted = false;
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void EndRound(int index)
	{
		Globals.RoundOutcome outcome = (Globals.RoundOutcome)index;
		if (endRoundExecuted || !Multiplayer.IsServer())
		{
			return;
		}
		endRoundExecuted = true;
		Rpc(nameof(PauseGame));
		int seekerScore = outcome == Globals.RoundOutcome.SeekerVictory ? 1 : 0;
		int hiderScore = outcome == Globals.RoundOutcome.HiderVictory ? 1 : 0;
		foreach (PlayerInfo playerInfo in Players.Values)
		{
			int score = playerInfo.Role == PlayerInfo.PlayerRole.Seeker ? seekerScore : hiderScore;
			playerInfo.Score.Add(score);
		}
		if (IsGameOver())
		{
			SendPlayerInfo(nameof(GameOver));
		}
		else
		{
			SetPlayerRoles();
			SendPlayerInfo(nameof(NextRound), 1);
		}
	}

	private bool IsGameOver()
	{
		return GameMode.AmountOfRounds == Players.First().Value.Score.Count;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	async public void GameOver()
	{
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
			ClearGame();
			SendPlayerInfo(nameof(ShowLobby));
			playerReady = 0;
			endRoundExecuted = false;

		}
	}

	public void ClearGame() {
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
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ShowLobby()
	{
		GetTree().Paused = false;
		if (DisplayServer.GetName() != "headless")
		{
			GetNode<Control>("MultiplayerManager/MainMenu").Show();
			GetNode<Control>("MultiplayerManager/Lobby").Show();
		}
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
			Rpc(nameof(UnpauseGame));
			playerReady = 0;
			endRoundExecuted = false;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void PauseGame()
	{
		GetTree().Paused = true;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void UnpauseGame()
	{
		GetTree().Paused = false;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void CastGameModeVote(int gameModeId)
	{
		int playerId = Multiplayer.GetRemoteSenderId();
		if (!Multiplayer.IsServer())
		{
			return;
		}
		// If the player has already voted, update their vote
		if (playerVotes.ContainsKey(playerId))
		{
			playerVotes[playerId] = gameModeId;
		}
		// Otherwise, add a new vote for the player
		else
		{
			playerVotes.Add(playerId, gameModeId);
		}
	}

	public void GetMostVotedGameMode()
	{
		if (!Multiplayer.IsServer())
		{
			return;
		}
		
		if (playerVotes.Count == 0)
		{ 
			RpcId(1,nameof(SetGameMode), 0);
		}
		else
		{
			int mostVotedGameModeID = 0;
			// Count the votes for each game mode
			Dictionary<int, int> modeCounts = new Dictionary<int, int>();
			foreach (var vote in playerVotes)
			{
				int gameModeId = vote.Value;
				if (modeCounts.ContainsKey(gameModeId))
				{
					modeCounts[gameModeId]++;
				}
				else
				{
					modeCounts.Add(gameModeId, 1);
				}
			}
			// Find the game mode with the highest number of votes
			mostVotedGameModeID = modeCounts.OrderByDescending(x => x.Value).First().Key;
			modeCounts.Clear();
			Rpc(nameof(SetGameMode), mostVotedGameModeID);
		}
	}


	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetGameMode(int mostVotedGameModeID)
	{
		GameMode =  GameModes.FirstOrDefault(gameMode => gameMode.Id == (GameModeId)mostVotedGameModeID);
	}

	public void ClearVotes()
	{
		playerVotes.Clear();
	}

}
