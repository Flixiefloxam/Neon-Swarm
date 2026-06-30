using System;
using Godot;

namespace NeonSwarm.UI;

public partial class PauseMenu : Control
{
	[Export] public NodePath ResumeButtonPath { get; set; } = "CenterContainer/PanelContainer/MarginContainer/VBoxContainer/ResumeButton";
	[Export] public NodePath RestartButtonPath { get; set; } = "CenterContainer/PanelContainer/MarginContainer/VBoxContainer/RestartButton";
	[Export] public NodePath MainMenuButtonPath { get; set; } = "CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MainMenuButton";

	public event Action ResumeRequested;
	public event Action RestartRequested;
	public event Action MainMenuRequested;

	private Button _resumeButton;
	private Button _restartButton;
	private Button _mainMenuButton;
	private UiController _uiController;

	public override void _Ready()
	{
		_resumeButton = GetNodeOrNull<Button>(ResumeButtonPath);
		_restartButton = GetNodeOrNull<Button>(RestartButtonPath);
		_mainMenuButton = GetNodeOrNull<Button>(MainMenuButtonPath);
		_uiController = GetNodeOrNull<UiController>("/root/UiController");

		if (_resumeButton != null)
			_resumeButton.Pressed += OnResumePressed;

		if (_restartButton != null)
			_restartButton.Pressed += OnRestartPressed;

		if (_mainMenuButton != null)
			_mainMenuButton.Pressed += OnMainMenuPressed;

		Hide();
	}

	public override void _ExitTree()
	{
		if (_resumeButton != null)
			_resumeButton.Pressed -= OnResumePressed;

		if (_restartButton != null)
			_restartButton.Pressed -= OnRestartPressed;

		if (_mainMenuButton != null)
			_mainMenuButton.Pressed -= OnMainMenuPressed;
	}

	public void ShowMenu()
	{
		Show();

		if (_resumeButton != null)
			_uiController?.SetDefaultFocus(_resumeButton, true);
	}

	public void HideMenu()
	{
		Hide();

		if (_resumeButton != null)
			_uiController?.ClearDefaultFocus(_resumeButton);
	}

	private void OnResumePressed()
	{
		ResumeRequested?.Invoke();
	}

	private void OnRestartPressed()
	{
		RestartRequested?.Invoke();
	}

	private void OnMainMenuPressed()
	{
		MainMenuRequested?.Invoke();
	}
}