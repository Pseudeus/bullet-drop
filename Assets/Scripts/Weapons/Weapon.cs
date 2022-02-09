using Godot;
using Godot.Collections;

public class Weapon : Spatial
{
    [Signal] public delegate void DoShoot();
    [Signal] public delegate void Recharged();

    #region WeaponCapabilities
    [Export] private string weaponName { get; set; }
    [Export] private AmmoType ammoType { get; set; }
    [Export] private AimType aimType { get; set; }
    [Export] private ushort WeaponSlotCapacity { get; set; } = 30;
    [Export] private float Range { get; set; } = 100f;
    [Export] private byte Damage { get; set; } = 15;
    [Export(PropertyHint.Range, "0.0, 1.0, 0.05")] private float Acuracy { get; set; } = 0.5f;
    [Export] private PackedScene hitEffect;
    #endregion

    public ushort SlotCapacity => WeaponSlotCapacity;
    public ushort CurrentBullets => _weaponCurrentBullets;
    public ushort CurrentAmmo => _ammo.GetAmmoAmount(ammoType);
    public string WeaponName => weaponName;
    public bool IsShooting => _shooting;
    public AimType AimType => aimType;

    private Ammo _ammo;
    private bool _canShoot = true;
    private bool _aiming;
    private bool _shooting;
    private Camera _fpCamera;
    private FirstPersonController _fpController;
    private Timer _shootCadence;
    private AudioStreamPlayer3D _shotSoundEffect;
    private ExplotionHandler _muzzleFlash;
    private ushort _weaponCurrentBullets;
    private float _xMaxAcuracyOffset = 30f;
    private float _yMaxAcuracyOffset = 30f;
    private Vector3 _rayOrigin;
    private DamageInfo _dmgInfo = new DamageInfo();

    public override void _Ready()
    {
        _shotSoundEffect = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
        _shootCadence = GetNode<Timer>("Cadence");
        _muzzleFlash = GetNode<ExplotionHandler>("MuzzleFlash");
        _fpCamera = GetViewport().GetCamera();
        _fpController = GetNode<FirstPersonController>("../../../");
        _ammo = GetNode<Ammo>("../Ammo");

        Connect("DoShoot", GetChild<WeaponRecoil>(0), "OnDoShoot");  
        GetNode<WeaponZoom>("../../Camera/Zoom").Connect("AimingChanged", this, nameof(OnWeaponZoomAimingChanged));

        _weaponCurrentBullets = WeaponSlotCapacity;

        _ammo.AddSlot(ammoType, 100);
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionPressed("fire") && _canShoot && Visible)
        {
            Shoot();
            if (!_shooting) _shooting = true;
        }

        if (Input.IsActionJustReleased("fire") && _shooting) _shooting = false;

