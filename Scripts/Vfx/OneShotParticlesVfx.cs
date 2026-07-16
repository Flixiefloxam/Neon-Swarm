using Godot;

namespace NeonSwarm.Vfx;

public partial class OneShotParticlesVfx : Node2D
{
	public Color TintColor { get; private set; } = Colors.White;
	private GpuParticles2D _particles;

	public void Initialize(Color tintColor)
	{
		TintColor = tintColor;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_particles = GetNodeOrNull<GpuParticles2D>("Particles");

		if (_particles == null)
		{
			GD.PushWarning($"{Name} has no GPUParticles2D child.");
			QueueFree();
			return;
		}

		_particles.OneShot = true;
		_particles.SelfModulate = TintColor;
		_particles.Finished += OnParticlesFinished;
		_particles.Restart();
	}

	public override void _ExitTree()
	{
		if (_particles != null)
			_particles.Finished -= OnParticlesFinished;
	}

	private void OnParticlesFinished()
	{
		QueueFree();
	}
}
