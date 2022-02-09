using Godot;
using Godot.Collections;

public class Bullet : Spatial
{
    [Export] public float speed = 900f;
    [Export] public float gravity = 9.81f;
    [Export] public float lifeTime = 5;
    [Export] public byte damage = 2;
    [Export] private bool halfCalculateProcess;

    private Vector3 _startPosition;
    private Vector3 _startForward;
    private float _currentTime;
    private bool _processFrame = true;

    private Timer lifeTimeTimer;

    public override void _Ready()
    {
        lifeTimeTimer = GetNode<Timer>("Timer");
        lifeTimeTimer.WaitTime = lifeTime;
        lifeTimeTimer.Connect("timeout", this, nameof(OnTimerTimeout));
        lifeTimeTimer.Start();

        _startPosition = GlobalTransform.origin;
        _startForward = GlobalTransform.basis.z;
    }

    private Vector3 FindPointOnParabola(float time)
    {
        Vector3 point = _startPosition + (_startForward * speed * time);
        Vector3 gravityVec = Vector3.Up * gravity * time * time;
        return point - gravityVec;
    }

    private bool CastRayBetweenPoints(Vector3 startPoint, Vector3 endPoint, out Dictionary intersection)
    {
        var spaceState = GetWorld().DirectSpaceState;
        intersection = spaceState.IntersectRay(startPoint, endPoint);

        if (intersection.Count > 0)
        {
            return true;
        }

        return false;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (halfCalculateProcess)
        {
            if (_processFrame)
            {
                _processFrame = !_processFrame;
            }
            else
            {
                _processFrame = !_processFrame;
                return;
            }
        }

        var hit = new Dictionary();
        _currentTime = lifeTimeTimer.TimeLeft - lifeTime;
        float prevTime = _currentTime + delta;
        float nextTime = _currentTime - delta;

        Vector3 currentPoint = FindPointOnParabola(_currentTime);
        Vector3 nextPoint = FindPointOnParabola(nextTime);

        if (prevTime < 0)
        {
            Vector3 prevPoint = FindPointOnParabola(prevTime);

            if (CastRayBetweenPoints(currentPoint, prevPoint, out hit))
            {
                if (hit["collider"] is KinematicBody enemy)
                {
                    //enemy.GetNode<IDamageable>("Stats").RecieveDamage(damage);
                    GD.Print("Enemy Hitted!");
                }
                QueueFree();
            }
        }
    
        

        if (CastRayBetweenPoints(currentPoint, nextPoint, out hit))
        {
            if (hit["collider"] is KinematicBody enemy)
            {
                //enemy.GetNode<IDamageable>("Stats").RecieveDamage(damage);
                GD.Print("Enemy Hitted!");
            }
            QueueFree();
        }

        Translation = currentPoint;
        //var direction = (nextPoint - currentPoint).Normalized();
        //GlobalTranslate(direction * speed * delta);
    }

    private void OnTimerTimeout()
    {
        QueueFree();
    }
}
