using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public Teleport Teleport;

    public void OnCollisionEnter(Collision collision)
    {
        if (gameObject.tag == "MainPlayer")
        {
            //Teleport.tp();
        }
    }
}
