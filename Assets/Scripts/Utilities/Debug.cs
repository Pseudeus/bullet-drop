using Godot;
using Godot.Collections;

public class Debug : CanvasLayer
{
    private bool _fullScreen;
    private Label _frameRate;

    public override void _Ready()
    {
        _frameRate = GetNode<Label>("FrameRate");
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Scancode == (uint)KeyList.F11 && !eventKey.Echo && eventKey.IsPressed())
            {
                _fullScreen = !_fullScreen;
                OS.WindowFullscreen = _fullScreen;
            }
            if (eventKey.Scancode == (uint)KeyList.F5 && !eventKey.Echo && eventKey.IsPressed())
            {
                GetTree().ReloadCurrentScene();
                GetTree().Paused = false;
            }
        }
    }
    public override void _Process(float delta)
    {
        _frameRate.Text = Engine.GetFramesPerSecond().ToString();
    }
}
