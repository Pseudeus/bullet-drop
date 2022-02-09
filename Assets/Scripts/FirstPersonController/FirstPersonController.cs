using Godot;

public class FirstPersonController : KinematicBody
{ 

    //maybe add another collider for the head
    #region ViewBehaviour
    [Export(PropertyHint.Range, "0.1, 2, 0.1")] public float mouseSensitivity = 0.3f;
    [Export] private float maxPitch = 1.1f;
    [Export] private float minPitch = -1.5f;
    [Export(PropertyHint.Range, "0, 0.5, 0.05")] private float mouseSmoothWeight = 0.2f;
    #endregion

    #region MovementSettings
    [Export] private float walkSpeed = 8f;
    [Export] private float runSpeed = 12f;
    [Export] private float jumpForce = 1f;
    [Export] private float gravity = 9.81f;
    [Export(PropertyHint.Range, "0, 0.5, 0.05")] private float moveSmoothWeight = 0.25f; 
    [Export(PropertyHint.Range, "0, 1, 0.01")] private float runStepLenghten = 0.47f;
    [Export] private float stepInterval = 320f;
    [Export] private AudioStream[] footstepSound;
    #endregion

    #region MotionSettings
    private Motion _motion;
    #endregion

    public MotionState State => _motionState;

    #region PrivateSettings
    private AudioStreamPlayer3D _audioStreamPlayer3D;
    private MotionState _motionState = MotionState.Idle;
    private Spatial _playerHead;
    private float _cameraPitch;
    private float _velocityY;
    private float _speed;
    private Vector2 _targetMouseDelta;
    private Vector2 _currentMouseDelta;
    private Vector2 _targetDirection;
    private Vector2 _currentDirection;
    private Vector3 _velocity;
    private float _steepCycle;
    private float _nextStep;
    #endregion  

    public override void _Ready()
    {
        _playerHead = GetNode<Spatial>("Head");
        Input.SetMouseMode(Input.MouseMode.Captured);

        _motion = GetNode<Motion>("Head");
        _audioStreamPlayer3D = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
    }

    public override void _Input(InputEvent @event)
    {
        //Update Mouse Look
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            _targetMouseDelta.x += mouseMotionEvent.Relative.x * mouseSensitivity * GetProcessDeltaTime() / 5f;
            _targetMouseDelta.y += mouseMotionEvent.Relative.y * mouseSensitivity * GetProcessDeltaTime() / 5f;

            Transform = Transform.Orthonormalized();
            _playerHead.Transform = _playerHead.Transform.Orthonormalized();
        }
    }

    private void UpdateMouseLook()
    {
        _currentMouseDelta = _currentMouseDelta.LinearInterpolate(_targetMouseDelta, mouseSmoothWeight);
        Pitch();
        Yaw();
        _targetMouseDelta = Vector2.Zero;
    }

    private void Pitch()
    {
        _cameraPitch -= _currentMouseDelta.y;
        _cameraPitch = Mathf.Clamp(_cameraPitch, minPitch, maxPitch);
        _playerHead.Rotation = Vector3.Right * _cameraPitch;
    }

    private void Yaw()
    {
        RotateY(-_currentMouseDelta.x);
    }

    private void UpdateMovement(float delta)
    {
        var playerBasis = GlobalTransform.basis;

        if (IsOnFloor())
        {
            _targetDirection = new Vector2(Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"), Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward")).Normalized();

            _velocityY = 0.0f;
            if (Input.IsActionJustPressed("jump"))
            {
                Jump();
            }
        }
        _currentDirection = _currentDirection.LinearInterpolate(_targetDirection, moveSmoothWeight * delta * 40f);//TODO: make this work with the 0 to 1 weight

        _velocityY += gravity * delta;
        SetSpeed();
        
        Vector3 velocity = (playerBasis.z * _currentDirection.y + playerBasis.x * _currentDirection.x) * _speed + Vector3.Down * _velocityY;

        _velocity = MoveAndSlide(velocity, Vector3.Up);
    }

    private void SetSpeed()
    {   
        if (_targetDirection.LengthSquared() < -float.Epsilon || _targetDirection.LengthSquared() > float.Epsilon)
        {
            if (Input.IsActionPressed("left_shift"))
            {
                _motionState = MotionState.Sprinting;
                _speed = runSpeed;
                return;
            }
            _motionState = MotionState.Walking;
            _speed = walkSpeed;
        }
        else
        {
            _motionState = MotionState.Idle;
            _steepCycle = 0;
            _nextStep = 0;
        }
    }

    private void Jump()
    {
        _velocityY = -Mathf.Sqrt(jumpForce * -2 * -gravity);
        _motionState = MotionState.Jumping;
    }

    private void ProgressStecCycle(float speed)
    {
        if (_velocity.LengthSquared() > 0 && ((_targetDirection.x > float.Epsilon || _targetDirection.x < -float.Epsilon) || (_targetDirection.y > float.Epsilon || _targetDirection.y < -float.Epsilon)))
        {
            _steepCycle += (_velocity.LengthSquared() * (speed * (_motionState == MotionState.Walking && _motionState != MotionState.Jumping ? 1f : runStepLenghten))) * GetPhysicsProcessDeltaTime();
        }

        if (_steepCycle < _nextStep) { return; }

        _nextStep = _steepCycle + stepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if(!IsOnFloor()) { return; }

        byte n = (byte)(GD.Randi() % footstepSound.Length);
        _audioStreamPlayer3D.Stream = footstepSound[n];

        float r = GD.Randf() * 0.1f;
        _audioStreamPlayer3D.PitchScale = 0.95f + r;

        _audioStreamPlayer3D.Play();

        footstepSound[n] = footstepSound[0];
        footstepSound[0] = _audioStreamPlayer3D.Stream;
    }

    public override void _PhysicsProcess(float delta)
    {
        UpdateMouseLook();
        UpdateMovement(delta);
        ProgressStecCycle(_speed);
        _motion.Bobbing(_motionState, delta);
    }
}

public enum MotionState
{
    Idle,
    Walking,
    Sprinting,
    Jumping
}
