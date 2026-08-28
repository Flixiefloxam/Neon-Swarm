using Godot;

public partial class DebugScenePauser : Node
{
	private bool _wasPaused;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		_wasPaused = GetTree().Paused;
		GetTree().Paused = true;
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid(GetTree()))
			GetTree().Paused = _wasPaused;
	}
}
