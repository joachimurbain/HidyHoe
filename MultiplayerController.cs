using Godot;
using Godot.Collections;
using System;


public partial class MultiplayerController : Node
{

	[Signal]
	public delegate void PlayerListUpdateEventHandler();

	[Export]
	public GameModeId GameModeId;
	[Export]
	public int PlayerReadyCount = 0;
	[Export]
	public int Port = 8910;
	[Export]
	public string Address = "127.0.0.1";
	//private string Address = "151.80.43.66";
	private ENetMultiplayerPeer peer;
	private ENetConnection.CompressionMode compressionMode = ENetConnection.CompressionMode.RangeCoder;
	private Dictionary<int, PlayerInfo> _players = new Dictionary<int, PlayerInfo>();
	public Dictionary<int, PlayerInfo> Players
	{
		get => _players;
		set => SetPlayers(value);
	}

	private void SetPlayers(Dictionary<int, PlayerInfo> value)
	{
		_players = value;
		EmitSignal(SignalName.PlayerListUpdate);
	}

	public override void _Ready()
	{
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		Multiplayer.PeerDisconnected += PeerDisconnected;

		if (DisplayServer.GetName() == "headless")
		{
			CallDeferred(nameof(HostGame));
		}
	}

	private void HostGame()
	{
		//	FIXME: Cant reset hosting
		//if(peer != null) 
		//{
		//	peer.Close();
		//	peer = null;
		//	Multiplayer.MultiplayerPeer.Close();
		//	GetTree().SetMultiplayer(MultiplayerApi.CreateDefaultInterface());
		//}

		peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(Port);
		if (error != Error.Ok)
		{
			GD.PrintErr($"ERROR CANNOT HOST! : {error.ToString()}");
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting For Players!");
		if (DisplayServer.GetName() != "headless")
		{
			AddLobby();
			SendPlayerInformation((FindChild("NameLineEdit") as LineEdit).Text, 1);
		}
	}

	private void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
	}


	private void PeerDisconnected(long id)
	{
		GD.Print("PEER DISCONECTED" + id.ToString());
	}

	private void ConnectedToServer()
	{
		AddLobby();
		SendPlayerInformation((FindChild("NameLineEdit") as LineEdit).Text, Multiplayer.GetUniqueId());
		GD.Print("Connected to Server!");
	}

	public void OnHostButtonDown()
	{
		HostGame();
	}

	public void OnJoinButtonDown()
	{
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(Address, Port);
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining Game!");
	}

	public void OnStartGameButtonDown()
	{
		StartGame();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartGame()
	{

		GetNode<Control>("MainMenu").Hide();
		Lobby lobby = (Lobby)GetNode<Control>("Lobby");
		lobby.OnCancelButtonDown();
		lobby.Hide();
		GetTree().Paused = false;
		if (Multiplayer.IsServer())
		{
			SetPlayerRoles();
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
	private void SendPlayerInformation(string name, int id)
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
	public void OnPlayerReadyCheck(int inc)
	{
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(OnPlayerReadyCheck), inc);
		}
		else
		{
			PlayerReadyCount += inc;
			if (PlayerReadyCount == Players.Count)
			{

				Rpc(nameof(StartGame));
			}
		}

	}

	public void OnGameModeSelected(GameMode gameMode)
	{
		GameModeId = gameMode.Id;
	}


	

	public void AddLobby()
	{
		PackedScene scene = ResourceLoader.Load<PackedScene>("res://UI/Lobby.tscn");
		Lobby lobby = scene.Instantiate() as Lobby;
		//PlayerInfoUpdated += lobby.RefreshPlayers;
		Connect(SignalName.PlayerListUpdate, new Callable(lobby, nameof(lobby.RefreshPlayers)));
		lobby.Connect(Lobby.SignalName.ReadyButtonDown, new Callable(this, nameof(OnPlayerReadyCheck)));
		lobby.Connect(Lobby.SignalName.CancelButtonDown, new Callable(this, nameof(OnPlayerReadyCheck)));
		lobby.Connect(Lobby.SignalName.GamemodeSelected, new Callable(this, nameof(OnGameModeSelected)));
		AddChild(lobby);
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
	public void RequestPlayerInfos()
	{
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(RequestPlayerInfos));
		}
		SendPlayerInfo();
	}

	public void SendPlayerInfo()
	{
		if (!Multiplayer.IsServer())
		{
			return;
		}

		foreach (PlayerInfo playerInfo in Players.Values)
		{
			Rpc(nameof(ReceivePlayerInfo), playerInfo.ToJsonString());
		}

	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReceivePlayerInfo(string jsonPlayerInfo)
	{
		PlayerInfo playerInfo= PlayerInfo.FromJsonString(jsonPlayerInfo);
		Players[playerInfo.Id] = playerInfo;
		UpdatePlayers();
	}

	public void UpdatePlayers() { 
		Players = Players.Duplicate(); // Trigger Setter
	}

	async public void EndRound(Globals.RoundOutcome outcome)
	{
		GetTree().Paused = true;
		int seekerScore = outcome == Globals.RoundOutcome.SeekerVictory ? 1 : 0;
		int hiderScore = outcome == Globals.RoundOutcome.HiderVictory ? 1 : 0;
		GameMode gameMode = GD.Load<GameMode>($"res://GameModes/{GameModeId}.tres");


		bool gameOver = false;
		foreach (PlayerInfo playerInfo in Players.Values)
		{
			int score = playerInfo.Role == PlayerInfo.PlayerRole.Seeker ? seekerScore : hiderScore;
			playerInfo.Score.Add(score);

			if (gameMode.AmountOfRounds == playerInfo.Score.Count)
			{
				//Array<Node> players = GetTree().GetNodesInGroup("Players");
				//foreach (Player player in players)
				//{
				//	player.PlayerId = 1;
				//}
				//playerInfo.GameOver = true;
					gameOver = true;
			}

		}

		if (gameOver)
		{

			SendPlayerInfo();
			Array<Node> HUDS = GetTree().GetNodesInGroup("HUD");
			foreach (HUD HUD in HUDS)
			{
				HUD.ShowMessage($"Game Over");
				//RoundCounter x = (RoundCounter)HUD.FindChild("RoundCounter");
			}
			await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);

			var players = GetNode("LevelContainer/Level/Players").GetChildren();



			Array<Node> playerInputs = GetTree().GetNodesInGroup("PlayerInput");
			foreach (Node2D player in players)
			{
				MultiplayerSynchronizer playerInput = player.GetNode<MultiplayerSynchronizer>("MovementComponent/PlayerInput");
				playerInput.PublicVisibility = false;
			}

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

			if (Multiplayer.IsServer()) { 
				Node level = GetNode("LevelContainer");
				foreach (Node child in level.GetChildren())
				{
					level.RemoveChild(child);
					child.QueueFree();
				}
			}

			GetNode<Control>("MainMenu").Show();
			GetNode<Control>("Lobby").Show();


			foreach (PlayerInfo playerInfo in Players.Values)
			{
				playerInfo.Score.Clear();
				playerInfo.GameOver = false;
				playerInfo.Role = PlayerInfo.PlayerRole.None;
			}
			await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
			SendPlayerInfo(); // would need to disconect from refreshCounter
			GetTree().Paused = false;
		}
		else {
			SetPlayerRoles();
			SendPlayerInfo();
			if(Multiplayer.IsServer())
			{
				CallDeferred(nameof(ChangeLevel), ResourceLoader.Load<PackedScene>("res://Level.tscn"));
			}
			GetTree().Paused = false;
		}





	}
	



}
