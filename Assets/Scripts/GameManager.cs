using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    IEnumerator ManualDelay(float seconds)
    {
        float startTime = Time.time;
        while ((Time.time - startTime) < seconds)
        {
            yield return null;
        }
    }
}
