using System;
using System.Collections.Generic;
using Godot;
using NeonSwarm.Weapons;

namespace NeonSwarm.Upgrades;

public partial class UpgradeManager : Node
{
	[Export] public UpgradeDatabase Database { get; set; }

	[ExportGroup("Selection")]
	[Export] public int ChoicesPerLevel { get; set; } = 3;

	[ExportGroup("Rarity Weights")] // How rare each rarity is. All the weights should add up to 100 for clarity.
	[Export] public float CommonWeight { get; set; } = 70f;
	[Export] public float UncommonWeight { get; set; } = 25f;
	[Export] public float RareWeight { get; set; } = 5f;

	private readonly RandomNumberGenerator _random = new();

	private readonly Dictionary<string, int> _upgradeStacks =
		new(StringComparer.OrdinalIgnoreCase);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_random.Randomize();
	}

	public Godot.Collections.Array<UpgradeDefinition> GenerateUpgradeChoices()
	{
		Godot.Collections.Array<UpgradeDefinition> choices = new();

		if (Database == null || Database.Upgrades == null || Database.Upgrades.Count == 0)
		{
			GD.PushWarning($"{Name} has no upgrade database or no upgrades.");
			return choices;
		}

		int attempts = 0;
		int maxAttempts = ChoicesPerLevel * 20;

		while (choices.Count < ChoicesPerLevel && attempts < maxAttempts)
		{
			attempts++;

			UpgradeRarity rarity = RollRarity();

			List<UpgradeDefinition> validUpgrades = GetValidUpgrades(rarity, choices);

			// If the rolled rarity has no valid upgrades, fall back to any valid rarity.
			if (validUpgrades.Count == 0)
				validUpgrades = GetValidUpgrades(null, choices);
			
			if (validUpgrades.Count == 0)
				break;
			
			int selectedIndex = _random.RandiRange(0, validUpgrades.Count - 1);
			choices.Add(validUpgrades[selectedIndex]);
		}

		return choices;
	}

	public bool ApplyUpgrade(UpgradeDefinition upgrade)
	{
		if (upgrade == null)
			return false;
		
		if (!upgrade.CanBeSelected(this))
		{
			GD.PushWarning($"Upgrade '{upgrade.DisplayName}' cannot currently be applied.");
			return false;
		}

		upgrade.Apply(this);
		AddUpgradeStack(upgrade.UpgradeId);

		GD.Print($"Applied upgrade: {upgrade.DisplayName}");

		return true;
	}

	// returns how many stack of the specified are applied.
	public int GetUpgradeStackCount(string upgradeId)
	{
		if (string.IsNullOrWhiteSpace(upgradeId))
			return 0;
		
		// if you can get it, return stack count, otherwise return 0.
		return _upgradeStacks.TryGetValue(upgradeId, out int stackCount)
			? stackCount
			: 0;
	}

	// Returns the weapon with the specified weaponId.
	public BaseWeapon FindWeapon(string weaponId)
	{
		if (string.IsNullOrWhiteSpace(weaponId))
			return null;
		
		foreach (Node node in GetTree().GetNodesInGroup("Weapons"))
		{
			if (node is not BaseWeapon weapon)
				continue;
			
			if (weapon.HasWeaponId(weaponId))
				return weapon;
		}

		return null;
	}

	// Returns all valid upgrades of the specified rarity that aren't in the provided array (alreadySelected).
	private List<UpgradeDefinition> GetValidUpgrades(
		UpgradeRarity? rarity,
		Godot.Collections.Array<UpgradeDefinition> alreadySelected
	)
	{
		List<UpgradeDefinition> validUpgrades = new();

		foreach (UpgradeDefinition upgrade in Database.Upgrades)
		{
			if (upgrade == null)
				continue;
			
			if (rarity.HasValue && upgrade.Rarity != rarity.Value)
				continue;
			
			if (alreadySelected.Contains(upgrade))
				continue;
			
			if (!upgrade.CanBeSelected(this))
				continue;
			
			validUpgrades.Add(upgrade);
		}

		return validUpgrades;
	}

	// Retuns a weighted random rarity.
	private UpgradeRarity RollRarity()
	{
		float commonWeight = Mathf.Max(0f, CommonWeight);
		float uncommonWeight = Mathf.Max(0f, UncommonWeight);
		float rareWeight = Mathf.Max(0f, RareWeight);

		float totalWeight = commonWeight + uncommonWeight + rareWeight;

		if (totalWeight <= 0f)
			return UpgradeRarity.Common;
		
		float roll = _random.RandfRange(0f, totalWeight);

		if (roll < commonWeight)
			return UpgradeRarity.Common;
		
		roll -= commonWeight;

		if (roll < uncommonWeight)
			return UpgradeRarity.Uncommon;
		
		return UpgradeRarity.Rare;
	}

	// Adds a stack of the specified upgrade
	private void AddUpgradeStack(string upgradeId)
	{
		if (string.IsNullOrWhiteSpace(upgradeId))
			return;
		
		int currentStacks = GetUpgradeStackCount(upgradeId);
		_upgradeStacks[upgradeId] = currentStacks + 1;
	}
}
