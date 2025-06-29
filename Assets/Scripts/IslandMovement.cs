using UnityEngine;
using System.Collections.Generic;

public class IslandMovement : MonoBehaviour
{
    [Header("Oscillation Settings")]
    public float amplitude = 0.5f;
    public float frequency = 0.5f;

    [Header("Sink Settings")]
    public float sinkDistance = 1f;
    public float sinkSpeed = 2f;
    public float sinkHoldTime = 0.3f;

    [Header("Collider Settings")]
    public float platformTopOffset = 0.5f; // Отступ для проверки верхней границы
    public float playerBottomOffset = 0.5f; // Отступ для проверки нижней границы игрока

    private Vector3 startPosition;
    private bool isSinking = false;
    private bool hasSunkForCurrentPlayer = false;
    private float sinkTimer;
    private float sinkProgress;
    private Vector3 sinkStartPosition;
    private Vector3 sinkTargetPosition;
    private Transform currentPlayer;
    private List<Collider> platformColliders = new List<Collider>();

    void Start()
    {
        startPosition = transform.position;
        CachePlatformColliders();
    }

    // Кэшируем все коллайдеры платформы и ее дочерних объектов
    private void CachePlatformColliders()
    {
        platformColliders.Clear();
        
        // Получаем все коллайдеры у текущего объекта и его детей
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            // Игнорируем триггеры и коллайдеры игрока
            if (!col.isTrigger && !col.CompareTag("Player") && !col.CompareTag("Legs"))
            {
                platformColliders.Add(col);
            }
        }

        // Если коллайдеров нет - создаем простой BoxCollider
        if (platformColliders.Count == 0)
        {
            Debug.LogWarning("No colliders found on platform! Adding a BoxCollider.");
            BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
            platformColliders.Add(newCollider);
        }
    }

    void Update()
    {
        if (!isSinking)
        {
            // Плавное движение вверх-вниз
            float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
        else
        {
            HandleSinking();
        }
    }

    private void HandleSinking()
    {
        sinkTimer += Time.deltaTime;

        // Плавное опускание
        if (sinkTimer < sinkProgress)
        {
            float t = sinkTimer / sinkProgress;
            transform.position = Vector3.Lerp(sinkStartPosition, sinkTargetPosition, t);
        }
        // Удерживаем внизу
        else if (sinkTimer < sinkProgress + sinkHoldTime)
        {
            transform.position = sinkTargetPosition;
        }
        // Плавное поднятие
        else if (sinkTimer < sinkProgress * 2 + sinkHoldTime)
        {
            float adjustedTime = sinkTimer - sinkProgress - sinkHoldTime;
            float t = adjustedTime / sinkProgress;
            transform.position = Vector3.Lerp(sinkTargetPosition, sinkStartPosition, t);
        }
        // Завершение цикла
        else
        {
            isSinking = false;
            hasSunkForCurrentPlayer = false;
            currentPlayer = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Legs") && !isSinking && !hasSunkForCurrentPlayer)
        {
            // Получаем ссылку на игрока
            Transform player = other.transform.root;
            
            // Проверяем, что игрок действительно приземляется сверху
            if (IsPlayerAbovePlatform(other, player))
            {
                currentPlayer = player;
                StartSinking();
            }
        }
    }

    private bool IsPlayerAbovePlatform(Collider legsCollider, Transform player)
    {
        // Получаем нижнюю границу игрока (с небольшим отступом)
        float playerBottom = legsCollider.bounds.min.y + playerBottomOffset;
        
        // Получаем верхнюю границу платформы
        float platformTop = GetPlatformTop() - platformTopOffset;
        
        return playerBottom > platformTop;
    }

    // Находим самую верхнюю точку платформы среди всех коллайдеров
    private float GetPlatformTop()
    {
        float maxY = float.MinValue;
        
        foreach (Collider col in platformColliders)
        {
            if (col != null && col.enabled)
            {
                float top = col.bounds.max.y;
                if (top > maxY) maxY = top;
            }
        }
        
        return maxY;
    }

    private void StartSinking()
    {
        isSinking = true;
        hasSunkForCurrentPlayer = true;
        sinkTimer = 0f;
        
        // Рассчитываем время для опускания
        sinkProgress = sinkDistance / sinkSpeed;
        
        sinkStartPosition = transform.position;
        sinkTargetPosition = new Vector3(
            transform.position.x,
            transform.position.y - sinkDistance,
            transform.position.z
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}