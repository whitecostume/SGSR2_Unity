using UnityEngine;

public class SGSR2_UICamera : MonoBehaviour
{
    private Camera cam;

    public Camera uiCamera => cam;

    void Awake()
    {
        OnEnable();
    }

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        
    }
}