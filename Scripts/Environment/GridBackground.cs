using Godot;

namespace NeonSwarm.Environment;

public partial class GridBackground : Node2D
{
    [Export] public int TileSize { get; set; } = 64; // The size of each tile in pixels.

    private Camera2D _targetCamera; // The camera that the grid will follow.

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        FindCamera();
        SnapToCamera();
    }
    
    public override void _Process(double delta)
    {
        if (_targetCamera == null)
            FindCamera();
        
        if (_targetCamera == null)
            return; // If we still don't have a camera, exit early to avoid errors.

        SnapToCamera();
    }

    // Finds the first Camera2D in the viewport and sets it as the target camera.
    private void FindCamera()
    {
        _targetCamera = GetViewport().GetCamera2D();
    }

    // Snaps the grid's position to align with the camera's position based on the tile size.
    private void SnapToCamera()
    {
        Vector2 cameraPos = _targetCamera.GlobalPosition;

        GlobalPosition = new Vector2(
            Mathf.Floor(cameraPos.X / TileSize) * TileSize,
            Mathf.Floor(cameraPos.Y / TileSize) * TileSize
        );
    }
}
