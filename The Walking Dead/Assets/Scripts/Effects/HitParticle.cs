using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitParticle : MonoBehaviour
{
    public float DestroyTime;

    void Start()
    {
        Invoke("Destroy", DestroyTime);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}