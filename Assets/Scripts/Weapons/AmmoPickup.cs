using Godot;

public class AmmoPickup : Area
{
    [Export] private AmmoType ammoType { get; set; }
    [Export] private ushort reloadAmount { get; set; } = 15;

    public override void _Ready()
    {
        Connect("body_entered", this, nameof(OnAreaBodyEntered));
    }

    private void OnAreaBodyEntered(Node body)
    {
        var target = body.GetNodeOrNull<Ammo>("Head/Weapons/Ammo");

        if (target != null)
        {
            target.RiseAmmo(ammoType, reloadAmount);
            QueueFree();
        }
    }
}
