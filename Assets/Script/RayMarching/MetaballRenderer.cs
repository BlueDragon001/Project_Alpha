using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class MetaballRenderer : MonoBehaviour
{
    [Header("Rendering Settings")]
    [SerializeField] private Material metaballMaterial;
    [SerializeField] private bool createQuadAuto = true;
    [SerializeField] private Vector2 quadSize = new Vector2(20f, 20f);

    [Header("Debug Settings")]
    [SerializeField] private bool showQuadGizmo = true;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        if (createQuadAuto)
        {
            CreateRenderQuad();
        }
        if (metaballMaterial != null)
        {
            meshRenderer.material = metaballMaterial;
        }
    }

    private void CreateRenderQuad()
    {
        Mesh quadMesh = new Mesh();
        quadMesh.name = "MetaballRenderQuad";

       Vector3[] vertices = new Vector3[]
        {
            new Vector3(-quadSize.x/2, -quadSize.y/2, 0), // Bottom-left
            new Vector3( quadSize.x/2, -quadSize.y/2, 0), // Bottom-right
            new Vector3( quadSize.x/2,  quadSize.y/2, 0), // Top-right
            new Vector3(-quadSize.x/2,  quadSize.y/2, 0)  // Top-left
        };

        Vector2[] uvs = new Vector2[]
        {
            new(0, 0), // Bottom-left
            new(1, 0), // Bottom-right
            new(1, 1), // Top-right
            new(0, 1)  // Top-left
        };

        int[] triangles = new int[6]
        {
            0, 1, 2,
            0, 2, 3
        };

        quadMesh.vertices = vertices;
        quadMesh.uv = uvs;
        quadMesh.triangles = triangles;
        quadMesh.RecalculateNormals();

        meshFilter.mesh = quadMesh;
    }

    private void OnDrawGizmos()
    {
        if (!showQuadGizmo) return;

        Gizmos.color = Color.white;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(quadSize.x, quadSize.y, 0.1f));
    }
    
    private void OnValidate()
    {
        // Update quad size if changed in inspector
        if (createQuadAuto && meshFilter != null && meshFilter.sharedMesh != null)
        {
            CreateRenderQuad();
        }
    }
}
