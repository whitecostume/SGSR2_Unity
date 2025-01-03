using UnityEngine;
using UnityEngine.Rendering;

public class SGSR2_UICamera : MonoBehaviour
{
    private Camera cam;

    public Camera uiCamera => cam;

    private CommandBuffer uiBlitCmd;

    void Awake()
    {
        OnEnable();
    }

    private void OnEnable()
    {
        cam = GetComponent<Camera>();       
    }

    private void OnDisable()
    {
        if(uiBlitCmd != null)
        {
            cam?.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, uiBlitCmd);
            uiBlitCmd.Dispose();
            uiBlitCmd = null;
        }
    }

    public void SetRenderTarget(RenderTexture renderTexture)
    {
        if(enabled == false)
        {
            return;
        }

        if(uiBlitCmd == null && cam != null)
        {
            uiBlitCmd = new CommandBuffer()
            {
                name = "SGSR2_UI"
            };
            cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, uiBlitCmd);
        }

        if(uiBlitCmd == null)
        {
            return;
        }
        uiBlitCmd.Clear();

        uiBlitCmd?.Blit(renderTexture, null as RenderTexture);
    }
}