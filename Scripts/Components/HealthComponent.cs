using Godot;
using System;

namespace NeonSwarm.Components;

public partial class HealthComponent : Node
{
	[Export] public float MaxHealth { get; set; } = 3f; // The maximum health of the entity. When health reaches 0, the entity dies.
	[Export] public bool IsInvulnerable { get; set; } = false; // If true, the entity will not take damage. Useful for temporary invulnerability after taking damage or for certain enemy types.

	public float CurrentHealth { get; private set; } // The current health of the entity. When health reaches 0, the entity dies.
	public bool IsDead { get; private set; } // Whether the entity is dead. Once true, the entity should not take any more damage.

	public event Action<float, float> HealthChanged; // Event triggered when health changes. Provides current health and max health as parameters.
	public event Action<float> Damaged; // Event triggered when the entity takes damage. Provides the amount of damage taken as a parameter.
	public event Action<float> Healed; // Event triggered when the entity is healed. Provides the amount of health restored as a parameter.
	public event Action Died; // Event triggered when the entity dies.

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ResetHealth();
	}

	// Sets the maximum health of the entity. Can choose whether to heal entity to full and won't allow current health to exceed max health.
	public void SetMaxHealth(float maxHealth, bool healToFull = true)
	{
		MaxHealth = Mathf.Max(1f, maxHealth); // Ensure max health is at least 1 to prevent division by zero and negative health.

		if (healToFull)
		{
			ResetHealth();
		}
		else
		{
			CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth); // Ensure current health does not exceed new max health.
			HealthChanged?.Invoke(CurrentHealth, MaxHealth);
		}
	}

	// Resets the entity's health to full and marks it as alive.
	public void ResetHealth()
	{
		MaxHealth = Mathf.Max(1f, MaxHealth);
		CurrentHealth = MaxHealth;
		IsDead = false;
		HealthChanged?.Invoke(CurrentHealth, MaxHealth);
	}

	public void TakeDamage(float damage)
	{
		if (IsInvulnerable || IsDead || damage <= 0f)
			return;
		
		CurrentHealth = Mathf.Max(CurrentHealth - damage, 0f); // Ensure health does not go below 0.
		Damaged?.Invoke(damage);
		HealthChanged?.Invoke(CurrentHealth, MaxHealth);

		if (CurrentHealth <= 0f)
			Die();
	}

	public void Heal(float amount)
	{
		if (IsDead || amount <= 0f)
			return;
		
		float previousHealth = CurrentHealth;
		CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth); // Ensure health does not exceed max health.
		float healedAmount = CurrentHealth - previousHealth;

		if (healedAmount <= 0f)
			return;
		
		Healed?.Invoke(healedAmount);
		HealthChanged?.Invoke(CurrentHealth, MaxHealth);
	}
	
	private void Die()
	{
		if (IsDead)
			return;
		
		IsDead = true;
		Died?.Invoke();
	}
}
