using Godot;
using System;
using NeonSwarm.Components;
using NeonSwarm.Upgrades;

namespace NeonSwarm.UI;

public partial class Hud : CanvasLayer
{
    [Export] public NodePath ExperienceComponentPath { get; set; } = "../Player/ExperienceComponent";

    private ProgressBar _xpBar;
    private Label _timerLabel;
    private GameOverScreen _gameOverScreen;
    private LevelUpScreen _levelUpScreen;

    private ExperienceComponent _experienceComponent;

    private double _elapsedTime = 0.0;
    private bool _timerRunning = true;
    public event Action<UpgradeDefinition> UpgradeSelected;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _xpBar = GetNode<ProgressBar>("Root/TopHud/XPBar");
        _timerLabel = GetNode<Label>("Root/TopHud/TimerLabel");
        _gameOverScreen = GetNode<GameOverScreen>("Root/GameOverScreen");
        _levelUpScreen = GetNode<LevelUpScreen>("Root/LevelUpScreen");

        _levelUpScreen.UpgradeSelected += OnUpgradeSelected;

        _experienceComponent = GetNodeOrNull<ExperienceComponent>(ExperienceComponentPath);

        if (_experienceComponent != null)
        {
            _experienceComponent.ExperienceChanged += OnExperienceChanged;

            OnExperienceChanged(
                _experienceComponent.CurrentExperience,
                _experienceComponent.ExperienceToNextLevel,
                _experienceComponent.CurrentLevel
            );
        }
        else
        {
            SetXp(0, 100);
        }
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

    public void ShowLevelUpChoices(Godot.Collections.Array<UpgradeDefinition> choices)
	{
		_levelUpScreen.ShowChoices(choices);
	}

    public void HideLevelUpScreen()
	{
		_levelUpScreen.Hide();
	}

    public void SetXp(float currentXp, float xpNeeded)
    {
        _xpBar.MaxValue = xpNeeded;
        _xpBar.Value = currentXp;
    }

    public void PauseGameTimer()
    {
        _timerRunning = false;
    }

    public void ResumeGameTimer()
	{
		_timerRunning = true;
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

    private void OnExperienceChanged(float currentExperience, float experienceToNextLevel, int currentLevel)
    {
        SetXp(currentExperience, experienceToNextLevel);
    }

    private void OnUpgradeSelected(UpgradeDefinition upgrade)
	{
		UpgradeSelected?.Invoke(upgrade);
	}

    public override void _ExitTree()
    {
        if (_experienceComponent != null)
            _experienceComponent.ExperienceChanged -= OnExperienceChanged;
        
        if (_levelUpScreen != null)
			_levelUpScreen.UpgradeSelected -= OnUpgradeSelected;
    }
}
