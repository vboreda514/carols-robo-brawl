using Godot;
using System;

public partial class Pausemenu_Control : Control
{
	private CanvasLayer _pausemenu;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_pausemenu = GetParent<CanvasLayer>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnResumeButtonPressed(){
		_pausemenu.Visible = false;
	}
	
	private void OnExitButtonPressed(){
		GetTree().ChangeSceneToFile("res://Scenes/main_menu_control.tscn");
	}
}
