using System.Numerics;
using UnityEngine;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using JetBrains.Annotations;
using Unity.VisualScripting;
using System.Collections.Generic;

public class FractalsSpawner : MonoBehaviour
{
    public GameObject Fractal1;
    public GameObject Fractal2;
    public GameObject Fractal3;
    public int ExpactedQuantity;
    public static int CloneQuantity;
    List<GameObject> FractalsList = new List<GameObject>();

    void Start()
    {

        FractalsList.Add(Fractal1);
        FractalsList.Add(Fractal2);
        FractalsList.Add(Fractal3);
    }
    public void CloneSpawner()
    {


        System.Random rnd = new System.Random();
        int FractalId = rnd.Next(0, 3);
        GameObject FractalClone = Instantiate(FractalsList[FractalId]); 

        FractalClone.AddComponent<CloneManagment>();
        CloneQuantity += 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (CloneQuantity < ExpactedQuantity)
        {
            CloneSpawner();
        }
    }

    

}

