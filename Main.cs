using Godot;
using System;

public partial class Main : Node2D
{
	public static Vector2 ScreenSize ;
	public override void _Ready()
	{
		SetPlaygroundSize();

	}

	private void SetPlaygroundSize()
	{
		ScreenSize = GetViewportRect().Size;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public override void _Process(double delta)
	{
	}

}
