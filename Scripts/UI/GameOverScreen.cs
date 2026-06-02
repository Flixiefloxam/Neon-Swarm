using Godot;

namespace NeonSwarm.UI;

public partial class GameOverScreen : Control
{
    [Export] public string MainMenuScenePath { get; set; } = "res://Scenes/UI/MainMenu.tscn";

    private Label _timeSurvivedLabel;
    private Button _restartButton;
    private Button _mainMenuButton;
    private UiController _uiController;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _timeSurvivedLabel = GetNode<Label>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/TimeSurvivedLabel"
		);

		_restartButton = GetNode<Button>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/RestartButton"
		);

		_mainMenuButton = GetNode<Button>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MainMenuButton"
		);

        _restartButton.Pressed += OnRestartPressed;
        _mainMenuButton.Pressed += OnMainMenuPressed;
        _uiController = GetNodeOrNull<UiController>("/root/UiController");

        Hide();
    }

    public void ShowGameOver(double elapsedSeconds)
    {
        _timeSurvivedLabel.Text = $"Time Survived: {FormatTime(elapsedSeconds)}";

        Show();
        _uiController?.SetDefaultFocus(_restartButton, true);
    }

    private void OnRestartPressed()
    {
        GetTree().ReloadCurrentScene();
    }

    private void OnMainMenuPressed()
    {
        if (string.IsNullOrWhiteSpace(MainMenuScenePath))
        {
            GD.PushWarning($"{Name} has no MainMenuScenePath assigned");
            return;
        }

        GetTree().ChangeSceneToFile(MainMenuScenePath);
    }

    private static string FormatTime(double elapsedSeconds)
    {
        int totalSeconds = Mathf.Max(0, (int)elapsedSeconds);
        int minuites = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minuites:00}:{seconds:00}";
    }

    public override void _ExitTree()
    {
        if (_restartButton != null)
			_restartButton.Pressed -= OnRestartPressed;

		if (_mainMenuButton != null)
			_mainMenuButton.Pressed -= OnMainMenuPressed;

        _uiController?.ClearDefaultFocus(_restartButton);
    }

}
