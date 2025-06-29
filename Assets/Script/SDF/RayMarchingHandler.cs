using UnityEngine;
using UnityEngine.Rendering;

public class RayMarchingHandler : MonoBehaviour
{
    [Header("Compute Shader")]
    public ComputeShader jfaComputeShader;

    [Header("Objects to convert")]
    public GameObject[] objectsToConvert;
    public Camera renderCamera;

    [Header("Distance Field Settings")]
    [Range(128, 2048)]
    public int textureSize = 512;

    [Header("Ray Marching Settings")]
    [Range(0.01f, 2.0f)]
    public float blendRadius = 0.5f;
    [Range(50, 300)]
    public int maxSteps = 100;
    [Range(0.001f, 0.1f)]
    public float epsilon = 0.01f;
    public float maxDistance = 100f;

    [Header("Output Texture")]
    public RenderTexture outputTexture;
    //Compute shader Kernels
    int initializeSeedKernel;
    int rayMarchingKernel;
    int jumpFloodKernel;

    //Render texture
    RenderTexture[] objectSilhouettes;
    RenderTexture[] objectDistanceFields;
    RenderTexture[] seedsTextures;
    RenderTexture[] tmpJFATextures;

    Camera[] silhouettecameras;



    void Start()
    {
        InitializeComputeShader();
        SetupRenderTextures();
        Setupsilhouettecameras();
    }

    void InitializeComputeShader()
    {
        if (jfaComputeShader == null)
        {
            Debug.LogError("Compute Shader not assigned!");
            return;
        }

        // Initialize Kernels
        initializeSeedKernel = jfaComputeShader.FindKernel("InitializeSeeds"); // Updated kernel name
        rayMarchingKernel = jfaComputeShader.FindKernel("RayMarchingScene");
        jumpFloodKernel = jfaComputeShader.FindKernel("JumpFloodPass");
    }

    void SetupRenderTextures()
    {
        // Create RenderTextures for each object
        int numObjects = objectsToConvert.Length;
        objectSilhouettes = new RenderTexture[numObjects];
        objectDistanceFields = new RenderTexture[numObjects];
        seedsTextures = new RenderTexture[numObjects];
        tmpJFATextures = new RenderTexture[numObjects];

        for (int i = 0; i < numObjects; i++)
        {
            // Silhouette textures (for rendering object shapes)
            objectSilhouettes[i] = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
            objectSilhouettes[i].enableRandomWrite = true;
            objectSilhouettes[i].Create();

            // Seed textures (JFA working memory)
            seedsTextures[i] = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat);
            seedsTextures[i].enableRandomWrite = true;
            seedsTextures[i].Create();

            // Distance field textures (JFA output)
            objectDistanceFields[i] = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat);
            objectDistanceFields[i].enableRandomWrite = true;
            objectDistanceFields[i].Create();

