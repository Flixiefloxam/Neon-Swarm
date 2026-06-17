using Godot;
using System;
using NeonSwarm.Upgrades;

namespace NeonSwarm.UI;

public partial class LevelUpScreen : Control
{
	[ExportGroup("Rarity Colors")]
	[Export] public Color CommonColor { get; set; } = new(0.9f, 1f, 1f, 1f);
	[Export] public Color UncommonColor { get; set; } = new(0.25f, 1f, 0.45f, 1f);
	[Export] public Color RareColor { get; set; } = new(1f, 0.65f, 0.05f, 1f);

	public event Action<UpgradeDefinition> UpgradeSelected;

	private UpgradeChoiceCard[] _choiceCards;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_choiceCards = new[]
		{
			GetNode<UpgradeChoiceCard>(
				"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/ChoicesContainer/ChoiceCard1"
			),
			GetNode<UpgradeChoiceCard>(
				"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/ChoicesContainer/ChoiceCard2"
			),
			GetNode<UpgradeChoiceCard>(
				"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/ChoicesContainer/ChoiceCard3"
			)
		};

		foreach (UpgradeChoiceCard card in _choiceCards)
		{
			card.UpgradeSelected += OnUpgradeSelected;
			card.Hide();
		}

		Hide();
	}

	public void ShowChoices(Godot.Collections.Array<UpgradeDefinition> upgrades)
	{
		if (upgrades == null || upgrades.Count == 0)
		{
			GD.PushWarning($"{Name} was asked to show upgrade choices, but no upgrades were provided.");
			return;
		}

		Show();

		for (int i = 0; i < _choiceCards.Length; i++)
		{
			if (i >= upgrades.Count) // If there are more choice cards then upgrades to show, hide the excess cards
			{
				_choiceCards[i].Hide();
				continue;
			}

			UpgradeDefinition upgrade = upgrades[i];
			_choiceCards[i].SetUpgrade(upgrade, GetRarityColor(upgrade.Rarity));
		}

		_choiceCards[0].CallDeferred(Control.MethodName.GrabFocus);
	}

	private Color GetRarityColor(UpgradeRarity rarity)
	{
		return rarity switch
		{
			UpgradeRarity.Common => CommonColor,
			UpgradeRarity.Uncommon => UncommonColor,
			UpgradeRarity.Rare => RareColor,
			_ => CommonColor
		};
	}

	private void OnUpgradeSelected(UpgradeDefinition upgrade)
	{
		if (upgrade == null)
			return;

		UpgradeSelected?.Invoke(upgrade);
		Hide();
	}

	public override void _ExitTree()
	{
		if (_choiceCards == null)
			return;

		foreach (UpgradeChoiceCard card in _choiceCards)
		{
			if (card != null)
				card.UpgradeSelected -= OnUpgradeSelected;
		}
	}
}
