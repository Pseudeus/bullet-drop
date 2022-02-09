using Godot;

public class WeaponZoom : Node
{
    [Signal] public delegate void AimingChanged(bool aimState);

    #region AimSettings
    [Export(PropertyHint.Range, "1.0, 6.0, 0.5")] private float multiplier = 1.5f;
    [Export] private float zoomOutSensibility;
    [Export] private float zoomInSensibility = 0.2f;
    [Export(PropertyHint.Range, "0.0, 1.0, 0.05")] private float smoothAiming = 0.5f;
    #endregion

    private Camera _camera;
    private FirstPersonController _fpsController;
    private byte _baseFieldOfView;
    private float _aimFieldOfView;
    private WeaponSelect _weapons;
    private Vector3 _hipFirePosition;
    private float _t;
    private bool _aiming;

    public override void _Ready()
    {
        _camera = GetParent<Camera>();
        _fpsController = GetNode<FirstPersonController>("../../../");
        _weapons = GetNode<WeaponSelect>("../../Weapons");

        zoomOutSensibility = _fpsController.mouseSensitivity;
        _hipFirePosition = _weapons.Translation;
        _baseFieldOfView = (byte)_camera.Fov;
        _aimFieldOfView = _baseFieldOfView / multiplier;
    }

    private void Aiming(bool aim)
    {
        if (aim)
        {
            _fpsController.mouseSensitivity = zoomInSensibility;
            EmitSignal(nameof(AimingChanged), aim);
        }
        else
        {
            _fpsController.mouseSensitivity = zoomOutSensibility;
            EmitSignal(nameof(AimingChanged), aim);
        }
    }

    private void ZoomIn(float delta)
    {   
        _camera.Fov = Mathf.Lerp(_baseFieldOfView, _aimFieldOfView, _t);
        _t += 3.1f * delta;
    }
    private void ZoomOut(float delta)
    {
        _camera.Fov = Mathf.Lerp(_baseFieldOfView, _aimFieldOfView, _t);
        _t -= 3.1f * delta;
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("aim"))
        {
            _aiming = true;
            Aiming(_aiming);
        }
        else if (Input.IsActionJustReleased("aim"))
        {
            _aiming = false;
            Aiming(_aiming);
        }

        if (_t < 1 && _aiming && _camera.Fov > _aimFieldOfView)
        {
            ZoomIn(delta);
        }
        else if (_t > float.Epsilon && !_aiming && _camera.Fov < _baseFieldOfView)
        {
            ZoomOut(delta);
        }
    }
}
