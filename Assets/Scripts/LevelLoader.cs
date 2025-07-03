using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public Teleport Teleport;
    [Header("Loc Teleport Settings")]
    public int delay;
    public string loc;

    [Header("Teleport Settings")]
    public Vector3 coords;

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "MainPlayer")
        {
            Teleport.tp(delay, loc, coords);
        }
    }
}
