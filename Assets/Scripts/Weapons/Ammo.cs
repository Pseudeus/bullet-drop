using Godot;
using System.Collections.Generic;

public class Ammo : Node
{
    [Signal] public delegate void AmmoIncreased();

    private List<AmmoSlot> ammoSlots = new List<AmmoSlot>();

    private class AmmoSlot
    {
        public AmmoType ammoType;
        public ushort ammoAmount;
    }

    private bool Contains(AmmoType ammoType, out AmmoSlot slotIn)
    {
        foreach (AmmoSlot slot in ammoSlots)
        {
            if (slot.ammoType == ammoType)
            {
                slotIn = slot;
                return true;
            }
        }
        slotIn = null;
        return false;
    }

    public void AddSlot(AmmoType ofType, ushort amount = 0)
    {
        if (!Contains(ofType, out AmmoSlot slot))
        {
            ammoSlots.Add(new AmmoSlot() { ammoType = ofType, ammoAmount = amount });
        }
    }
    public void RemoveSlot(AmmoType ofType)
    {
        if (Contains(ofType, out AmmoSlot slot))
        {
            ammoSlots.Remove(slot);
        }
    }

    public bool IsAmmoAvailable(AmmoType ammoType)
    {
        if (GetAmmoSlot(ammoType).ammoAmount > 0) return true;
        return false;
    }

    public void ReduceAmmo(AmmoType ammoType, ushort reduceAmount = 1)
    {
        GetAmmoSlot(ammoType).ammoAmount -= reduceAmount;
    }

    public void RiseAmmo(AmmoType ammoType, ushort ammoReloadAmount)
    {
        GetAmmoSlot(ammoType).ammoAmount += ammoReloadAmount;
        EmitSignal(nameof(AmmoIncreased));
    }

    public ushort GetAmmoAmount(AmmoType ammoType)
    {
        return GetAmmoSlot(ammoType).ammoAmount;
    }

    public void SetAmountToCero(AmmoType ammoType)
    {
        GetAmmoSlot(ammoType).ammoAmount = 0;
    }

    private AmmoSlot GetAmmoSlot(AmmoType ammoType)
    {
        foreach (AmmoSlot slot in ammoSlots)
        {
            if (slot.ammoType == ammoType) return slot;
        }
        return null;
    }
}
