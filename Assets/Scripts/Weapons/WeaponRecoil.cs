using Godot;

public class WeaponRecoil : Spatial
{
    #region SpeedSettings
    [Export] private float positionalRecoilSpeed = 8f;
    [Export] private float rotationalRecoilSpeed = 32f;
    [Export] private float positionalReturnSpeed = 18f;
    [Export] private float rotationalReturnSpeed = 36f;
    #endregion

    #region AmountSettings
    [Export] private Vector3 recoilRotation = new Vector3(10, 5, 7);
    [Export] private Vector3 recoilKickBack = new Vector3(0.015f, 0f, -0.2f);
    [Export] private Vector3 recoilRotationAim = new Vector3(10f, 4f, 6f);
    [Export] private Vector3 recoilKickBackAim = new Vector3(0.015f, 0f, -0.2f);
    #endregion

    #region  Values
    private Spatial _recoilPosition;

    private Vector3 _rotationalRecoil;
    private Vector3 _positionalRecoil;
    private Vector3 _rotation;
    private bool _aiming;
    #endregion

    public override void _Ready()
    {
        _recoilPosition = GetNode<Spatial>("../");
        GetNode<WeaponZoom>("../../../Camera/Zoom").Connect("AimingChanged", this, nameof(OnAimChanged));
    }

    public override void _PhysicsProcess(float delta)
    {
        _rotationalRecoil = _rotationalRecoil.LinearInterpolate(Vector3.Zero, rotationalReturnSpeed * delta);//TODO: make this work with the 0 to 1 weight
        _positionalRecoil = _positionalRecoil.LinearInterpolate(Vector3.Zero, positionalReturnSpeed * delta);//TODO: make this work with the 0 to 1 weight

        _recoilPosition.Translation = Translation.MoveToward(_positionalRecoil, positionalRecoilSpeed * delta); //TODO: find a smoother interpolation
        _rotation = _rotation.MoveToward(_rotationalRecoil, rotationalRecoilSpeed * delta);     //TODO: find a smoother interpolation

        RotationDegrees = _rotation;
    }

    private void Fire()
    {
        if (_aiming)
        {
            _rotationalRecoil -= new Vector3(-recoilRotationAim.x, (float)GD.RandRange(-recoilRotation.y, recoilRotationAim.y), (float)GD.RandRange(-recoilRotationAim.z, recoilRotationAim.z));
            _positionalRecoil -= new Vector3((float)GD.RandRange(-recoilKickBackAim.x, recoilKickBackAim.x), (float)GD.RandRange(-recoilKickBackAim.y, recoilKickBackAim.y), recoilKickBackAim.z);
        }
        else
        {
            _rotationalRecoil -= new Vector3(-recoilRotation.x, (float)GD.RandRange(-recoilRotation.y, recoilRotation.y), recoilRotation.z);
            _positionalRecoil -= new Vector3((float)GD.RandRange(-recoilKickBack.x, recoilKickBack.x), (float)GD.RandRange(-recoilKickBack.y, recoilKickBack.y), recoilKickBack.z);
        }
    }

    public void OnDoShoot()
    {
        Fire();
        Transform = Transform.Orthonormalized();
    }
    public void OnAimChanged(bool aim)
    {
        _aiming = aim;
    }
}
