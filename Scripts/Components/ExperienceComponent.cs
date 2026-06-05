using Godot;
using System;

namespace NeonSwarm.Components;

public partial class ExperienceComponent : Node
{
	[Export] public int StartingLevel { get; set; } = 1;
	[Export] public float StartingExperience { get; set; } = 0f;

	[Export] public float BaseExperienceToLevel { get; set; } = 5f;
	[Export] public float LevelExperienceMultiplier { get; set; } = 1.25f;

	public int CurrentLevel { get; private set; }
	public float CurrentExperience { get; private set; }
	public float ExperienceToNextLevel { get; private set; }

	public event Action<float, float, int> ExperienceChanged; // Fires when the player gains xp or levels up. Args: current xp, xp needed for next level, current level.
	public event Action<int> LeveledUp; // Fires once for each level gained. Args: new current level

	// Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ResetExperience();
    }

	public void AddExperience(float amount)
	{
		if (amount <= 0f)
			return;
		
		CurrentExperience += amount;

		while (CurrentExperience >= ExperienceToNextLevel)
		{
			CurrentExperience -= ExperienceToNextLevel;
			CurrentLevel++;

			ExperienceToNextLevel = CalculateExperienceToNextLevel(CurrentLevel);

			LeveledUp?.Invoke(CurrentLevel);
			GD.Print($"Level up! Level {CurrentLevel}");
		}

		ExperienceChanged?.Invoke(CurrentExperience, ExperienceToNextLevel, CurrentLevel);
	}

	public void ResetExperience()
	{
		CurrentLevel = Mathf.Max(1, StartingLevel);
		CurrentExperience = Mathf.Max(0f, StartingExperience);
		ExperienceToNextLevel = CalculateExperienceToNextLevel(CurrentLevel);

		ExperienceChanged?.Invoke(CurrentExperience, ExperienceToNextLevel, CurrentLevel);
	}

	private float CalculateExperienceToNextLevel(int level)
	{
		float levelOffset = Mathf.Max(0, level - 1); // How many levels above the starting level the player is at.
		float requiredExperience = BaseExperienceToLevel * Mathf.Pow(LevelExperienceMultiplier, levelOffset);

		return Mathf.Max(1f, requiredExperience);
	}
}
