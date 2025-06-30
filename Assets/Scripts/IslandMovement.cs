using UnityEngine;
using System.Collections.Generic;

public class IslandMovement : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 1f;
    public float positionSmoothTime = 0.1f;

    [Header("Tilt Restrictions")]
    public float maxTiltAngle = 10f;
    public float tiltRecoverySpeed = 5f;

    [Header("Sag Settings")]
    public float maxSag = 0.2f;
    public float sagPerMassUnit = 0.01f;
    public float sagSmoothTime = 0.3f; // Плавность изменения проседания

    private Vector3 originalPosition;
    private Rigidbody rb;
    private List<Rigidbody> objectsOnPlatform = new List<Rigidbody>();
    private float totalMass = 0f;
    private Vector3 positionVelocity;
    private float currentSag = 0f;
    private float sagVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalPosition = transform.position;

        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // Замораживаем ненужные движения
        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationY;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.isTrigger) return;

        Rigidbody otherRb = collision.collider.attachedRigidbody;
        if (otherRb != null && !objectsOnPlatform.Contains(otherRb))
        {
            objectsOnPlatform.Add(otherRb);
            totalMass += otherRb.mass;
            
            if (otherRb.CompareTag("MainPlayer"))
            {
                otherRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                CapsuleCollider col = otherRb.GetComponent<CapsuleCollider>();
                if (col != null)
                {
                    col.material.dynamicFriction = 1f;
                    col.material.staticFriction = 1f;
                    col.contactOffset = 0.1f;
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.isTrigger) return;

        Rigidbody otherRb = collision.collider.attachedRigidbody;
        if (otherRb != null && objectsOnPlatform.Contains(otherRb))
        {
            objectsOnPlatform.Remove(otherRb);
            totalMass -= otherRb.mass;
        }
    }

    void FixedUpdate()
    {
        ApplyFloatingMotion();
        ApplyTiltRestrictions();
    }

    private void ApplyFloatingMotion()
    {
        // 1. Рассчет целевого проседания на основе массы
        float targetSag = Mathf.Clamp(totalMass * sagPerMassUnit, 0f, maxSag);
        
        // 2. Плавное изменение текущего проседания
        currentSag = Mathf.SmoothDamp(currentSag, targetSag, ref sagVelocity, sagSmoothTime);
        
        // 3. Базовое движение платформы (синусоида)
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        
        // 4. Итоговая позиция: начальная позиция + колебания - проседание
        Vector3 targetPosition = originalPosition + 
                                Vector3.up * yOffset - 
                                Vector3.up * currentSag;
        
        // 5. Плавное перемещение к целевой позиции
        rb.MovePosition(Vector3.SmoothDamp(
            rb.position, 
            targetPosition, 
            ref positionVelocity, 
            positionSmoothTime
        ));
    }

    private void ApplyTiltRestrictions()
    {
        // Текущее вращение
        Quaternion currentRotation = rb.rotation;
        
        // Извлекаем углы Эйлера
        Vector3 euler = currentRotation.eulerAngles;
        
        // Ограничиваем углы наклона
        euler.x = ClampAngle(euler.x, -maxTiltAngle, maxTiltAngle);
        euler.z = ClampAngle(euler.z, -maxTiltAngle, maxTiltAngle);
        
        // Плавное восстановление горизонтального положения
        Quaternion targetRotation = Quaternion.Euler(
            Mathf.LerpAngle(euler.x, 0, tiltRecoverySpeed * Time.fixedDeltaTime),
            euler.y,
            Mathf.LerpAngle(euler.z, 0, tiltRecoverySpeed * Time.fixedDeltaTime)
        );
        
        rb.MoveRotation(targetRotation);
    }

    // Вспомогательная функция для корректного ограничения углов
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return Mathf.Clamp(angle, min, max);
    }
}