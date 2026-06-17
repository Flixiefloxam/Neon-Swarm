using Godot;
using System;
using NeonSwarm.Upgrades;

namespace NeonSwarm.UI;

public partial class UpgradeChoiceCard : Button
{
	private Label _nameLabel;
	private Label _rarityLabel;
	private Label _descriptionLabel;

	public UpgradeDefinition Upgrade { get; private set; }

	public event Action<UpgradeDefinition> UpgradeSelected; // Fired when this card is pressed and has a valid upgrade assigned.

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_nameLabel = GetNode<Label>("MarginContainer/VBoxContainer/HeaderRow/NameLabel");
		_rarityLabel = GetNode<Label>("MarginContainer/VBoxContainer/HeaderRow/RarityLabel");
		_descriptionLabel = GetNode<Label>("MarginContainer/VBoxContainer/DescriptionLabel");

		Text = "";

		Pressed += OnPressed;
	}

	// Set the upgrade displayed by the upgrade card and show the card
	public void SetUpgrade(UpgradeDefinition upgrade, Color rarityColor)
	{
		Upgrade = upgrade;

		if (upgrade == null)
		{
			Hide();
			return;
		}

		Show();

		_nameLabel.Text = upgrade.DisplayName;
		_rarityLabel.Text = upgrade.Rarity.ToString();
		_descriptionLabel.Text = upgrade.Description;

		_nameLabel.AddThemeColorOverride("font_color", rarityColor);
		_rarityLabel.AddThemeColorOverride("font_color", rarityColor);
	}

	private void OnPressed()
	{
		if (Upgrade == null)
			return;

		UpgradeSelected?.Invoke(Upgrade);
	}

	public override void _ExitTree()
	{
		Pressed -= OnPressed;
	}
}
