using Godot;

public class Fade : ColorRect
{
    [Export] public float FadeTime { get; set; } = 1f;

    private Timer _timer;

    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");

        _timer.WaitTime = FadeTime;
        _timer.OneShot = true;
        _timer.Connect("timeout", this, nameof(Finished));

        _timer.Start();
    }

    private void Finished()
    {
        QueueFree();
    }

    private void FadeOutFinished()
    {
        Visible = false;
    }

    public void FadeOut()
    {
        Modulate = Color.Color8(255, 255, 255, (byte)(_timer.TimeLeft * 255 / FadeTime));
    }

    public void FadeIn()
    {
        
    }

    public override void _Process(float delta)
    {
        FadeOut();
    }
}
