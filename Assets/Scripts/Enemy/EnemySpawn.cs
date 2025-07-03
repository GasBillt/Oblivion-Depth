using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public Vector3 Spawn;          // Точка телепортации
    public Vector3 move;           // Направление и скорость движения
    public bool spawning = false;  // Флаг для активации телепортации
    
    private int takeLayer;         // Кэшированный слой Take

    void Start()
    {
        // Кэшируем слой для оптимизации
        takeLayer = LayerMask.NameToLayer("Take");
    }

    void Update()
    {
        // Постоянное перемещение по вектору move
        transform.Translate(move * Time.deltaTime);
        
        // Телепортация при установке флага spawning
        if (spawning)
        {
            transform.position = Spawn;
            spawning = false;  // Сразу сбрасываем флаг
        }
    }

    // Обработка триггеров
    void OnTriggerEnter(Collider other)
    {
        // Начинаем поиск с текущего объекта коллизии
        Transform current = other.transform;
        GameObject objectToDestroy = null;

        // Поднимаемся вверх по иерархии, пока не найдем объект с нужным слоем
        while (current != null)
        {
            if (current.gameObject.layer == takeLayer)
            {
                objectToDestroy = current.gameObject;
                break;
            }
            current = current.parent;
        }

        // Если нашли объект со слоем Take, уничтожаем его
        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
        }
    }
}