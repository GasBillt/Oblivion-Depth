using UnityEngine;
using System;
using Unity.Mathematics;
using JetBrains.Annotations;
using UnityEditor.Callbacks;
using NUnit.Framework;
using Unity.VisualScripting.FullSerializer;
using System.Collections;
public class CloneManagment : MonoBehaviour
{

    public IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
        FractalsSpawner.CloneQuantity -= 1;
    }
    System.Random rnd = new System.Random();
    // Position
    void RandomPosition()
    {
        int PositionZ;
        int PositionX = rnd.Next(-300, 300);
        int PositionY = rnd.Next(-50, 400);
        int RandZ = rnd.Next(0, 1);
        if (Math.Sqrt(math.square(PositionX) + math.square(PositionY)) < 50)
        {
            if (RandZ == 1) PositionZ = rnd.Next(200, 300);
            else PositionZ = rnd.Next(-300, -200);
        }
        else
        {
            PositionZ = rnd.Next(-300, 300);
        }
        transform.position = new UnityEngine.Vector3(PositionX, PositionY, PositionZ);
    }

    // Movement
    float SpeedX;
    float SpeedY;
    float SpeedZ;
    void Start()
    {
        // GameObject Spawner = GameObject.Find("Fractals");
        

        // Position
        RandomPosition();

        // Movement
        SpeedX = (float)(rnd.Next(-1, 1) * 0.1);
        SpeedY = (float)(rnd.Next(-1, 1) * 0.1);
        SpeedZ = (float)(rnd.Next(-1, 1) * 0.1);

        // Destruction
        StartCoroutine(DelayedDestroy(
            rnd.Next(5, 15)
            ));
    }


    void FixedUpdate()
    {
        // Movement
        transform.position += new Vector3(SpeedX, SpeedY, SpeedZ);
    }
};