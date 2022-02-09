using Godot;
using System.Threading.Tasks;

public interface IDestroyable
{
    ushort Durability { get; set; }
    bool Destroyed { get; set; }

    void RecieveDamage(ushort damage);
}

public class Barrel : RigidBody, IDestroyable
{
    [Export] public ushort Durability { get; set; } = 100;
    [Export] private ushort ExplotionDamage { get; set; } = 100;

    public bool Destroyed { get; set; }
    private Timer _destructionTimeTimer;
    private AudioStreamPlayer3D _impactSound;
    private AudioStreamPlayer3D _explotionSound;
    private ExplotionHandler _explotionParticles;
    private MeshInstance _barrelMeshInstance;
    private Area _damageArea;

    public override void _Ready()
    {
        _destructionTimeTimer = GetNode<Timer>("DestroyTimer");
        _impactSound = GetNode<AudioStreamPlayer3D>("Sound/Impact");
        _explotionSound = GetNode<AudioStreamPlayer3D>("Sound/Explotion");
        _barrelMeshInstance = GetNode<MeshInstance>("MeshInstance");
        _explotionParticles = GetNode<ExplotionHandler>("Explotion");
        _damageArea = GetNode<Area>("ExplotionRange");

        _destructionTimeTimer.Connect("timeout", this, nameof(Destroy));
    }

    private void Destroy()
    {
        QueueFree();
    }

    public void RecieveDamage(ushort damage)
    {
        if (Durability - damage > 0)
        {
            float r = GD.Randf() * 0.2f;
            _impactSound.PitchScale = 0.9f + r;
            _impactSound.Play();
            Durability -= damage;
        }
        else
        {
            _destructionTimeTimer.Start();
            DoExplotion();
        }
    }

    private void OnExplotionDamage()
    {
        var nearBodies = _damageArea.GetOverlappingBodies();
        Node[] nearBodiesArray = new Node[nearBodies.Count];
        nearBodies.CopyTo(nearBodiesArray, 0);

        Parallel.ForEach(nearBodiesArray, i => CheckDamage(i));
    }

    private async void CheckDamage(Node body)
    {
        float distance = float.Epsilon;
        switch (body)
        {
            case KinematicBody humanoid:
                if (humanoid is IKilleable hum)
                {   
                    distance = humanoid.GlobalTransform.origin.DistanceSquaredTo(GlobalTransform.origin);
                    var dmgInfo = new DamageInfo((ushort)(ExplotionDamage - distance), GlobalTransform.origin);
                    hum.Stats.RecieveDamage(dmgInfo);
                }
                else if (humanoid is FirstPersonController player)
                {
                    distance = player.GlobalTransform.origin.DistanceSquaredTo(GlobalTransform.origin);
                    var dmgInfo = new DamageInfo((ushort)(ExplotionDamage - distance), GlobalTransform.origin);
                    player.GetNode<Stats>("Stats").RecieveDamage(dmgInfo);
                }
                break;

            case RigidBody prop:
                if (prop != this && prop is IDestroyable obj && !obj.Destroyed)
                {
                    Vector3 direction = (prop.GlobalTransform.origin - GlobalTransform.origin).Normalized();
                    prop.ApplyImpulse(Vector3.Up * 0.5f, direction * (ExplotionDamage - distance) / 2);

                    await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
                    distance = prop.GlobalTransform.origin.DistanceSquaredTo(GlobalTransform.origin);
                    obj.RecieveDamage((ushort)(ExplotionDamage - distance));   
                }
                break;
        }
    }

    private void DoExplotion()
    {
        GetNode<CollisionShape>("CollisionShape").Disabled = true;
        Destroyed = true;

        Mode = ModeEnum.Static;

        _barrelMeshInstance.Visible = false;
        _explotionParticles.EmitParticles();

        float r = GD.Randf() * 0.2f;
        _explotionSound.PitchScale = 0.9f + r;
        _explotionSound.Play();

        CallDeferred(nameof(OnExplotionDamage));
    }
}
