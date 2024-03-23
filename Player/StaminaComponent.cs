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
	[Export]
	public float DepletedStaminaDebuffDuration = 5f;

	private bool depletedStamina = false;
	private Player playerNode
	{
		get => GetParent<Player>();
	}

	public override void _Ready()
	{
		playerNode.CurrentStamina = MaxStamina;
	}

	public override void _Process(double delta)
	{
		if (playerNode.IsRunning)
		{
			DeductStamina(StaminaDrainRate * delta);
		}
		else if (!depletedStamina)
		{
			RegenStamina(StaminaRegenerationRate * delta);
		}
	}

	private void DeductStamina(double amount)
	{
		playerNode.CurrentStamina = Mathf.Clamp(playerNode.CurrentStamina - amount, 0, MaxStamina);
		if(playerNode.CurrentStamina < 1)
		{
			depletedStamina = true;
			Timer depletedStaminaTimer = GetNode<Timer>("DepletedStaminaTimer");
			if (!depletedStaminaTimer.IsStopped())
			{
				depletedStaminaTimer.Stop();
			}
			depletedStaminaTimer.WaitTime = DepletedStaminaDebuffDuration;
			depletedStaminaTimer.Start();
		}
	}

	private void RegenStamina(double amount)
	{
		playerNode.CurrentStamina = Mathf.Clamp(playerNode.CurrentStamina + amount, 0, MaxStamina);
	}

	public void OnDepletedStaminaTimerTimeout()
	{
		depletedStamina = false;
	}

}