            // Temp textures for JFA ping-pong
            tmpJFATextures[i] = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat);
            tmpJFATextures[i].enableRandomWrite = true;
            tmpJFATextures[i].Create();
        }

    }

    void Setupsilhouettecameras()
    {
        int numObjects = objectsToConvert.Length;
        silhouettecameras = new Camera[numObjects];

        for (int i = 0; i < numObjects; i++)
        {
            GameObject camObj = new GameObject("SilhouetteCamera_" + i);
            camObj.transform.SetParent(transform);
            Camera cam = camObj.AddComponent<Camera>();
            cam.CopyFrom(renderCamera);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.enabled = false;
            silhouettecameras[i] = cam;
        }
    }
    void Update()
    {
        for (int i = 0; i < objectsToConvert.Length; i++)
        {
            GenerateDistanceField(i);
        }
        PerformRayMarching();

        // Display result :: Need to Change Later URP support is broken
        if (outputTexture != null)
        {
            Graphics.Blit(outputTexture, (RenderTexture)null);
        }
    }

    void GenerateDistanceField(int objectIndex)
    {
        if (objectIndex >= objectsToConvert.Length) return;

        RenderObjectSilhouette(objectIndex);
        InitializeSeeds(objectIndex);
        RunJumpFloodAlgorithm(objectIndex);

    }

    void RenderObjectSilhouette(int objectIndex)
    {
        for (int i = 0; i < objectsToConvert.Length; i++)
        {
            if (objectsToConvert[i] != null)
            {
                objectsToConvert[i].SetActive(i == objectIndex);
            }
        }

        silhouettecameras[objectIndex].Render();

        // Restore all objects
        for (int i = 0; i < objectsToConvert.Length; i++)
        {
            if (objectsToConvert[i] != null)
            {
                objectsToConvert[i].SetActive(true);
            }
        }
    }

    void InitializeSeeds(int objectIndex)
    {
        jfaComputeShader.SetTexture(initializeSeedKernel, "ObjectSilhouette", objectSilhouettes[objectIndex]); // Updated property name
        jfaComputeShader.SetTexture(initializeSeedKernel, "SeedTexture", seedsTextures[objectIndex]);
        jfaComputeShader.SetInt("textureSize", textureSize);
        int threadGroupSize = Mathf.CeilToInt(textureSize / 8.0f);
        jfaComputeShader.Dispatch(initializeSeedKernel, threadGroupSize, threadGroupSize, 1);
    }

    void RunJumpFloodAlgorithm(int objectIndex)
    {
        RenderTexture currentInput = seedsTextures[objectIndex];
        RenderTexture currentOutput = tmpJFATextures[objectIndex];

        int jumpDistance = textureSize / 2;

        while (jumpDistance >= 1)
        {
            jfaComputeShader.SetTexture(jumpFloodKernel, "PreviousJFA", currentInput);
            jfaComputeShader.SetTexture(jumpFloodKernel, "SeedTexture", currentOutput);
            jfaComputeShader.SetInt("JumpDistance", jumpDistance);
            jfaComputeShader.SetInt("textureSize", textureSize);

            int threadGroups = Mathf.CeilToInt(textureSize / 8.0f);
            jfaComputeShader.Dispatch(jumpFloodKernel, threadGroups, threadGroups, 1);

            // Swap textures for next iteration
            RenderTexture temp = currentInput;
            currentInput = currentOutput;
            currentOutput = temp;

            jumpDistance /= 2;
        }

        // Copy final result to distance field
        Graphics.CopyTexture(currentInput, objectDistanceFields[objectIndex]);

    }

    void PerformRayMarching()
    {
        Matrix4x4 cameraToWorld = renderCamera.cameraToWorldMatrix;
        Matrix4x4 cameraInvProjection = renderCamera.projectionMatrix.inverse;

        jfaComputeShader.SetMatrix("CameraToWorld", cameraToWorld);
        jfaComputeShader.SetMatrix("CameraInvProjection", cameraInvProjection);
        jfaComputeShader.SetVector("CameraPosition", renderCamera.transform.position);
        jfaComputeShader.SetVector("ScreenSize", new Vector2(Screen.width, Screen.height));

        // Set raymarching parameters
        jfaComputeShader.SetFloat("MaxDistance", maxDistance);
        jfaComputeShader.SetInt("MaxSteps", maxSteps);
        jfaComputeShader.SetFloat("Epsilon", epsilon);
        jfaComputeShader.SetFloat("BlendRadius", blendRadius);
        jfaComputeShader.SetInt("TextureSize", textureSize);

        if (objectDistanceFields.Length > 0) jfaComputeShader.SetTexture(rayMarchingKernel, "Object1DistanceField", objectDistanceFields[0]);
        if (objectDistanceFields.Length > 1) jfaComputeShader.SetTexture(rayMarchingKernel, "Object2DistanceField", objectDistanceFields[1]);

        jfaComputeShader.SetTexture(rayMarchingKernel, "OutputTexture", outputTexture);

        // Dispatch raymarching
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        jfaComputeShader.Dispatch(rayMarchingKernel, threadGroupsX, threadGroupsY, 1);
    }

    void OnDestroy()
    {
        if (objectSilhouettes != null)
        {
            for (int i = 0; i < objectSilhouettes.Length; i++)
            {
                if (objectSilhouettes[i] != null)
                    objectSilhouettes[i].Release();
            }
        }

        if (objectDistanceFields != null)
        {
            for (int i = 0; i < objectDistanceFields.Length; i++)
            {
                if (objectDistanceFields[i] != null)
                    objectDistanceFields[i].Release();
            }
        }

        if (seedsTextures != null)
        {
            for (int i = 0; i < seedsTextures.Length; i++)
            {
                if (seedsTextures[i] != null)
                    seedsTextures[i].Release();
            }
        }

        if (tmpJFATextures != null)
        {
            for (int i = 0; i < tmpJFATextures.Length; i++)
            {
                if (tmpJFATextures[i] != null)
                    tmpJFATextures[i].Release();
            }
        }
    }

    void OnGUI()
    {
        if (outputTexture != null)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), outputTexture);
        }

        // Debug info
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Objects: {objectsToConvert.Length}");
        GUILayout.Label($"Texture Size: {textureSize}x{textureSize}");
        GUILayout.Label($"Blend Radius: {blendRadius:F2}");
        GUILayout.Label($"Max Steps: {maxSteps}");
        GUILayout.EndArea();
    }
}
