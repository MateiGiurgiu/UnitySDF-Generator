using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private float m_movementSpeed = 2f;
    [SerializeField] private float m_rotationSpeed = 0.25f;
    private Transform m_transform;
    private Vector3 m_lastMousePosition = Vector2.zero;

    private void Start()
    {
        m_transform = gameObject.GetComponent<Transform>();
        m_lastMousePosition = Input.mousePosition;
    }

    private void Update()
    {
        // movement
        float Z = Input.GetAxis("Vertical");
        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetKey(KeyCode.Q)? -1 : (Input.GetKey(KeyCode.E)? 1 : 0);

        m_transform.Translate(new Vector3(X, Y, Z) * m_movementSpeed * Time.deltaTime, Space.Self);


        // rotation
        Vector3 mouseDelta = Input.mousePosition - m_lastMousePosition;
        Vector3 rotation = new Vector3(-mouseDelta.y * m_rotationSpeed, mouseDelta.x * m_rotationSpeed, 0);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x + rotation.x, transform.eulerAngles.y + rotation.y, 0);
        m_lastMousePosition = Input.mousePosition;
    }
}
