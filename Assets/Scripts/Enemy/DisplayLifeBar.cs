using Godot;

public class DisplayLifeBar : MeshInstance
{
    [Export] private NodePath StatsNode { get; set; }

    private IDamageable _healthStats;
    private Viewport _viewport;
    private TextureProgress _lifeBar;
    private Tween _tween;

    public override void _Ready()
    {
        _healthStats = GetNode<IDamageable>(StatsNode);
        _healthStats.ConnectToSignal("DamageTaken", this, nameof(OnStatsDamageTaken));

        _viewport = GetNode<Viewport>("LifeBarViewport");
        _lifeBar = GetNode<TextureProgress>("LifeBarViewport/TextureProgress");
        _tween = _lifeBar.GetNode<Tween>("Tween");

        _lifeBar.Value = 100;
        _viewport.RenderTargetUpdateMode = Viewport.UpdateMode.Once;
    }

    private async void OnStatsDamageTaken(ushort currentHealth, Object o)
    {
        _viewport.RenderTargetUpdateMode = Viewport.UpdateMode.WhenVisible;
        _tween.InterpolateProperty(_lifeBar, "value", _lifeBar.Value, currentHealth * 100 / _healthStats.HealthPoints, 0.2f, Tween.TransitionType.Linear, Tween.EaseType.Out);
        _tween.Start();
        await ToSignal(_tween, "tween_all_completed");
        _viewport.RenderTargetUpdateMode = Viewport.UpdateMode.Once;
    }
}
