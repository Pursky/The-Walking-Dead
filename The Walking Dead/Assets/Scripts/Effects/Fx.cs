using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fx : MonoBehaviour
{
    public float MinCutoff;
    public float MaxCutoff;
    public float TransitionTime;

    private AudioLowPassFilter filter;
    [SerializeField]
    private bool filterActive;

    void Start()
    {
        filter = GetComponent<AudioLowPassFilter>();
        filter.cutoffFrequency = MinCutoff;
        filterActive = true;
    }

    void Update()
    {
        float value = (MaxCutoff - MinCutoff) / TransitionTime;

        if (filterActive)
        {
            filter.cutoffFrequency = filter.cutoffFrequency > MinCutoff + 1 ? filter.cutoffFrequency - Time.deltaTime * value : MinCutoff;
        }
        else
        {
            filter.cutoffFrequency = filter.cutoffFrequency < MaxCutoff - 1 ? filter.cutoffFrequency + Time.deltaTime * value : MaxCutoff;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 11) filterActive = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 11) filterActive = true;
    }
}