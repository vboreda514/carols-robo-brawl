using Godot;
using System;

public partial class GlobalVariables : Node
{
	public static GlobalVariables Instance { get; private set; }
	
	// define variables here
	public int maxPlayers { get; set; } = 6;
	public const string MAPEDITOR_SAVE_DIRECTORY = "user://custom_maps/";

	public override void _Ready()
	{
		Instance = this;
	}
}
