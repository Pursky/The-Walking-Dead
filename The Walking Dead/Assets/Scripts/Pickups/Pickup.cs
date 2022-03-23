using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    private AudioSource audioSource;
    protected bool deleting;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    protected abstract void HandlePickup();

    private void OnTriggerStay(Collider other)
    {
        if (Player.Instance.Dead || deleting) return;
        if (other.gameObject.layer == 11) HandlePickup();
    }

    protected void StartDeleting()
    {
        deleting = true;
        audioSource.time = 0.2f;
        audioSource.Play();
        Destroy(transform.GetChild(0).gameObject);
        Invoke("Delete", 1);
    }

    private void Delete()
    {
        Destroy(gameObject);
    }
}