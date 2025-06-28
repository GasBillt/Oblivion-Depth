using UnityEngine;
using System;
using Unity.Mathematics;

public class ClonePosition : MonoBehaviour
{
    public void RandomPosition()
    {
        System.Random rnd = new System.Random();
        int PositionZ;
        int PositionX = rnd.Next(-150, 150);
        int PositionY = rnd.Next(100, -10);
        if (Math.Sqrt(math.square(PositionX) + math.square(PositionY)) < 50)
        {
            PositionZ = rnd.Next(100, 150);
        }
        else
        {
            PositionZ = rnd.Next(-150, -100);
        }
        transform.position = new UnityEngine.Vector3(PositionX, PositionY, PositionZ);
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
