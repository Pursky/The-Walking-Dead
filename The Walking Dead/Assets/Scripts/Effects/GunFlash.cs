using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFlash : MonoBehaviour
{
    private float oIntensity;
    new private Light light;

    void Start()
    {
        light = GetComponent<Light>();
        oIntensity = light.intensity;
    }

    void Update()
    {
        light.intensity -= oIntensity * Time.deltaTime * 15;
        if (light.intensity <= 0) Destroy(gameObject);
    }
}