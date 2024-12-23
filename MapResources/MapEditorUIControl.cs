using Godot;
using System;

public partial class MapEditorUIControl : Control
{	
	private Button _saveButton;
	private FileDialog _saveDialog;
	private FileDialog _loadDialog;
	private TileMapLayer _tileMapLayer;
	
	private string _currentFilePath = string.Empty;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		_saveButton = GetNode<Button>("VBoxContainer/PanelContainer/CenterContainer/HBoxContainer/Button");
		_tileMapLayer = GetNode<TileMapLayer>("../../TileMapLayer");
		
		// save map file dialog window
		_saveDialog = new FileDialog();
		AddChild(_saveDialog);
		
		_saveDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
		_saveDialog.Access = FileDialog.AccessEnum.Userdata;
		_saveDialog.Filters = new string[] { "*.map;Map Files" };
		_saveDialog.CurrentDir = ProjectSettings.GlobalizePath(GlobalVariables.MAPEDITOR_SAVE_DIRECTORY);
		
		// create the save directory if it doesn't exist
		DirAccess.MakeDirRecursiveAbsolute(GlobalVariables.MAPEDITOR_SAVE_DIRECTORY);
		
		// connect dialog box signal
		_saveDialog.FileSelected += OnFileSelectedSaveDialog;
		
		_loadDialog = new FileDialog();
		AddChild(_loadDialog);
		
		_loadDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		_loadDialog.Access = FileDialog.AccessEnum.Userdata;
		_loadDialog.Filters = new string[] { "*.map;Map Files" };
		_loadDialog.CurrentDir = ProjectSettings.GlobalizePath(GlobalVariables.MAPEDITOR_SAVE_DIRECTORY);
		
		// connect dialog box signal
		_loadDialog.FileSelected += OnFileSelectedLoadDialog;
	}

	
	private void OnSaveButtonPressed()
	{	
		// if no file is already loaded then bring up file dialog box
		if (string.IsNullOrEmpty(_currentFilePath))
		{
			
			_saveDialog.PopupCentered();
		}
		// if a file is loaded then overwrite that file
		else
		{
			
			SaveTileMap(_currentFilePath);
		}
	}

	private void OnFileSelectedSaveDialog(string path)
	{
		_currentFilePath = path;
		SaveTileMap(path);
	}

	private void SaveTileMap(string path)
	{
		try
		{
			// called PackedByteArray in docs
			byte[] tilemapData = _tileMapLayer.TileMapData;
		
			// use fileaccess to write the byte array to a file
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
			if (file == null)
			{
				GD.PrintErr($"Failed to open file for writing: {path}");
				return;
			}

			file.StoreBuffer(tilemapData);
			GD.Print($"TileMap saved successfully to: {path}");
		}
		catch (Exception e)
		{
			GD.PushError($"Exception while saving TileMapLayer: {e.Message}");
		}
	}
	
	private void OnLoadButtonPressed()
	{
		_loadDialog.PopupCentered();
	}
	
	private void OnFileSelectedLoadDialog(string path)
	{
		try
		{
			_currentFilePath = path;
			
			// open map file
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			if (file == null)
			{
				GD.PrintErr($"Failed to open file for reading: {path}");
				return;
			}

			// read into byte[] (PackedByteArray in docs) and load into tilemaplayer
			byte[] tilemapData = file.GetBuffer((long)file.GetLength());
			_tileMapLayer.TileMapData = tilemapData;
			
			GD.Print($"TileMap loaded successfully from: {path}");
		}
		catch (Exception e)
		{
			GD.PushError($"Exception while loading TileMap: {e.Message}");
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
