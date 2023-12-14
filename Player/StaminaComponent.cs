using Godot;
using System;

public partial class StaminaComponent : Node
{
	[Export]
	public double MaxStamina = 100;
	[Export]
	public double StaminaDrainRate = 10;
	[Export]
	public double StaminaRegenerationRate = 0.2;
	[Export]
	public double CurrentStamina;


	private Player localPlayer;

	public override void _Ready()
	{
		localPlayer = GetParent<Player>();
		CurrentStamina = MaxStamina;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		if (localPlayer.IsRunning)
		{
			DeductStamina(StaminaDrainRate * delta);
		}
	}

		private void DeductStamina(double amount)
	{
		CurrentStamina = Mathf.Clamp(CurrentStamina - amount, 0, MaxStamina);
		if (CurrentStamina <= 0f)
		{
			GD.Print("No More Stamina");
			// Stamina is depleted, handle accordingly (e.g., disable sprinting)
		}
	}

}
