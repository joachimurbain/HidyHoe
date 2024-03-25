using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Globals : Node
{


	public enum PlayerAnimation
	{
		Crouch,
		Walk,
	}

	public enum RoundOutcome
	{
		SeekerVictory,
		HiderVictory,
		Draw
	}
}
