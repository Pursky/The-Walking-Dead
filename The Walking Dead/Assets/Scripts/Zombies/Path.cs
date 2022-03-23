using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public Vector3[] Points;

    void Start()
    {
        Points = new Vector3[transform.childCount];
        for (int i = 0; i < transform.childCount; i++) Points[i] = transform.GetChild(i).position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3[] points = new Vector3[transform.childCount];
        for (int i = 0; i < transform.childCount; i++) points[i] = transform.GetChild(i).position;

        for (int i = 0; i < points.Length; i++)
        {
            if (i < points.Length - 1) Gizmos.DrawLine(points[i], points[i + 1]);
            else Gizmos.DrawLine(points[i], points[0]);
        }
    }
}
