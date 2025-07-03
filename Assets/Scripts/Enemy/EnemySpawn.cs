using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public Vector3 Spawn;          // Точка телепортации
    public Vector3 move;           // Направление и скорость движения
    public bool spawning = false;  // Флаг для активации телепортации
    private int takeLayer;

    void Start()
    {
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

    // Обработка столкновений
    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        Transform current = other.transform;
        while (current != null)
        {
            if (current.gameObject != null && current.gameObject.layer == takeLayer)
            {
                Destroy(current.gameObject);
                return;
            }
            current = current.parent;
        }
    }
}