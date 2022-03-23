using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : Pickup
{
    public int BulletCount;

    protected override void HandlePickup()
    {
        Gun.Instance.UnloadedAmmo += BulletCount;
        UI.Instance.UpdateAmmoBox();

        StartDeleting();
    }
}