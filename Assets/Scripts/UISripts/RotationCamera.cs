using UnityEngine;

public class RotationCamera : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
