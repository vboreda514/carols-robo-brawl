using Godot;
using System;

public partial class PowerupVBoxContainer : VBoxContainer
{	
	private Button _toggleButton;
	private Control _cardLayout;
	private bool _isOpen = true;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_toggleButton = GetNode<Button>("Button");
		_cardLayout = GetNode<Control>("GCardHandLayoutPowerups");

		UpdateButtonIcon();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnTogglePressed(bool buttonPressed)
	{
		_isOpen = !_isOpen;
		_cardLayout.Visible = _isOpen;
		UpdateButtonIcon();
	}

	private void UpdateButtonIcon()
	{
		_toggleButton.Text = _isOpen ? "▼" : "▲";
	}
}
