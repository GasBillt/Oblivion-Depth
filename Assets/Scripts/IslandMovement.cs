using UnityEngine;
using System.Collections.Generic;

public class FloatingPlatform : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 1f;
    public float positionSmoothTime = 0.1f;
    
    [Header("Tilt Restrictions")]
    public float maxTiltAngle = 10f;
    public float tiltRecoverySpeed = 5f;
    public float rotationSmoothTime = 0.1f;
    
    [Header("Sag Settings")]
    public float maxSag = 0.2f;
    public float sagPerMassUnit = 0.01f;
    
    private Vector3 initialPosition;
    private Rigidbody rb;
    private List<Rigidbody> objectsOnPlatform = new List<Rigidbody>();
    private float totalMass = 0f;
    private Vector3 positionVelocity;
    private Vector3 rotationVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        
        // Важные физические настройки
        rb.isKinematic = true; // Теперь кинематический
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // Замораживаем ненужные оси
        rb.constraints = RigidbodyConstraints.FreezePositionX | 
                         RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationY;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Игнорируем триггеры и объекты без Rigidbody
        if (collision.collider.isTrigger) return;
        
        Rigidbody otherRb = collision.collider.attachedRigidbody;
        if (otherRb != null && !objectsOnPlatform.Contains(otherRb))
        {
            objectsOnPlatform.Add(otherRb);
            totalMass += otherRb.mass;
            
            // Важно для игрока: улучшаем отслеживание столкновений
            if (otherRb.CompareTag("Player"))
            {
                otherRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                if (otherRb.gameObject.GetComponent<CapsuleCollider>() != null)
                {
                    otherRb.gameObject.GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
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
        // Плавное синусоидальное движение
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        
        // Рассчет проседания с плавным изменением
        float targetSag = Mathf.Clamp(totalMass * sagPerMassUnit, 0f, maxSag);
        
        // Целевая позиция с плавным сглаживанием
        Vector3 targetPosition = initialPosition + Vector3.up * (yOffset - targetSag);
        Vector3 newPosition = Vector3.SmoothDamp(
            rb.position, 
            targetPosition, 
            ref positionVelocity, 
            positionSmoothTime
        );
        
        rb.MovePosition(newPosition);
    }

    private void ApplyTiltRestrictions()
    {
        // Целевое вращение - без наклона
        Quaternion targetRotation = Quaternion.Euler(0, rb.rotation.eulerAngles.y, 0);
        
        // Плавное сглаживание вращения
        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation, 
            targetRotation, 
            tiltRecoverySpeed * Time.fixedDeltaTime
        );
        
        // Ограничение углов наклона
        Vector3 euler = newRotation.eulerAngles;
        euler.x = ClampAngle(euler.x, -maxTiltAngle, maxTiltAngle);
        euler.z = ClampAngle(euler.z, -maxTiltAngle, maxTiltAngle);
        
        rb.MoveRotation(Quaternion.Euler(euler));
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return Mathf.Clamp(angle, min, max);
    }
}