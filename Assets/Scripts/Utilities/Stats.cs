using Godot;

public interface IDamageable
{
    ushort HealthPoints { get; set; }

    void RecieveDamage(in DamageInfo dmg);
    void ConnectToSignal(in string name, in Object target, string method);
}
public interface IDamager
{
    ushort DamagePoints { get; set; }
    
    void GiveDamage(IDamageable target, in DamageInfo dmg);
}

public class Stats : Node, IDamageable, IDamager
{
    [Signal] public delegate void CriticalDamageTaken();
    [Signal] public delegate void DamageTaken(ushort currentHealth, Object damageOrigin);

    [Export] public ushort HealthPoints { get; set; } = 8;
    [Export] public ushort DamagePoints { get; set; } = 1;

    public ushort CurrentHealthPoints => _currentHealthPoints;

    private ushort _currentHealthPoints;

    public override void _Ready()
    {
        _currentHealthPoints = HealthPoints;
    }

    public void GiveDamage(IDamageable target, in DamageInfo dmg)
    {
        target.RecieveDamage(dmg);
    }

    public void RecieveDamage(in DamageInfo dmg)
    {
        if (_currentHealthPoints - dmg.Amount > 0)
        {
            _currentHealthPoints -= dmg.Amount;
            EmitSignal(nameof(DamageTaken), _currentHealthPoints, dmg.Origin);
            GD.Print(_currentHealthPoints, "/", HealthPoints, "     origin: ", dmg.Origin);
        }
        else
        {
            _currentHealthPoints = 0;
            EmitSignal(nameof(DamageTaken), _currentHealthPoints, dmg.Origin);
            EmitSignal(nameof(CriticalDamageTaken));
            GD.Print("Death!");
        }
    }
    public void ConnectToSignal(in string name, in Object target, string method)
    {
        Connect(name, target, method);
    }
}
