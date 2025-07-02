using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyApproachingVFX : MonoBehaviour
{

    GameObject Player;
    public float DistanceToStart = 30;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.Find("MainPlayer");
        
    }


    // Update is called once per frame
    void Update()
    {

        // Player 
        float PlayerPosX = Player.transform.position.x;
        float PlayerPosY = Player.transform.position.y;
        float PlayerPosZ = Player.transform.position.z;
        // Object
        float PosX = transform.position.x;
        float PosY = transform.position.y;
        float PosZ = transform.position.z;
        if (math.sqrt(math.square(PlayerPosX - PosX) + math.square(PlayerPosY - PosY) + math.square(PlayerPosZ - PosZ)) < DistanceToStart)
        {
            GameObject.Find("Enemy Approaching VFX").GetComponent<Volume>().weight = 1 - (math.sqrt(math.square(PlayerPosX - PosX) + math.square(PlayerPosY - PosY) + math.square(PlayerPosZ - PosZ)) / DistanceToStart);
        }
        else GameObject.Find("Enemy Approaching VFX").GetComponent<Volume>().weight = 0;
    }
}
