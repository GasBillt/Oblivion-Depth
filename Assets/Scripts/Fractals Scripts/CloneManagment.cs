using UnityEngine;
using System;
using Unity.Mathematics;
using JetBrains.Annotations;
using UnityEditor.Callbacks;
using NUnit.Framework;
using Unity.VisualScripting.FullSerializer;
using System.Collections;
using UnityEngine.Animations;
using UnityEngine.UIElements;
public class CloneManagment : MonoBehaviour
{
    Animator Anim;
    

    public IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Anim.SetTrigger("ScaleOut");
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
        FractalsSpawner.CloneQuantity -= 1;
    }
    System.Random rnd = new System.Random();
    // Position
    void RandomPosition()
    {
        float PositionZ;
        float PositionX = GameObject.Find("Player").transform.position.x + rnd.Next(-300, 300);
        float PositionY = GameObject.Find("Player").transform.position.y + rnd.Next(-50, 400);
        float RandZ = rnd.Next(0, 1);
        if (Math.Sqrt(math.square(PositionX) + math.square(PositionY)) < 100)
        {
            if (RandZ == 1) PositionZ = GameObject.Find("Player").transform.position.z + rnd.Next(200, 300);
            else PositionZ = GameObject.Find("Player").transform.position.z + rnd.Next(-300, -200);
        }
        else
        {
            PositionZ = GameObject.Find("Player").transform.position.z + rnd.Next(-300, 300);
        }
        transform.position = new UnityEngine.Vector3(PositionX, PositionY, PositionZ);
    }

    // Movement
    float SpeedX;
    float SpeedY;
    float SpeedZ;
    void Start()
    {
        // Animator
        Anim = GetComponent<Animator>();
        // Anim.

        // Position
        RandomPosition();

        // Movement
        SpeedX = (float)(rnd.Next(-1, 1) * 0.1);
        SpeedY = (float)(rnd.Next(-1, 1) * 0.1);
        SpeedZ = (float)(rnd.Next(-1, 1) * 0.1);

        // Destruction
        StartCoroutine(DelayedDestroy(
            rnd.Next(5, 13)
            ));
    }


    void FixedUpdate()
    {
        // Movement
        transform.position += new Vector3(SpeedX, SpeedY, SpeedZ);
    }
};