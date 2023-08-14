using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField] private LineController lr;

    private void Start()
    {
        lr.SetUpLine(points);
    }
}