        /*if (Input.IsActionJustPressed("reload") && _weaponCurrentBullets < WeaponSlotCapacity) 
        { 
            _canShoot = false;
            VerifyAmmo();
        }*/
    }

    private void OnWeaponZoomAimingChanged(bool aim)
    {
        _aiming = aim;
    }

    private void RayCasting()
    {
        switch (ammoType)
        {
            case AmmoType.Bullet:
                NormalShot();
                break;
            case AmmoType.Shell:
                ShotgunShot();
                break;
            default:
                NormalShot();
                break; 
        }
    }

    private void NormalShot()
    {
        Dictionary hit;
        DoRaycast(out hit);

        if (hit.Count > 0)
        {
            _dmgInfo.Amount = Damage;
            switch (hit["collider"])
            {
                case IKilleable collider:
                    collider.Stats.RecieveDamage(_dmgInfo);
                    break;
                    
                case IDestroyable collider:
                    collider.RecieveDamage(Damage);
                    break;
            }
            
            var blood = hitEffect.Instance<Particles>();

            //Vector3 incomingVec = (Vector3)hit["position"] - _rayOrigin;
            //Vector3 reflecVec = incomingVec.Reflect((Vector3)hit["normal"]);

            blood.Translation = (Vector3)hit["position"];
            GetTree().CurrentScene.AddChild(blood);
        }
    } 
    
    private void ShotgunShot()
    {
        Dictionary[] hits = new Dictionary[5];

        for (byte i = 0; i < hits.Length; i++)
        {
            Vector3 reflect = Vector3.Zero;
            DoRaycast(out hits[i]);

            if (hits[i].Count > 0)
            {
                _dmgInfo.Amount = Damage;
                switch (hits[i]["collider"])
                {
                    case IKilleable collider:
                        collider.Stats.RecieveDamage(_dmgInfo);
                        break;
                    
                    case IDestroyable collider:
                        collider.RecieveDamage(Damage);
                        break;
                }

                var blood = hitEffect.Instance<Particles>();
                
                blood.Translation = (Vector3)hits[i]["position"];// - (Vector3)hit["normal"];
                GetTree().CurrentScene.AddChild(blood);
            }
        }
    }

    private void DoRaycast(out Dictionary hit)
    {
        Vector2 center = GetViewport().GetMousePosition();
        var rayOrigin = _fpCamera.ProjectRayOrigin(center);
        var rayTarget = rayOrigin + _fpCamera.ProjectRayNormal(center + CalculateAcuracyOffset()) * Range;

        var spaceState = GetWorld().DirectSpaceState;
        hit = spaceState.IntersectRay(rayOrigin, rayTarget);
        _rayOrigin = rayOrigin;
    }

    private Vector2 CalculateAcuracyOffset()
    {
        float acc = 1 - Acuracy;
        if (acc <= float.Epsilon) return Vector2.Zero;

        Vector2 offset = Vector2.Zero;
        offset = Vector2.Right * (float)GD.RandRange(-_xMaxAcuracyOffset, _xMaxAcuracyOffset) + Vector2.Up * (float)GD.RandRange(-_yMaxAcuracyOffset, _yMaxAcuracyOffset);

        if (ammoType != AmmoType.Shell)
        {
            if (_aiming) offset /= 2;

            switch (_fpController.State)
            {
                case MotionState.Walking:
                    offset *= 2;
                    break;
                case MotionState.Sprinting:
                    offset *= 4;
                    break;
                case MotionState.Jumping:
                    offset *= 4.5f;
                    break;
            }
        }
        return offset * acc;
    }

    private async void Shoot()
    {
        _canShoot = false;
        _shootCadence.Start();

        _weaponCurrentBullets--;
        EmitSignal(nameof(DoShoot));
        _muzzleFlash.EmitParticles();

        CallDeferred(nameof(RayCasting));

        float r = GD.Randf() * 0.1f;
        _shotSoundEffect.PitchScale = 0.95f + r;
        _shotSoundEffect.Play();

        await ToSignal(_shootCadence, "timeout");
        _canShoot = _weaponCurrentBullets > 0
                  ? true
                  : false;

        VerifyAmmo();
    }

    private void VerifyAmmo()
    {
        if (_ammo.IsAmmoAvailable(ammoType) && !_canShoot)
        {
            Recharge();
        }
    }

    private async void Recharge()
    {
        _canShoot = false;
        _shooting = true;

        await ToSignal(GetTree().CreateTimer(2), "timeout");

        var amount = _ammo.GetAmmoAmount(ammoType);
        var bulletsLeft = (ushort)(WeaponSlotCapacity - _weaponCurrentBullets);

        if (amount < bulletsLeft)
        {
            _ammo.SetAmountToCero(ammoType);
            _weaponCurrentBullets += amount;
        }
        else
        {
            _ammo.ReduceAmmo(ammoType, bulletsLeft);
            _weaponCurrentBullets += bulletsLeft;
        }

        _shooting = false;
        _canShoot = true;
        EmitSignal(nameof(Recharged));
    }
}

public enum AmmoType
{
    Bullet,
    Shell,
    Rocket
}
public enum AimType
{
    Traditional,
    Scope
}
