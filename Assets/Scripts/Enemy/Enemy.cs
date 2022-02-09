using Godot;

public interface IKilleable
{
    Enemy.State CurrentState { get; set; }
    bool IsDeath { get; set; }
    IDamageable Stats { get; }

    void Enabled(bool status);
    void SetGlobalTransformPosition(Vector3 position);
}

public class Enemy : KinematicBody, IKilleable
{
    [Export] private float movementSpeed = 4f;
    [Export] private float acceleration = 50f;
    [Export] private float turnSpeed = 0.2f;
    [Export] private float attackRange = 1f;

    public IDamageable Stats { get; private set; }
    public bool IsDeath { get; set; }
    public State CurrentState { get; set; }

    private AnimationPlayer _anim;
    private NavMeshAgent _navMeshAgent;
    private KinematicBody _player;
    private Vector3 _velocity;
    private float _distanceToTargetSquared;
    private float _targetRotationAngleRad;
    private float _currentRotationAngleRad;
    private DamageInfo _dmgInfo = new DamageInfo();

    public enum State
    {
        Idle,
        Seeking,
        Attacking,
        Resting
    }

    public void Enabled(bool status) 
    { 
        Visible = status; 
        IsDeath = !status;
        GetNode<CollisionShape>("CollisionShape").Disabled = !status;
        
        if (!status) _navMeshAgent.StopPathUpdating();
    }

    public void SetGlobalTransformPosition(Vector3 position) { Translation = position; }

    public override void _Ready()
    {
        CurrentState = State.Idle;
        var parent = GetTree().CurrentScene;

        _player = parent.GetNode<KinematicBody>("Player");
        _anim = GetNode<AnimationPlayer>("AnimationPlayer");
        _navMeshAgent = GetNode<NavMeshAgent>("NavMeshAgent");

        //_navMeshAgent.StopingDistance = attackRange - 0.15f * 0.15f;

        Stats = GetNode<IDamageable>("Stats");
        Stats.ConnectToSignal("CriticalDamageTaken", this, nameof(OnCriticalDamageTaken));
        Stats.ConnectToSignal("DamageTaken", this, nameof(OnDamageTaken));
        
        GetNode<Area>("ChaseRange").Connect("body_entered", this, nameof(OnChaseRangeBodyEntered));
    }
    private void EngageTarget()
    {
        if (_navMeshAgent.HasStoped)
        {
            CurrentState = State.Attacking;
            AttackTarget();
        }
    }
    private void AttackTarget()
    {
        _anim.CurrentAnimation = "Attack";
        _anim.Play();
    }
    private void DamageTarget()
    {
        if (IsInstanceValid(_player))
        {
            var damage = Stats as IDamager;
            _dmgInfo.Amount = damage.DamagePoints;
            _dmgInfo.Origin = GlobalTransform.origin;
            damage.GiveDamage(_player.GetNode<IDamageable>("Stats"), _dmgInfo);
            GD.Print("I deal to the player ", damage.DamagePoints, " of damage");
        }
    }

    private void FaceTarget()
    {
        Vector3 direction = (_player.GlobalTransform.origin - GlobalTransform.origin).Normalized();
        _targetRotationAngleRad = Mathf.Atan2(-direction.z, direction.x) - Mathf.Deg2Rad(90);
        _currentRotationAngleRad = Mathf.LerpAngle(_currentRotationAngleRad, _targetRotationAngleRad, turnSpeed);
        Rotation = new Vector3(0f, Mathf.LerpAngle(Rotation.y, _currentRotationAngleRad, turnSpeed / 2), 0f);
    }

    private void OnDamageTaken(ushort c, Object o) //todo refactor this shit
    {
        if (!IsDeath)
        {
            _distanceToTargetSquared = (_player.GlobalTransform.origin - GlobalTransform.origin).LengthSquared();
            CurrentState = State.Seeking;
            _navMeshAgent.StartPathUpdating();
        }
    }
    private void OnCriticalDamageTaken()
    {
        Enabled(false);
        QueueFree();
    }
    private void OnChaseRangeBodyEntered(Node body)
    {
        if (body is KinematicBody && !IsDeath)
        {
            _distanceToTargetSquared = (_player.GlobalTransform.origin - GlobalTransform.origin).LengthSquared();
            CurrentState = State.Seeking;
            _navMeshAgent.StartPathUpdating();
        }
    }

    private void MoveState(float delta)
    {
        var direction = _navMeshAgent.FollowPath();

        if (direction != Vector3.Zero)
        {
            _velocity = _velocity.MoveToward(direction * movementSpeed, acceleration * delta);
        }
        else 
        {
            _velocity = _velocity.MoveToward(Vector3.Zero, acceleration * delta * 5f);
        }
    }

    public override void _Process(float delta)
    {
        if (!IsDeath && IsInstanceValid(_player))
        {
            switch (CurrentState)
            {
                case State.Idle:
            
                    break;
                case State.Seeking:
                    _distanceToTargetSquared = (_player.GlobalTransform.origin - GlobalTransform.origin).LengthSquared();
                    _navMeshAgent.SetDestination(_player.GlobalTransform.origin);
                    MoveState(delta);
                    EngageTarget();
                    break;
                case State.Attacking:
                    FaceTarget();
                    break;
            }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_velocity != Vector3.Zero && !IsDeath)
        {
            _velocity = MoveAndSlide(_velocity, Vector3.Up);
        }
    }
}
