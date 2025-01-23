using Godot;
using System;
using System.Collections.Generic;

public partial class Game2d : Node2D
{
	private PackedScene PlayerScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");

	private List<Vector2> SpawnPositions = new List<Vector2>
	{
		new Vector2(0, 0),
		new Vector2(1, 0)
	};
	
	private List<Dictionary<string, object>> MoveSequence = new List<Dictionary<string, object>>
	{
		new Dictionary<string, object> { { "player_id", 1 }, { "action", "rotate_right" } },
		new Dictionary<string, object> { { "player_id", 2 }, { "action", "rotate_left" } },
		new Dictionary<string, object> { { "player_id", 2 }, { "action", "move_1" } },
		new Dictionary<string, object> { { "player_id", 1 }, { "action", "u_turn" } }
	};

	private List<Node2D> _playerInstances = new List<Node2D>();
	private Node2D _currentPlayer;

	private bool _executing = false;
	private bool _rotateLeft = false;
	private bool _rotateRight = false;
	private bool _moveForward = false;
	private float _goalRotation = 0;
	private Vector2 _goalPosition;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach (var position in SpawnPositions)
		{
			var playerInstance = (Node2D)PlayerScene.Instantiate();
			playerInstance.Position = position * 64 + new Vector2(32, 32);

			GetParent().AddChild(playerInstance);
			_playerInstances.Add(playerInstance);

			GD.Print("Player spawned at: ", position);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("execute") && !_executing)
		{
			_executing = true;
		}

		if (_executing && !_rotateRight && !_rotateLeft && !_moveForward)
		{
			if (MoveSequence.Count <= 0)
			{
				_executing = false;
			}
			else
			{
				var move = MoveSequence[0];
				MoveSequence.RemoveAt(0);

				_currentPlayer = _playerInstances[(int)move["player_id"] - 1];
				string action = (string)move["action"];

				if (action == "rotate_right")
				{
					_rotateRight = true;
					_goalRotation = _currentPlayer.Rotation + Mathf.Pi / 2;
				}
				else if (action == "rotate_left")
				{
					_rotateLeft = true;
					_goalRotation = _currentPlayer.Rotation - Mathf.Pi / 2;
				}
				else if (action == "u_turn")
				{
					_rotateRight = true;
					_goalRotation = _currentPlayer.Rotation + Mathf.Pi;
				}
				else if (action == "move_1")
				{
					_moveForward = true;

					if (Mathf.IsEqualApprox(_currentPlayer.Rotation, 0))
					{
						_goalPosition = _currentPlayer.Position + new Vector2(0, 64);
					}
					else if (Mathf.IsEqualApprox(_currentPlayer.Rotation, Mathf.Pi / 2))
					{
						_goalPosition = _currentPlayer.Position + new Vector2(-64, 0);
					}
					else if (Mathf.IsEqualApprox(_currentPlayer.Rotation, Mathf.Pi))
					{
						_goalPosition = _currentPlayer.Position + new Vector2(0, -64);
					}
					else if (Mathf.IsEqualApprox(_currentPlayer.Rotation, 3 * Mathf.Pi / 2))
					{
						_goalPosition = _currentPlayer.Position + new Vector2(64, 0);
					}
				}
			}
		}

		if (_rotateRight)
		{
			if (_currentPlayer.Rotation < _goalRotation)
			{
				_currentPlayer.Rotation += Mathf.Pi / 30;
			}
			else
			{
				if (_goalRotation > 2 * Mathf.Pi)
					_goalRotation -= 2 * Mathf.Pi;
				_currentPlayer.Rotation = _goalRotation;
				_rotateRight = false;
			}
		}
		else if (_rotateLeft)
		{
			if (_currentPlayer.Rotation > _goalRotation)
			{
				_currentPlayer.Rotation -= Mathf.Pi / 30;
			}
			else
			{
				if (_goalRotation < 0)
					_goalRotation += 2 * Mathf.Pi;
				_currentPlayer.Rotation = _goalRotation;
				_rotateLeft = false;
			}
		}
		else if (_moveForward)
		{
			if (_currentPlayer.Position.X < _goalPosition.X)
			{
				_currentPlayer.Position += new Vector2(1, 0);
			}
			else if (_currentPlayer.Position.X > _goalPosition.X)
			{
				_currentPlayer.Position -= new Vector2(1, 0);
			}
			else if (_currentPlayer.Position.Y < _goalPosition.Y)
			{
				_currentPlayer.Position += new Vector2(0, 1);
			}
			else if (_currentPlayer.Position.Y > _goalPosition.Y)
			{
				_currentPlayer.Position -= new Vector2(0, 1);
			}
			else
			{
				_moveForward = false;
			}
		}
	}
}
