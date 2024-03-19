using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Godot.TextServer;

public partial class RoundCounter : Control
{
	[Export]
	private Texture2D roundTexture;
	[Export]
	private Texture2D victoryTexture;
	[Export]
	private Texture2D defeatTexture;
	private Control markerContainer;
	private MultiplayerController mainNode;
	public override void _Ready()
	{

		mainNode = FindParent("Main") as MultiplayerController;
		mainNode.Connect(MultiplayerController.SignalName.PlayerListUpdate, new Callable(this, nameof(RefreshCounter)));
		markerContainer = FindChild("HBoxContainer") as Control;


		GameMode gameMode = GD.Load<GameMode>($"res://GameModes/{mainNode.GameModeId}.tres");

		for (int i = 0; i < gameMode.AmountOfRounds; i++)
		{
			TextureRect marker = new TextureRect();
			marker.Name = $"Round{i + 1}";
			marker.Texture = roundTexture;
			marker.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
			markerContainer.AddChild(marker);
		}
		RefreshCounter();
	}

	public void RefreshCounter()
	{
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










	private void SetMarkerTexture(int index,Texture2D texture)
	{
		TextureRect marker = markerContainer.GetChild(index + 1) as TextureRect;
		marker.Texture = texture;
	}
}
