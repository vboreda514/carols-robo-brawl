using Godot;
using System;
using System.Collections.Generic;

public partial class MapEditorUIControl : Control
{	
	private FileDialog _saveDialog;
	private FileDialog _loadDialog;
	private TileMapLayer _tileMapLayerFloor;
	private TileMapLayer _tileMapLayerExtras;
	private Label _mapEditorLabel;
	
	private string _currentFilePath = string.Empty;
	
	// texture button highlighting shaders
	private List<TextureButton> _textureButtons = new List<TextureButton>();
	private TextureButton selectedButton = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		_tileMapLayerFloor = GetNode<TileMapLayer>("../../TileMapLayer");
		_tileMapLayerExtras = GetNode<TileMapLayer>("../../TileMapLayer2");
		_mapEditorLabel = GetNode<Label>("VBoxContainer/PanelContainer/MarginContainer/HBoxContainer/Label");
		
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
		
		// texturebutton shaders
		foreach (Node child in GetNode<HBoxContainer>("VBoxContainer/MarginContainer/PanelContainer/MarginContainer/HBoxContainer").GetChildren())
		{
			if (child is TextureButton button)
			{
				_textureButtons.Add(button);

				// Ensure each button uses the shader
				if (button.Material == null)
				{
					ShaderMaterial shaderMaterial = new ShaderMaterial();
					shaderMaterial.Shader = GD.Load<Shader>("res://MapResources/texturebutton_shader.gdshader"); // Update the path
					button.Material = shaderMaterial;
				}
			}
		}
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
		_mapEditorLabel.Text = FormatFilename(path);
		SaveTileMap(path);
	}

	private void SaveTileMap(string path)
	{
		try
		{
			// called PackedByteArray in docs
			byte[] tilemapDataFloor = _tileMapLayerFloor.TileMapData;
			byte[] tilemapDataExtras = _tileMapLayerExtras.TileMapData;
		
			// use fileaccess to write each layer's byte array length and info to the file
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
			if (file == null)
			{
				GD.PushError($"Failed to open file for writing: {path}");
				return;
			}
			
			file.Store32((uint)tilemapDataFloor.Length);
			file.StoreBuffer(tilemapDataFloor);
			file.Store32((uint)tilemapDataExtras.Length);
			file.StoreBuffer(tilemapDataExtras);
			
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
				GD.PushError($"Failed to open file for reading: {path}");
				return;
			}
			
			_mapEditorLabel.Text = FormatFilename(path);
			
			// read file into byte[] instances (PackedByteArray in docs) and load into tilemaplayers
			uint floorLength = file.Get32();
			byte[] tilemapDataFloor = file.GetBuffer((int)floorLength);
			uint extrasLength = file.Get32();
			byte[] tilemapDataExtras = file.GetBuffer((int)extrasLength);
			
			_tileMapLayerFloor.TileMapData = tilemapDataFloor;
			_tileMapLayerExtras.TileMapData = tilemapDataExtras;
			
			GD.Print($"TileMap loaded successfully from: {path}");
		}
		catch (Exception e)
		{
			GD.PushError($"Exception while loading TileMap: {e.Message}");
		}
	}
	
	private string FormatFilename(string path) { 
		int i = path.LastIndexOf('/') + 1;
		return "Currently Editing: " + path.Substring(i);
	}
	
	public void OnTextureButtonHovered(String buttonName)
	{
		if (buttonName != null)
		{
			var button = GetNode($"VBoxContainer/MarginContainer/PanelContainer/MarginContainer/HBoxContainer/{buttonName}") as TextureButton;
			
			var shaderMaterial = button.Material as ShaderMaterial;
			if (shaderMaterial != null)
			{
				shaderMaterial.SetShaderParameter("is_hovered", true);
			}
		}
	}

	public void OnTextureButtonExited(String buttonName)
	{
		if (buttonName != null)
		{
			var button = GetNode($"VBoxContainer/MarginContainer/PanelContainer/MarginContainer/HBoxContainer/{buttonName}") as TextureButton;
			
			var shaderMaterial = button.Material as ShaderMaterial;
			if (shaderMaterial != null)
			{
				shaderMaterial.SetShaderParameter("is_hovered", false);
			}
		}
	}

	public void OnTextureButtonPressed(String buttonName)
	{	
		
		
		if (buttonName != null)
		{
			var button = GetNode($"VBoxContainer/MarginContainer/PanelContainer/MarginContainer/HBoxContainer/{buttonName}");
			
			// Update all buttons
			foreach (var btn in _textureButtons)
			{
				var shaderMaterial = btn.Material as ShaderMaterial;
				if (shaderMaterial != null)
				{
					// Set 'is_selected' true only for the currently pressed button
					shaderMaterial.SetShaderParameter("is_selected", btn == button);
				}
			}
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
