using Godot;
using Godot.Collections;

public class CameraRecoil : Camera
{
    #region RecoilSettings
    [Export] private float rotationSpeed = 6f;
    [Export] private float returnSpeed = 25f;
    #endregion

    #region RecoilStrenghts
    [Export] private Vector3 recoilRotation = new Vector3(2f, 2f, 2f);
    [Export] private Vector3 recoilRotationAiming = new Vector3(0.5f, 0.5f, 0.5f);
    #endregion

    #region Priv
    private bool _aiming;
    private Vector3 _currentRotation;
    private Vector3 _rotation;
    #endregion

    public override void _Ready()
    {    
        GetNode<WeaponSelect>("../Weapons").Connect("ChildrenFound", this, nameof(OnWeaponsChildrenFound), null, 4);
        GetNode<WeaponZoom>("Zoom").Connect("AimingChanged", this, nameof(OnAimChanged));
    }

    private void OnWeaponsChildrenFound(in Array<Spatial> weapons)
    {
        foreach (var weapon in weapons)
        {
            if (weapon is Weapon)
            {
                weapon.Connect("DoShoot", this, nameof(OnDoShoot));
            }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        _currentRotation = _currentRotation.LinearInterpolate(Vector3.Zero, returnSpeed * delta); //TODO: make this work with the 0 to 1 weight
        _rotation = _rotation.MoveToward(_currentRotation, rotationSpeed * delta); //TODO: find a smoother interpolation
        RotationDegrees = _rotation;
    }
    
    private void Fire()
    {
        if (_aiming)
        {
            _currentRotation -= new Vector3(-recoilRotationAiming.x, (float)GD.RandRange(-recoilRotationAiming.y, recoilRotationAiming.y), (float)GD.RandRange(-recoilRotationAiming.z, recoilRotationAiming.z));
        }
        else 
        {
            _currentRotation -= new Vector3(-recoilRotationAiming.x, (float)GD.RandRange(-recoilRotation.y, recoilRotation.y), (float)GD.RandRange(-recoilRotation.z, recoilRotation.z));
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
