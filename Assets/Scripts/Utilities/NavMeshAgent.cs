using Godot;
using System.Collections.Generic;
using System.Linq;

public class NavMeshAgent : Node
{   
    [Export] private NodePath navigationPath;
    [Export] public float StopingDistance { get; set; } = 1f;
    [Export(PropertyHint.Range, "0.0, 1.0, 0.05")] public float SteeringSpeed { get; set; } = 0.2f;
    [Export(PropertyHint.Range, "0.0, 0.6, 0.05")] private float PathDetectionMargin { get; set; } = 0.4f;
    [Export] private float YOffset { get; set; } = 0.4f;
    [Export] private bool UseDownForce { get; set; } = true;
    [Export] private float DownForce { get; set; } = 0.8f;

    public bool HasStoped => _distance2Player <= StopingDistance * StopingDistance;

    private KinematicBody _parent;
    private Navigation _navigation;
    private List<Vector3> _path = new List<Vector3>();
    private byte _navPathNode;
    private Vector3 _direction;
    private float _distance2Player;
    private Timer _pathUpdateTimer;
    private Vector3 _destination;
    private float _velocityY;
    private float _targetRotationAngleRad;
    private float _currentRotationAngleRad;
    private float _longRangeUpdateFactor = 2.5f;
    private float _midRangeUpdateFactor = 1.8f;
    private float _shortRangeUpdateFactor = 0.2f;

    public override void _Ready()
    {
        if (navigationPath != null)
        {
            _navigation = GetNode<Navigation>(navigationPath);
        }
        else
        {
            _navigation = GetTree().CurrentScene.GetNode<Navigation>("Navigation");
        }

        _parent = GetParent<KinematicBody>();
        _pathUpdateTimer = GetNode<Timer>("PathUpdateTimer");
        _pathUpdateTimer.Connect("timeout", this, nameof(OnPathUpdateTimerTimeout));
    }

    public void SetDestination(in Vector3 targetGlobalPosition)
    {   
        _destination = targetGlobalPosition;
    }

    public void StartPathUpdating()
    {
        UpdatePath(_destination);
        _pathUpdateTimer.Start();
    }

    public void StopPathUpdating()
    {
        _pathUpdateTimer.Stop();
    }

    private void UpdatePath(in Vector3 target)
    {
        _path.Clear();
        _path = _navigation.GetSimplePath(_parent.GlobalTransform.origin, target).ToList<Vector3>();
        _navPathNode = 0;
        //GD.Print(_path.Count, "     ", this, "  ", _path[1]);
    }

    public void ForceUpdate()
    {
        UpdatePath(_destination);
    }

    private void RefreshTimeFactor(float dis2Player)
    {
        if (dis2Player >= 1750)
        {
            _pathUpdateTimer.WaitTime = _longRangeUpdateFactor;
        }
        else if (dis2Player > 500 && dis2Player < 1750f)
        {
            _pathUpdateTimer.WaitTime = _midRangeUpdateFactor;
        }
        else
        {
            _pathUpdateTimer.WaitTime = _shortRangeUpdateFactor;
        }
    }

    public Vector3 FollowPath()
    {
        var direction = (_path[_navPathNode] + Vector3.Down * YOffset) - _parent.GlobalTransform.origin;
        _distance2Player = (_destination - _parent.GlobalTransform.origin).LengthSquared();

        if (HasStoped) return Vector3.Zero;

        if (direction.LengthSquared() < PathDetectionMargin * PathDetectionMargin && _navPathNode < _path.Count - 1)
        {
            _navPathNode++;
            RefreshTimeFactor(_distance2Player);
            _direction = ((_path[_navPathNode] + Vector3.Down * YOffset) - _parent.GlobalTransform.origin).Normalized();
            FacePath();
            //GD.Print("target_point", _path[_navPathNode], "     node_num: ", _navPathNode, "     direction: ", _direction, "    path_lenght: ", _path.Count);
        }

        if (UseDownForce) 
        { 
            _velocityY -= DownForce * GetProcessDeltaTime();
            _direction.y += _velocityY; 
            if (_parent.IsOnFloor()) _velocityY = 0f;
        }

        return _direction;
    }

    private void FacePath()
    {
        _targetRotationAngleRad = Mathf.Atan2(-_direction.z, _direction.x) - Mathf.Deg2Rad(90);  
    }

    private void OnPathUpdateTimerTimeout()
    {
        UpdatePath(_destination);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!HasStoped)
        {
            _currentRotationAngleRad = Mathf.LerpAngle(_currentRotationAngleRad, _targetRotationAngleRad, SteeringSpeed);
            _parent.Rotation = new Vector3(0f, Mathf.LerpAngle(_parent.Rotation.y, _currentRotationAngleRad, SteeringSpeed / 2), 0f);
        }

        if (_navPathNode > 0)
        {
            float prevNodeToParentSquaredDistance = (_parent.GlobalTransform.origin - (_path[_navPathNode - 1] + Vector3.Down * YOffset)).LengthSquared();
            float prevNodeToCurrentNodeSquaredDistance = ((_path[_navPathNode] + Vector3.Down * YOffset) - (_path[_navPathNode - 1] + Vector3.Down * YOffset)).LengthSquared() + PathDetectionMargin * PathDetectionMargin * 2;

            if (prevNodeToParentSquaredDistance > prevNodeToCurrentNodeSquaredDistance)
            {
                _direction = ((_path[_navPathNode] + Vector3.Down * YOffset) - _parent.GlobalTransform.origin).Normalized();
                FacePath();
            }
        }
    }
}
