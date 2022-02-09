using Godot;
using Godot.Collections;
using System.Threading.Tasks;

public class WeaponSelect : Position3D
{
    [Signal] public delegate void ChildrenFound(Array<Spatial> children);
    [Signal] public delegate void WeaponChanged(byte index);

    [Export] private Array<Spatial> weapons = new Array<Spatial>();
    [Export] private byte weaponIndex;

    private bool _canScroll = true;

    public Array<Spatial> Weapons => weapons;
    public byte WeaponIndex => weaponIndex;

    public override void _Ready()
    {
        FindWeapons();
        SetActiveWeapon(weaponIndex);
    }

    private void FindWeapons()
    {
        var children = GetChildren();

        foreach (var child in children)
        {
            if (child is Weapon)
            {
                weapons.Add((Spatial)child);
            }
        }
        EmitSignal(nameof(ChildrenFound), weapons);
    }

    private void SetActiveWeapon(byte index)
    {
        Parallel.ForEach(weapons, i => i.Visible = false);

        weapons[index].Visible = true;
        EmitSignal(nameof(WeaponChanged), index);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && !(weapons[WeaponIndex] as Weapon).IsShooting)
        {
            switch (mouseButton.ButtonIndex)
            {
                case (int)ButtonList.WheelUp when _canScroll:
                    if (weaponIndex > 0)
                    {
                        SetActiveWeapon(--weaponIndex);
                    }
                    else
                    {
                        weaponIndex = (byte)weapons.Count;
                        SetActiveWeapon(--weaponIndex);
                    }
                    Wait();
                    break;
                
                case (int)ButtonList.WheelDown when _canScroll:
                    if (weaponIndex < weapons.Count - 1)
                    {
                        SetActiveWeapon(++weaponIndex);
                    }
                    else 
                    {
                        weaponIndex = 0;
                        SetActiveWeapon(weaponIndex);
                    }
                    Wait();
                    break;
            }
        }

        if (@event is InputEventKey eventKey && !(weapons[WeaponIndex] as Weapon).IsShooting)
        {
            switch (eventKey.PhysicalScancode)
            {
                case (uint)KeyList.Key1:
                    weaponIndex = 0;
                    SetActiveWeapon(weaponIndex);
                    break;

                case (uint)KeyList.Key2:
                    weaponIndex = 1;
                    SetActiveWeapon(weaponIndex);
                    break;

                case (uint)KeyList.Key3:
                    weaponIndex = 2;
                    SetActiveWeapon(weaponIndex);
                    break;
            }
        }
    }
    private async void Wait()
    {
        _canScroll = false;
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        _canScroll = true;
    }
}
