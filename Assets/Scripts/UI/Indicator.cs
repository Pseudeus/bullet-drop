using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Indicator : Control
{
    [Export] private PackedScene indicatorPointer;

    private List<DamagePointerHandler> _damagePointersRegist = new List<DamagePointerHandler>();
    private Vector2 _playerPositionXZ;
    private float _offsetAngle;

    public void SetDamageCoordinates(Vector2 player, float rot)
    {
        _playerPositionXZ = player;
        _offsetAngle = rot;
    }

    public void CreateDamagePointer(Vector2 enemyPosition, Vector2 playerPosition, float offsetAngle)
    {
        var pointer = indicatorPointer.Instance<DamagePointerHandler>();

        pointer.Holder = this;
        pointer.SetDamageOrigin(enemyPosition);
        pointer.SetDirection(playerPosition, offsetAngle);

        _damagePointersRegist.Add(pointer);

        AddChild(pointer);
    }

    public void DeleteDamagePointer(DamagePointerHandler pointer)
    {
        if (_damagePointersRegist.Contains(pointer))
        {
            _damagePointersRegist.Remove(pointer);
        }
        pointer.QueueFree();
    }

    public override void _Process(float delta)
    {
        Parallel.ForEach(_damagePointersRegist, i => i.SetDirection(_playerPositionXZ, _offsetAngle));
    }
}