using Godot;
using System;

public partial class MapEditorCamera2D : Camera2D
{
	[Export] public float Speed = 700f; // pixels per second movement speed
	[Export] public float ZoomSpeed = 0.1f; // zoom speed

	public override void _Process(double delta)
	{
		Vector2 movement = Vector2.Zero;

		// Get movement input
		if (Input.IsActionPressed("ui_left"))
			movement.X -= 1;
		if (Input.IsActionPressed("ui_right"))
			movement.X += 1;
		if (Input.IsActionPressed("ui_up"))
			movement.Y -= 1;
		if (Input.IsActionPressed("ui_down"))
			movement.Y += 1;

		// Apply movement
		if (movement != Vector2.Zero)
		{
			movement = movement.Normalized();
			float adjustedSpeed = Speed / Zoom.X; 
			Position += movement * adjustedSpeed * (float)delta;
		}


		// clamp position manually based on viewport
		Rect2 viewportRect = GetViewport().GetVisibleRect();
		Vector2 viewportSize = viewportRect.Size;

		float xMin = LimitLeft + viewportSize.X / (2 * Zoom.X);
		float xMax = LimitRight - viewportSize.X / (2 * Zoom.X);
		float yMin = LimitTop + viewportSize.Y / (2 * Zoom.Y);
	 	float yMax = LimitBottom - viewportSize.Y / (2 * Zoom.Y);

		Position = new Vector2(
			Mathf.Clamp(Position.X, xMin, xMax),
			Mathf.Clamp(Position.Y, yMin, yMax)
		);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
				Zoom += new Vector2(ZoomSpeed, ZoomSpeed);
			else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
				Zoom -= new Vector2(ZoomSpeed, ZoomSpeed);

			Zoom = new Vector2(
				Mathf.Clamp(Zoom.X, 0.7f, 2.0f),
				Mathf.Clamp(Zoom.Y, 0.7f, 2.0f)
			);
		}
	}
}
