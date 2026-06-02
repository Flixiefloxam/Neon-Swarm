using Godot;

namespace NeonSwarm.UI;

public partial class MenuGridBackground : Control
{
	[Export] public Texture2D GridTexture { get; set; } // The grid texture that will be repeated.

	[Export] public Vector2 ScrollSpeed { get; set; } = new(8f, 5f); // How fast the background scrolls in pixels per second.
	[Export] public float TileScale { get; set; } = 1f; // Multiplies the size of each grid texture.
	[Export] public Color TintColor { get; set; } = new(1f, 1f, 1f, 0.8f); // The tint/fade on the grid.

	private Vector2 _scrollOffset = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (GridTexture == null)
			return;
		
		_scrollOffset += ScrollSpeed * (float) delta;
		QueueRedraw();
	}

    public override void _Draw()
    {
        if (GridTexture == null)
			return;
		
		Vector2 textureSize = GridTexture.GetSize();

		if (textureSize.X <= 0f || textureSize.Y <= 0f)
			return;
		
		float scale = Mathf.Max(TileScale, 0.01f);
		Vector2 tileSize = textureSize * scale;

		// Wrap the scroll offset so it doesn't grow forever, while still maintaining the infinite scroll effect
		float offsetX = Wrap(_scrollOffset.X, tileSize.X);
		float offsetY = Wrap(_scrollOffset.Y, tileSize.Y);

		for (float x = -offsetX; x < Size.X; x += tileSize.X)
		{
			for (float y = -offsetY; y < Size.Y; y += tileSize.Y)
			{
				Rect2 tileRect = new(
					new Vector2(x,y),
					tileSize
				);

				DrawTextureRect(GridTexture, tileRect, false, TintColor);
			}
		}
    }


	// Wraps a value to the range [0, max)
	private static float Wrap(float value, float max)
	{
		if (max <= 0f)
			return 0f;
		
		return ((value % max) + max) % max; // prevents negative % results
	}
}
