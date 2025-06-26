using UnityEngine;

public class MetaballSceneSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private Material rayMarchingMaterial;
    [SerializeField] private int numberOfTestBalls = 3;
    [SerializeField] private float spawnRadius = 5f;
    void Start()
    {
        if (setupOnStart)
        {
            SetupScene();
        }
    }

    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        if (MetaballManager.Instance == null)
        {
            GameObject managerObject = new GameObject("MetaballManager");
            managerObject.AddComponent<MetaballManager>();
        }
        CreateRenderQuad();
        CreateTestMetaballs();

        Debug.Log("Metaball scene setup complete!");


    }

    private void CreateRenderQuad()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = FindAnyObjectByType<Camera>();
        GameObject quad = new GameObject("MetaballRenderQuad");
        quad.transform.SetParent(cam.transform, false);
        quad.transform.localPosition = new Vector3(0, 0, cam.nearClipPlane + 1f); // 1 unit in front of camera
        quad.transform.localRotation = Quaternion.identity;
        MetaballRenderer renderer = quad.AddComponent<MetaballRenderer>();
        if (rayMarchingMaterial == null)
        {
            rayMarchingMaterial = Resources.Load<Material>("RaymarchMaterial");
            if (rayMarchingMaterial == null)
            {
                Debug.LogWarning("No ray marching material assigned! Please assign the metaball material to the renderer.");
            }
        }
        if (rayMarchingMaterial != null)
        {
            quad.GetComponent<MeshRenderer>().material = rayMarchingMaterial;
        }

    }

    private void CreateTestMetaballs()
    {
        for (int i = 0; i < numberOfTestBalls; i++)
        {
            // Create a test sphere
            GameObject ballGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballGO.name = $"TestMetaball_{i}";

            // Random position
            Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
            ballGO.transform.position = randomPos;

            // Add metaball field contributor
            MetaballFieldContributor contributor = ballGO.AddComponent<MetaballFieldContributor>();

            // Random properties for variety
            contributor.GetType().GetField("fieldStrength", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(contributor, Random.Range(0.5f, 2f));
            contributor.GetType().GetField("fieldRadius", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(contributor, Random.Range(1f, 3f));

            // Make the visual sphere semi-transparent so we can see the ray marched result
            Renderer sphereRenderer = ballGO.GetComponent<Renderer>();
            Material transparentMat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
            transparentMat.color = new Color(1, 1, 1, 0.3f);
            sphereRenderer.material = transparentMat;

            // Add a simple movement script for testing
            ballGO.AddComponent<SimpleMetaballMover>();
        }
    }

}
