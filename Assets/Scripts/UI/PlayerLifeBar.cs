using Godot;

public class PlayerLifeBar : TextureProgress
{
    public IDamageable HealthStats { get => _healthStats; set { _healthStats = value; _lifeText.Text = value.HealthPoints.ToString(); } }

    private IDamageable _healthStats;
    private Label _lifeText;
    private Tween _tween;
    

    public override void _Ready()
    {
        _lifeText = GetNode<Label>("Label");
        _tween = GetNode<Tween>("Tween");  
    }

    public void UpdateLifeBar(ushort healthPoints)
    {
        _tween.InterpolateProperty(this, "value", Value, healthPoints * 100 / HealthStats.HealthPoints, 0.4f, Tween.TransitionType.Linear, Tween.EaseType.Out);
        _tween.Start();
        _lifeText.Text = healthPoints.ToString();
    }
}
