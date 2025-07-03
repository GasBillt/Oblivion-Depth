using UnityEngine;

public class EnemyActivate : MonoBehaviour
{
    public GameObject enemyToActivate;  // Ссылка на врага, которого нужно активировать

    void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в триггер вошел объект с тегом MainPlayer
        if (other.CompareTag("MainPlayer"))
        {
            // Активируем врага
            if (enemyToActivate != null)
            {
                enemyToActivate.SetActive(true);
                
                // Опционально: отключаем триггер, чтобы он сработал только один раз
                // GetComponent<Collider>().enabled = false;
            }
            else
            {
                Debug.LogWarning("Enemy reference is not set in EnemyActivate script!");
            }
        }
    }
}