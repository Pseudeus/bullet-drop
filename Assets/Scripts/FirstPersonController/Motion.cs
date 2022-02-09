using Godot;

public class Motion : Spatial
{
    #region MotionSettings
    [Export] private float idleMotionAmplitude = 0.005f;
    [Export] private float idleMotionSpeed = 0.5f;
    [Export] private float walkingMotionAmplitude = 0.015f;
    [Export] private float walkingMotionSpeed = 5f;
    [Export] private Vector2 sprintingMotionAmplitude = new Vector2(0.05f, 0.020f);
    [Export] private float sprintingMotionSpeed = 8f;
    [Export(PropertyHint.Range, "0.0, 0.5, 0.05")] private float smoothWeaponFactor = 0.15f;
    [Export(PropertyHint.Range, "0.0, 0.5, 0.05")] private float smoothCameraFactor = 0.1f;
    #endregion

    #region PrivValues
    private Position3D _weapons;
    private float _gunBobbingTimeFactor;
    private float _cameraBobbingTimeFactor;
    private Vector3 _aimingWeaponPosition;
    internal Vector3 startWeaponPosition;
    internal Vector3 startCameraPosition;
    private Vector3 _targetWeaponPosition;
    private Vector3 _targetCameraPosition;
    private Vector3 _camRot;
    private Vector3 _weaRot;
    private bool _aiming;
    #endregion

    public override void _Ready()
    {
        _weapons = GetNode<Position3D>("Weapons");
        _aimingWeaponPosition = GetNode<Position3D>("Weapons/Aim").Translation;

        GetNode<WeaponZoom>("Camera/Zoom").Connect("AimingChanged", this, nameof(OnAimingChanged));

        startWeaponPosition = _weapons.Translation;
        startCameraPosition = Translation;
    }

    public void Bobbing(in MotionState motionState, in float delta)
    {
        switch(motionState)
        {
            case MotionState.Idle:
                GunBobbing(idleMotionSpeed, idleMotionAmplitude, idleMotionAmplitude);
                _gunBobbingTimeFactor += delta;
                _cameraBobbingTimeFactor = 0;
                break;

            case MotionState.Walking:
                GunBobbing(walkingMotionSpeed, walkingMotionAmplitude, walkingMotionAmplitude);
                _gunBobbingTimeFactor += delta;

                CameraBobbing(walkingMotionSpeed, walkingMotionAmplitude, walkingMotionAmplitude);
                _cameraBobbingTimeFactor += delta;
                break;

            case MotionState.Sprinting:
                GunBobbing(sprintingMotionSpeed, sprintingMotionAmplitude.x, sprintingMotionAmplitude.y);
                _gunBobbingTimeFactor += delta;

                CameraBobbing(sprintingMotionSpeed, sprintingMotionAmplitude.x, sprintingMotionAmplitude.y);
                _cameraBobbingTimeFactor += delta;
                break;
        }
    }

    private void GunBobbing(in float bobbingSpeed, in float horizontalAmplitude, in float verticalAmplitude)
    {
       if (_aiming)
       {
            _targetWeaponPosition = _aimingWeaponPosition + CalculateBobbingOffset(_gunBobbingTimeFactor, bobbingSpeed, horizontalAmplitude / 8, verticalAmplitude / 8);
            _weaRot = _weaRot.LinearInterpolate(_targetWeaponPosition, smoothWeaponFactor);//TODO: make this work with the 0 to 1 weight
            _weapons.Translation = _weapons.Translation.MoveToward(_weaRot, smoothWeaponFactor);//TODO: find a smoother interpolation
       }
       else
       {
            _targetWeaponPosition = startWeaponPosition + CalculateBobbingOffset(_gunBobbingTimeFactor, bobbingSpeed, horizontalAmplitude, verticalAmplitude);
            _weaRot = _weaRot.LinearInterpolate(_targetWeaponPosition, smoothWeaponFactor);//TODO: make this work with the 0 to 1 weight
            _weapons.Translation = _weapons.Translation.MoveToward(_weaRot, smoothWeaponFactor);//TODO: find a smoother interpolation
       }
    }

    private void CameraBobbing(in float bobbingSpeed, in float horizontalAmplitude, in float verticalAmplitude)
    {
        _targetCameraPosition = startCameraPosition + CalculateBobbingOffset(_cameraBobbingTimeFactor, bobbingSpeed, horizontalAmplitude * 2.5f, verticalAmplitude * 2.5f);
        _camRot = _camRot.LinearInterpolate(_targetCameraPosition, smoothCameraFactor);//TODO: make this work with the 0 to 1 weight
        Translation = Translation.MoveToward(_camRot, smoothCameraFactor);//TODO: find a smoother interpolation
        RotationDegrees = RotationDegrees.MoveToward(-_camRot, smoothCameraFactor);
        Transform = Transform.Orthonormalized();
    }

    private Vector3 CalculateBobbingOffset(in float t, in float bobSpeed, in float horizontalAmplitude, in float verticalAmplitude)
    {
        float horizontalOffset = 0f;
        float verticalOffset = 0f;
        Vector3 offset = Vector3.Zero;

        if (t > 0)
        {
            horizontalOffset = Mathf.Cos(t * bobSpeed) * horizontalAmplitude;
            verticalOffset = Mathf.Sin(t * bobSpeed * 2) * verticalAmplitude;

            offset = Vector3.Right * horizontalOffset + Vector3.Up * verticalOffset;
        }
        return offset;
    }

    private void OnAimingChanged(bool aim)
    {
        _aiming = aim;
    }
}
