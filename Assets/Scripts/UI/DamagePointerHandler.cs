using Godot;

public class DamagePointerHandler : Control
{
    public Indicator Holder { get; set; }
    private Timer _timer;
    private Vector2 _enemyPositionXZ;
    private Vector2 _playerPositionXZ;
    private float _offsetAngle;

    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _timer.Connect("timeout", this, nameof(EndLifeReached), null, 4);
        _timer.Start();
    }

    private void EndLifeReached()
    {
        Holder.DeleteDamagePointer(this);
    }

    public void SetDamageOrigin(Vector2 enemyPosXZ)
    {
        _enemyPositionXZ = enemyPosXZ;
    }

    public void SetDirection(Vector2 playerPositionXZ, float offsetAngle)
    {
        _playerPositionXZ = playerPositionXZ;
        _offsetAngle = offsetAngle;
    }

    public override void _Process(float delta)
    {
        RectRotation = Mathf.Rad2Deg(Mathf.LerpAngle(Mathf.Deg2Rad(RectRotation), 
                                                    Mathf.Atan2(_enemyPositionXZ.y - _playerPositionXZ.y, _enemyPositionXZ.x - _playerPositionXZ.x) + 
                                                    Mathf.Deg2Rad(_offsetAngle + 90), 0.1f));
    }
}
