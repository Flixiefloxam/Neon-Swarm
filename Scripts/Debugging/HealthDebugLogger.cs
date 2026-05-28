using Godot;
using NeonSwarm.Components;

namespace NeonSwarm.Debugging;

public partial class HealthDebugLogger : Node
{
	[Export] public NodePath HealthComponentPath { get; set; } = "../HealthComponent";
	[Export] public string EntityName { get; set; } = ""; // Override the entity name used in logs. If empty, will use the name of specified node (EntityNameNodePath).
    [Export] public NodePath EntityNameNodePath { get; set; } = ".."; // If Entity Name is empty will use the name of this node.
	[Export] public bool Enabled { get; set; } = true;

	private HealthComponent _health;

	public override void _Ready()
	{
		_health = GetNodeOrNull<HealthComponent>(HealthComponentPath);
		if (_health == null)
		{
			GD.PushWarning($"{Name} could not find HealthComponent.");
			return;
		}

        if (string.IsNullOrEmpty(EntityName))
        {
            Node entityNameNode = GetNodeOrNull<Node>(EntityNameNodePath);
            if (entityNameNode != null)
            {
                EntityName = entityNameNode.Name;
            }
            else
            {
                GD.PushWarning($"{Name} could not find node for EntityName. Using default name.");
                EntityName = "Entity";
            }
        }

		_health.Damaged += OnDamaged;
		_health.HealthChanged += OnHealthChanged;
		_health.Died += OnDied;
	}

	public override void _ExitTree()
	{
		if (_health == null)
			return;

		_health.Damaged -= OnDamaged;
		_health.HealthChanged -= OnHealthChanged;
		_health.Died -= OnDied;
	}

	private void OnDamaged(float damage)
	{
		if (!Enabled)
			return;

		GD.Print($"{EntityName} took {damage} damage.");
	}

	private void OnHealthChanged(float currentHealth, float maxHealth)
	{
		if (!Enabled)
			return;

		GD.Print($"{EntityName} health: {currentHealth}/{maxHealth}");
	}

	private void OnDied()
	{
		if (!Enabled)
			return;

		GD.Print($"{EntityName} died.");
	}
}