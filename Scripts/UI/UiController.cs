using Godot;

namespace NeonSwarm.UI;

public partial class UiController : Node
{
	[Export] public float MouseMoveThreshold { get; set; } = 1.5f; // How much the mouse has to move for it to count
	[Export] public bool ReleaseFocusOnMouseMove { get; set; } = true; // Should the ui focus release on mouse move

	private Control _defaultFocus; // The control that is focused by default(if grabImmediately is true).
	private Control _lastFocus; // The control that is refocused when a focus nav key is pressed after focus has been cleared by mouse movement.

	// Called when the ui enters the scene to set the default focus node.
	public void SetDefaultFocus(Control control, bool grabImmediately = true)
	{
		_defaultFocus = control;
		_lastFocus = control;

		if (grabImmediately && IsValidFocusTarget(control))
			control.CallDeferred(Control.MethodName.GrabFocus);
	}

	// Clears the default focus node.
	public void ClearDefaultFocus(Control control)
	{
		if (_defaultFocus == control)
			_defaultFocus = null;

		if (_lastFocus == control)
			_lastFocus = null;
	}

    public override void _Input(InputEvent inputEvent)
	{
		if (ReleaseFocusOnMouseMove && inputEvent is InputEventMouseMotion mouseMotion)
		{
			if (mouseMotion.Relative.Length() >= MouseMoveThreshold)
				ReleaseCurrentFocus();
			
			return;
		}

		if (IsFocusNavigationInput(inputEvent))
			RestoreFocusIfNeeded();
	}

	private void ReleaseCurrentFocus()
	{
		Control focusOwner = GetViewport().GuiGetFocusOwner();

		if (IsValidFocusTarget(focusOwner))
			_lastFocus = focusOwner;
		
		GetViewport().GuiReleaseFocus();
	}

	private void RestoreFocusIfNeeded()
	{
		if (GetViewport().GuiGetFocusOwner() != null)
			return;
		
		Control target = IsValidFocusTarget(_lastFocus)
			? _lastFocus
			: _defaultFocus;
		
		if (IsValidFocusTarget(target))
			target.CallDeferred(Control.MethodName.GrabFocus);
	}
	
	// Returns whether given InputEvent is a focus nav input
	private static bool IsFocusNavigationInput(InputEvent inputEvent)
	{
		return
			inputEvent.IsActionPressed("ui_up") ||
			inputEvent.IsActionPressed("ui_down") ||
			inputEvent.IsActionPressed("ui_left") ||
			inputEvent.IsActionPressed("ui_right") ||
			inputEvent.IsActionPressed("ui_focus_next") ||
			inputEvent.IsActionPressed("ui_focus_prev");
	}

	// Returns whether the given control is a valid node to focus
	private static bool IsValidFocusTarget(Control control)
	{
		if (control == null)
			return false;

		if (!control.IsInsideTree())
			return false;

		if (!control.IsVisibleInTree())
			return false;

		if (control.FocusMode == Control.FocusModeEnum.None)
			return false;

		if (control is BaseButton button && button.Disabled)
			return false;

		return true;
	}
}
