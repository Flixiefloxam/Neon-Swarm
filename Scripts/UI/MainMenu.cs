using Godot;

namespace NeonSwarm.UI;

public partial class MainMenu : Control
{
    [Export] public string GameScenePath { get; set; } = "res://Scenes/Main/Main.tscn";

    private Button _startButton;
    private Button _quitButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _startButton = GetNode<Button>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/StartButton"
		);

		_quitButton = GetNode<Button>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/QuitButton"
		);

		_startButton.Pressed += OnStartPressed;
		_quitButton.Pressed += OnQuitPressed;

		_startButton.GrabFocus();
    }

    private void OnStartPressed()
    {
        if (string.IsNullOrWhiteSpace(GameScenePath))
        {
            GD.PushError($"{Name} has no GameScenePath assigned.");
            return;
        }

        GetTree().ChangeSceneToFile(GameScenePath);
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    public override void _ExitTree()
	{
		if (_startButton != null)
			_startButton.Pressed -= OnStartPressed;

		if (_quitButton != null)
			_quitButton.Pressed -= OnQuitPressed;
	}
}
