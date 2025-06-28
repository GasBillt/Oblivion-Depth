using UnityEngine;
using System;
using JetBrains.Annotations;

public class CloneMovement : MonoBehaviour
{
    System.Random rnd = new System.Random();
    float SpeedX;
    float SpeedY;
    float SpeedZ;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpeedX = (float)(rnd.Next(-1, 1) * 0.1);
        SpeedY = (float)(rnd.Next(-1, 1) * 0.1);
        SpeedZ = (float)(rnd.Next(-1, 1) * 0.1);
    }
    void FixedUpdate()
    {
        transform.position += new Vector3(SpeedX, SpeedY, SpeedZ);
    }
}
