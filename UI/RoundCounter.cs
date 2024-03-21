using Godot;

public partial class RoundCounter : Control
{
	[Export]
	private Texture2D roundTexture;
	[Export]
	private Texture2D victoryTexture;
	[Export]
	private Texture2D defeatTexture;
	private Control markerContainer;
	private Main mainNode;
	public override void _Ready()
	{

		mainNode = FindParent("Main") as Main;

		mainNode.PlayerListUpdate += RefreshCounter;


		//mainNode.Connect(MultiplayerController.SignalName.PlayerListUpdate, new Callable(this, nameof(RefreshCounter)));
		markerContainer = FindChild("HBoxContainer") as Control;

		for (int i = 0; i < mainNode.GameMode.AmountOfRounds; i++)
		{
			TextureRect marker = new TextureRect();
			marker.Name = $"Round{i + 1}";
			marker.Texture = roundTexture;
			marker.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			markerContainer.AddChild(marker);
		}
		RefreshCounter();
	}

	public override void _Process(double delta)
	{ }

		public void RefreshCounter()
	{
		if (IsProcessing() && mainNode.Players.ContainsKey(Multiplayer.GetUniqueId())) { 
			PlayerInfo client = mainNode.Players[Multiplayer.GetUniqueId()];
			for(int i = 0;i < client.Score.Count;i++)
			{
				Texture2D texture = victoryTexture;
				if (client.Score[i] == 0)
				{
					texture = defeatTexture;
				}
				SetMarkerTexture(i,texture);
			}
		}
	}










	private void SetMarkerTexture(int index,Texture2D texture)
	{
		TextureRect marker = markerContainer.GetChild(index + 1) as TextureRect;
		marker.Texture = texture;
	}


	public override void _ExitTree()
	{
		mainNode.PlayerListUpdate -= RefreshCounter;
		base._ExitTree();
	}
}
