using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SGSR2 : MonoBehaviour
{
    private Camera cam;
    private Material material;
    private Matrix4x4 prevViewProj;
    private RenderTexture motionDepthClipRT;
    private RenderTexture[] outputRTs = new RenderTexture[2];
    private int frameCount = 0;
    private int jitterIndex = 0;


    [Header("Settings")]
    [Range(1.0f, 2.0f)]
    public float upscaledRatio = 1.5f;

    [Range(0f, 1f)]
    public float minLerpContribution = 0.3f;

    public RawImage displayMotionDepthClipImg;

    // 预计算的Halton序列
    private Vector2[] HaltonSequence = new Vector2[32];

    private Rect originalRect,scaledRect;


    private float Halton(int index, int baseN)
    {
        float result = 0f;
        float invBase = 1f / baseN;
        float fraction = invBase;
        
        while (index > 0)
        {
            result += (index % baseN) * fraction;
            index /= baseN;
            fraction *= invBase;
        }
        
        return result;
    }

    private Vector2 GetJitter()
    {
        return HaltonSequence[jitterIndex % HaltonSequence.Length];
    }

    private void OnEnable()
    {
        for (int i = 0; i < HaltonSequence.Length; i++) // 使用32帧序列
        {
            HaltonSequence[i] = new Vector2(
                Halton(i + 1, 2) - 0.5f,
                Halton(i + 1, 3) - 0.5f
            );
        }

        
        cam = GetComponent<Camera>();

        material = new Material(Shader.Find("Hidden/SGSR2"));
        prevViewProj = cam.nonJitteredProjectionMatrix * cam.worldToCameraMatrix;
        
        // Enable depth texture
        cam.depthTextureMode |= DepthTextureMode.Depth;
        cam.depthTextureMode |= DepthTextureMode.MotionVectors;

        originalRect = cam.rect;
        
    }

    private void OnDisable()
    {
        if (material != null)
            DestroyImmediate(material);
        material = null;
        ReleaseRenderTextures();
        cam.rect = originalRect;
        cam?.ResetProjectionMatrix();
    }

    private Vector2Int screenSize;

    void Update()
    {
        screenSize = new Vector2Int(Screen.width, Screen.height);
    }


    void OnPreRender()
    {
        if(cam == null)
            return;
        
        scaledRect = new Rect(originalRect.x, originalRect.y, originalRect.width / upscaledRatio, originalRect.height / upscaledRatio);
        cam.rect = scaledRect;
        
        if(!cam.orthographic)
        {
            cam.ResetProjectionMatrix();

            var nextJitter = GetJitter();
            var jitProj = cam.projectionMatrix;
            cam.nonJitteredProjectionMatrix = jitProj;
            
            jitProj.m02 += nextJitter.x / screenSize.x ; 
            jitProj.m12 += nextJitter.y / screenSize.y ;   

            cam.projectionMatrix = jitProj;

        }

        UpdateTargetRenderTexture();
        
    }
    

    private void ReleaseRenderTextures()
    {

        if(outputRTs != null)
        {
            for (int i = 0; i < outputRTs.Length; i++)
            {
                if(outputRTs[i] != null)
                {
                    outputRTs[i].Release();
                    outputRTs[i] = null;
                }   
            }
        }

        if (motionDepthClipRT != null)
        {
            motionDepthClipRT.Release();
            motionDepthClipRT = null;
        }

        if(displayMotionDepthClipImg != null)
        {
            displayMotionDepthClipImg.texture = null;
            displayMotionDepthClipImg.enabled = false;
        }
    }

    private void UpdateTargetRenderTexture()
    {
        int width = screenSize.x;
        int height = screenSize.y;

        if(outputRTs[0] == null || outputRTs[0].width != width || outputRTs[0].height != height)
        {
            for (int i = 0; i < outputRTs.Length; i++)
            {
                if(outputRTs[i] != null)
                    outputRTs[i].Release();
                outputRTs[i] = new RenderTexture(width, height, 0, RenderTextureFormat.Default);
                outputRTs[i].filterMode = FilterMode.Bilinear;
                outputRTs[i].name = "SGSR2_outputRT" + i;
            }
            // 重置帧计数
            frameCount = 0;
        }

    }

    private void UpdateRenderTextures(int width, int height)
    {
        int renderWidth = Mathf.RoundToInt(width / upscaledRatio);
        int renderHeight = Mathf.RoundToInt(height / upscaledRatio);

        if (motionDepthClipRT == null || motionDepthClipRT.width != renderWidth || motionDepthClipRT.height != renderHeight)
        {
            if (motionDepthClipRT != null)
                motionDepthClipRT.Release();
            motionDepthClipRT = new RenderTexture(renderWidth, renderHeight, 0, RenderTextureFormat.ARGBHalf);
            motionDepthClipRT.filterMode = FilterMode.Point;
            motionDepthClipRT.name = "SGSR2_MotionDepthClipRT";

            if(displayMotionDepthClipImg != null)
            {
                displayMotionDepthClipImg.texture = motionDepthClipRT;
                displayMotionDepthClipImg.enabled = true;
            }
        }

    }

    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        cam.rect = originalRect;

        Graphics.Blit(source, destination);
        RenderToDisplayRT(destination);
    }


    private void RenderToDisplayRT(RenderTexture source)
    {
        UpdateRenderTextures(screenSize.x, screenSize.y);


        Vector2 outputSize = new Vector2(screenSize.x, screenSize.y);

        // Calculate render sizes
        Vector2 renderSize = new Vector2(motionDepthClipRT.width, motionDepthClipRT.height);
        
        // Generate jitter using the new method
        Vector4 jitter = GetJitter();
    
        
        // Update shader parameters
        Matrix4x4 currentViewProj = cam.nonJitteredProjectionMatrix * cam.worldToCameraMatrix;

        Matrix4x4 clipToPrevClip = Matrix4x4.Scale(new Vector3(1, -1, 1)) * // Y轴翻转
                          prevViewProj * 
                          Matrix4x4.Inverse(currentViewProj) *
                          Matrix4x4.Scale(new Vector3(1, -1, 1));
        
        material.SetMatrix("_ClipToPrevClip", clipToPrevClip);
        material.SetVector("_RenderSize", renderSize);
        material.SetVector("_OutputSize", outputSize);
        material.SetVector("_RenderSizeRcp", new Vector2(1f/renderSize.x, 1f/renderSize.y));
        material.SetVector("_OutputSizeRcp", new Vector2(1f/outputSize.x, 1f/outputSize.y));
        material.SetVector("_JitterOffset", jitter);
        material.SetVector("_ScaleRatio", new Vector2(upscaledRatio, Mathf.Min(20f, Mathf.Pow((outputSize.x * outputSize.y) / (renderSize.x * renderSize.y), 3f))));
        material.SetFloat("_CameraFovAngleHor", Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * renderSize.x / renderSize.y);
        material.SetFloat("_MinLerpContribution", minLerpContribution);
        material.SetFloat("_Reset", frameCount == 0 ? 1f : 0f);
        
        // Check if camera moved
        // float vpDiff = 0f;
        // for(int i = 0; i < 4; i++)
        //     for(int j = 0; j < 4; j++)
        //         vpDiff += Mathf.Abs(currentViewProj[i,j] - prevViewProj[i,j]);
        // material.SetInt("_SameCameraFrmNum", vpDiff < 1e-5f ? 1 : 0);

        // Convert Pass

        var historyRT = outputRTs[frameCount % 2];
        var displayRT = outputRTs[(frameCount + 1) % 2];
        
        material.SetTexture("_MainTex", source);
        material.SetTexture("_DepthTex", Shader.GetGlobalTexture("_CameraDepthTexture"));
        material.SetTexture("_CameraMotionVectorsTexture", Shader.GetGlobalTexture("_CameraMotionVectorsTexture"));
        Graphics.Blit(source, motionDepthClipRT, material, 0);

        // Upscale Pass
        material.SetTexture("_MainTex", source);
        material.SetTexture("_PrevHistory", historyRT);
        material.SetTexture("_MotionDepthClipBuffer", motionDepthClipRT);
        Graphics.Blit(source, displayRT, material, 1);

        // // Copy result to history
        // Graphics.Blit(displayRT, historyRT);

        // Update previous frame data
        prevViewProj = currentViewProj;
        frameCount++;
        jitterIndex++;

    }

   

} 