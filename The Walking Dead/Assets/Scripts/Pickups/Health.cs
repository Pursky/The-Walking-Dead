using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Pickup
{
    public float HealAmount;

    protected override void HandlePickup()
    {
        if (Player.Instance.Health == 10) return;

        Player.Instance.Health = Mathf.Clamp(Player.Instance.Health + HealAmount, 0, 10);
        UI.Instance.UpdateHealthBar();

        StartDeleting();
    }
}