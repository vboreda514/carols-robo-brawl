using Godot;
using System;

public partial class Game2d_Control : Control
{
	private CanvasLayer _pausemenu;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_pausemenu = GetTree().Root.GetNode<CanvasLayer>("Node2D/PauseMenuCanvasLayer");
		_pausemenu.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnMenuButtonPressed(){
		_pausemenu.Visible = true;
	}
}
