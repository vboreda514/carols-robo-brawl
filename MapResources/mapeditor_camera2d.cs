using Godot;
using System;

public partial class mapeditor_camera2d : Camera2D
{
	[Export] public float Speed = 450f; // pixels per second movement speed
	[Export] public float ZoomSpeed = 0.1f; // zoom speed

	public override void _Process(double delta)
	{
		Vector2 movement = Vector2.Zero;

		// get movement input
		if (Input.IsActionPressed("ui_left"))
			movement.X -= 1;
		if (Input.IsActionPressed("ui_right"))
			movement.X += 1;
		if (Input.IsActionPressed("ui_up"))
			movement.Y -= 1;
		if (Input.IsActionPressed("ui_down"))
			movement.Y += 1;

		// normalize and move camera position
		if (movement != Vector2.Zero)
			movement = movement.Normalized();
			float adjustedSpeed = Speed / Zoom.X; // Increase speed as zoom increases
			Position += movement * adjustedSpeed * (float)delta;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// mouse wheel zoom
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
				Zoom += new Vector2(ZoomSpeed, ZoomSpeed);
			else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
				Zoom -= new Vector2(ZoomSpeed, ZoomSpeed);

			// keep zoom within a range
			Zoom = new Vector2(
				Mathf.Clamp(Zoom.X, 0.7f, 2.0f),
				Mathf.Clamp(Zoom.Y, 0.7f, 2.0f)
			);
		}
	}
}
