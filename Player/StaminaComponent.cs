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



	private Player localPlayer;

	public override void _Ready()
	{
		localPlayer = GetParent<Player>();
		localPlayer.CurrentStamina = MaxStamina;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
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
		
		if (localPlayer.CurrentStamina <= 0f)
		{
			GD.Print("No More Stamina");
			// Stamina is depleted, handle accordingly (e.g., disable sprinting)
		}

	}

	private void RegenStamina(double amount)
	{
		localPlayer.CurrentStamina = Mathf.Clamp(localPlayer.CurrentStamina + amount, 0, MaxStamina);
	}

}
