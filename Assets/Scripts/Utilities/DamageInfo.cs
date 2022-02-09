using Godot;

public class DamageInfo
{
    public ushort Amount { get; set; }
    public Vector3? Origin { get; set; }

    public DamageInfo()
    {
        Amount = 0;
        Origin = null;
    }

    public DamageInfo(ushort amount, Vector3? origin = null)
    {
        Amount = amount;
        Origin = origin;
    }
}
