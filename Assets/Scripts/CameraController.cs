using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;
    public float zoomSpeed = 2f;

    private Vector3 lastMousePosition;
    private float lastTouchDistance;

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleTouchInput();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        transform.Translate(moveX, 0, moveZ);
    }

    void HandleMouseLook()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotationX = delta.y * lookSpeed * Time.deltaTime;
            float rotationY = delta.x * lookSpeed * Time.deltaTime;

            transform.eulerAngles += new Vector3(-rotationX, rotationY, 0);
            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                float rotationX = touch.deltaPosition.y * lookSpeed * Time.deltaTime;
                float rotationY = touch.deltaPosition.x * lookSpeed * Time.deltaTime;

                transform.eulerAngles += new Vector3(-rotationX, rotationY, 0);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                float currentTouchDistance = Vector2.Distance(touch0.position, touch1.position);
                if (lastTouchDistance != 0)
                {
                    float deltaDistance = currentTouchDistance - lastTouchDistance;
                    transform.Translate(0, 0, deltaDistance * zoomSpeed * Time.deltaTime);
                }
                lastTouchDistance = currentTouchDistance;
            }
        }
        else
        {
            lastTouchDistance = 0;
        }
    }
}