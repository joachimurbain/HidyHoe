using Godot;
using System;

public partial class StaminaComponent : Node
{
	[Export]
	public double MaxStamina = 100;
	[Export]
	public double StaminaDrainRate = 50;
	[Export]
	public double StaminaRegenerationRate = 10;
	private Player localPlayer
	{
		get => GetParent<Player>();
	}

	public override void _Ready()
	{
		localPlayer.CurrentStamina = MaxStamina;
	}

	public override void _Process(double delta)
	{
		if (localPlayer.IsRunning && !localPlayer.Velocity.IsZeroApprox())
		{
			DeductStamina(StaminaDrainRate * delta);
		}
		else
		{
			RegenStamina(StaminaRegenerationRate * delta);
		}
	}

	private void DeductStamina(double amount)
	{
		localPlayer.CurrentStamina = Mathf.Clamp(localPlayer.CurrentStamina - amount, 0, MaxStamina);
	}

	private void RegenStamina(double amount)
	{
		localPlayer.CurrentStamina = Mathf.Clamp(localPlayer.CurrentStamina + amount, 0, MaxStamina);
	}

}
