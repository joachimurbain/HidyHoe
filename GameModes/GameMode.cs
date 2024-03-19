using Godot;
using System;

[GlobalClass]
public partial class GameMode : Resource
{
	[Export] public GameModeId Id { get; set; }
	[Export] public string Name { get; set; }
	[Export] public int AmountOfRounds { get; set; }
}

public enum GameModeId
{
	Bo3,
	Bo5
}
