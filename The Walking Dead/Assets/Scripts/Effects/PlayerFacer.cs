using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFacer : MonoBehaviour
{
    void Update()
    {
        transform.forward = Player.Instance.transform.position - transform.position;
        transform.eulerAngles = new Vector3(90, transform.eulerAngles.y, 0);
    }
}