using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;


public partial class PlayerInfo : GodotObject
{
	public string Name { get; set; }
	public int Id { get; set; }
	public PlayerRole Role { get; set; }
	public List<int> Score { get; set; }
	public bool GameOver { get; set; }

	public PlayerInfo()
	{
		Name = "";
		Id = 0;
		Role = PlayerRole.None;
		Score = new List<int>();
		GameOver = false;
	}

	public string ToJsonString()
	{
		Dictionary<string, object> data = new Dictionary<string, object>
		{
			{ "Name", Name },
			{ "Id", Id },
			{ "Role", Role },
			{ "Score", Score },
			{ "GameOver",GameOver }
		};
		return JsonSerializer.Serialize(data);
	}

	public static PlayerInfo FromJsonString(string jsonString)
	{
		try
		{
			return JsonSerializer.Deserialize<PlayerInfo>(jsonString);
		}
		catch (JsonException ex)
		{
			// Handle JSON parsing errors
			GD.Print($"Error deserializing JSON: {ex.Message}");
			return null;
		}
	}

	public enum PlayerRole
	{
		None,
		Seeker,
		Hider
	}
}
