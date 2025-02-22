using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private Transform player;
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] public Vector2 cameraLimit = new Vector2(-45, 40);


    private float mouseX;
    private float mouseY;
    private float offsetDistanceY;

    private void Start()
    {
        offsetDistanceY = transform.position.y;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void Update()
    {
        transform.position = player.position + new Vector3(0, offsetDistanceY, 0);

        Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();
        mouseX += Input.GetAxis("Mouse X") * sensitivity;
        mouseY += Input.GetAxis("Mouse Y") * sensitivity;
        mouseY = Mathf.Clamp(mouseY, cameraLimit.x, cameraLimit.y);

        transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);

    }
}
