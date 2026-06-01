using Godot;

namespace NeonSwarm.UI;

public partial class Hud : CanvasLayer
{
    private ProgressBar _xpBar;
    private Label _timerLabel;
    private GameOverScreen _gameOverScreen;

    private double _elapsedTime = 0.0;
    private bool _timerRunning = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _xpBar = GetNode<ProgressBar>("Root/TopHud/XPBar");
        _timerLabel = GetNode<Label>("Root/TopHud/TimerLabel");
        _gameOverScreen = GetNode<GameOverScreen>("Root/GameOverScreen");

        SetXp(0, 100);
        UpdateTimerLabel();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {   
        if (!_timerRunning)
            return;

        _elapsedTime += delta;
        UpdateTimerLabel();
    }

    public void SetXp(float currentXp, float xpNeeded)
    {
        _xpBar.MaxValue = xpNeeded;
        _xpBar.Value = currentXp;
    }

    public void StopGameTimer()
    {
        _timerRunning = false;
    }

    public void ShowGameOverScreen()
    {
        _gameOverScreen.ShowGameOver(_elapsedTime);
    }

    private void UpdateTimerLabel()
    {
        int totalSeconds = (int)_elapsedTime;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        _timerLabel.Text = $"{minutes:00}:{seconds:00}";
    }
}
