using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Vector3[] levelPoints = new Vector3[5];
    public string[] LocName = new string[5];
    private void Start()
    {
        CoordsMassive();
        LocNamesMassive();
    }

    public void LocNamesMassive()
    {
        LocName[0] = "k";
        LocName[1] = "l";
        LocName[2] = "b";
        LocName[3] = "h";
        LocName[4] = "";
    }

    public void CoordsMassive()
    {
        levelPoints[0] = new Vector3(1f, 2f, 3f);
        levelPoints[1] = new Vector3(4f, 5f, 6f);
        levelPoints[2] = new Vector3(7f, 8f, 9f);
        levelPoints[3] = new Vector3(10f, 11f, 12f);
        levelPoints[4] = new Vector3(13f, 14f, 15f);
    }
}
