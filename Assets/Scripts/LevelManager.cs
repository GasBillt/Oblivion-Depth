using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public Vector3[] levelPoints = new Vector3[5];
    private void Start()
    {
        //CoordsMassive();
    }
    //public void CoordsMassive()
    //{
    //    points[0, 0] = new Vector3(1f, 2f, 3f);
    //    points[0, 1] = new Vector3(4f, 5f, 6f);
    //    points[1, 0] = new Vector3(7f, 8f, 9f);
    //    points[1, 1] = new Vector3(10f, 11f, 12f);
    //}
}
