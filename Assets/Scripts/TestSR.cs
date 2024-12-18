using UnityEngine;

public class TestSR : MonoBehaviour
{
    public SGSR2 sgSR;

    void Start()
    {
        Application.targetFrameRate = 120;
    }

    private void OnGUI()
    {
        // UI适配
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1280f, Screen.height / 720f, 1f));
        // GUI.Label(new Rect(10, 10, 200, 20), "Frame Count: " + Time.tim);
        // 打印FPS
        GUI.Label(new Rect(10, 20, 400, 40), "FPS: " + (1.0f / Time.deltaTime).ToString("f2"));
        if (sgSR != null)
        {
            // 显示当前的upscaledRatio
            GUI.Label(new Rect(10, 40, 400, 40), "upscaledRatio: " + sgSR.upscaledRatio.ToString("f2"));
            // 显示拖动条，调整upscaledRatio
            sgSR.upscaledRatio = GUI.HorizontalSlider(new Rect(10, 60, 400, 40), sgSR.upscaledRatio, 1.0f, 2.0f);
            // 切换sgSR
            sgSR.enabled = GUI.Toggle(new Rect(10, 100, 400, 40), sgSR.enabled, "Enable SGSR");
        }
    }
}