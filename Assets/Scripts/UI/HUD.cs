using Godot;

public class HUD : CanvasLayer
{
    [Export] private NodePath weapons;

    private WeaponSelect _weapons;
    private Control _crosshair;
    private Label _bulletsLabel;
    private Label _ammoLabel;
    private Label _nameLabel;

    public override void _Ready()
    {
        _weapons = GetNode<WeaponSelect>(weapons);
        _crosshair = GetNode<Control>("Crosshair");
        _bulletsLabel = GetNode<Label>("UI/Bullets");
        _ammoLabel = GetNode<Label>("UI/Ammo");
        _nameLabel = GetNode<Label>("UI/Name");

        _weapons.Connect("WeaponChanged", this, nameof(OnWeaponSelectWeaponChanged));
        _weapons.GetNode<Ammo>("Ammo").Connect("AmmoIncreased", this, nameof(OnAmmoIncreased));
        _weapons.GetNode<WeaponZoom>("../Camera/Zoom").Connect("AimingChanged", this, nameof(OnAimingChanged));

        SuscribeToWeaponSignals();

        _bulletsLabel.Text = (_weapons.Weapons[_weapons.WeaponIndex] as Weapon).CurrentBullets.ToString();
        _nameLabel.Text = (_weapons.Weapons[_weapons.WeaponIndex] as Weapon).WeaponName;
        _ammoLabel.Text = (_weapons.Weapons[_weapons.WeaponIndex] as Weapon).CurrentAmmo.ToString();
    }

    private void SuscribeToWeaponSignals()
    {
        foreach (Weapon weapon in _weapons.Weapons)
        {
            weapon.Connect("DoShoot", this, nameof(UpdateWeaponUIWhenShoot));
            weapon.Connect("Recharged", this, nameof(OnWheaponRecharged));
        }
    }
    private void OnAimingChanged(bool aim)
    {
        AimType aT = (_weapons.Weapons[_weapons.WeaponIndex] as Weapon).AimType;

        if (aim && aT == AimType.Scope)
        {
            _crosshair.Visible = false;
            return;
        }

        if (!_crosshair.Visible)
        {
            _crosshair.Visible = true;
        }
    }

    private void OnWeaponSelectWeaponChanged(byte index)
    {
        var weapon = _weapons.Weapons[index] as Weapon;
        _bulletsLabel.Text = weapon.CurrentBullets.ToString();
        _nameLabel.Text = weapon.WeaponName;
        _ammoLabel.Text = weapon.CurrentAmmo.ToString();
        UpdateBulletLabelColor();
    }
    private void OnWheaponRecharged()
    {
        var weapon = _weapons.Weapons[_weapons.WeaponIndex] as Weapon;
        _bulletsLabel.Text = weapon.CurrentBullets.ToString();
        _ammoLabel.Text = weapon.CurrentAmmo.ToString();
        UpdateBulletLabelColor();
    }

    private void UpdateWeaponUIWhenShoot()
    {
        _bulletsLabel.Text = (_weapons.Weapons[_weapons.WeaponIndex] as Weapon).CurrentBullets.ToString();
        UpdateBulletLabelColor();
    }

    private void OnAmmoIncreased()
    {
        _ammoLabel.Text = (_weapons.Weapons[_weapons.WeaponIndex] as Weapon).CurrentAmmo.ToString();
    }

    private void UpdateBulletLabelColor()
    {
        var weapon = _weapons.Weapons[_weapons.WeaponIndex] as Weapon;
        var bullets = weapon.CurrentBullets;
        var maxBullets = weapon.SlotCapacity;

        //maxBullets is to a hundred
        //bullets is to ?

        float factor = (bullets * 100) / maxBullets;

        if (factor <= 25)
        {
            _bulletsLabel.AddColorOverride("font_color", Color.Color8(255, 0, 0));
        }
        else if (factor <= 50)
        {
            _bulletsLabel.AddColorOverride("font_color", Color.Color8(221, 118, 27));
        }
        else
        {
            _bulletsLabel.AddColorOverride("font_color", Color.Color8(255, 255, 255));
        }
    }
}
