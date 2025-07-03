using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int LevelsCount = 1;
    public string currentLevel;
    public Vector3[] LevelPoints;
    public GameObject[] Locations;
    public string[] LocName;

    void Awake()
    {
        // Изменяем размеры массивов в соответствии с LevelsCount
        ResizeArray(ref LevelPoints, LevelsCount);
        ResizeArray(ref Locations, LevelsCount);
        ResizeArray(ref LocName, LevelsCount);
    }

    void Start()
    {
        // Заполняем массив LocName именами объектов из Locations
        for (int i = 0; i < LevelsCount; i++)
        {
            if (Locations[i] != null)
            {
                LocName[i] = Locations[i].name;
            }
            else
            {
                LocName[i] = "Empty";
            }
        }
    }

    // Вспомогательный метод для изменения размера массива с сохранением данных
    private void ResizeArray<T>(ref T[] array, int newSize)
    {
        if (array == null)
        {
            array = new T[newSize];
            return;
        }

        if (newSize == array.Length) return;

        T[] newArray = new T[newSize];
        int elementsToCopy = Mathf.Min(array.Length, newSize);
        for (int i = 0; i < elementsToCopy; i++)
        {
            newArray[i] = array[i];
        }
        array = newArray;
    }

    public void LevelLoad(string loc)
    {
        if (loc == null) return;
        
        // Деактивируем все уровни
        for (int i = 0; i < Locations.Length; i++)
        {
            if (Locations[i] != null)
            {
                Locations[i].SetActive(false);
            }
        }

        // Активируем только указанный уровень
        bool levelFound = false;
        for (int i = 0; i < LocName.Length; i++)
        {
            if (LocName[i] == loc)
            {
                if (Locations[i] != null)
                {
                    Locations[i].SetActive(true);
                    currentLevel = loc; // Обновляем текущий уровень
                    levelFound = true;
                    Debug.Log($"Level activated: {loc}");
                }
                break; // Выходим после активации первого совпадения
            }
        }

        if (!levelFound)
        {
            Debug.LogWarning($"Level '{loc}' not found in LocName array!");
        }
    }
}