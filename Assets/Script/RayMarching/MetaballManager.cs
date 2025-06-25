using System.Collections.Generic;
using UnityEngine;

public class MetaballManager : MonoBehaviour
{
    private static MetaballManager instance;
    public static MetaballManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<MetaballManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("MetaballManager");
                    instance = obj.AddComponent<MetaballManager>();
                }
            }
            return instance;
        }
    }
    [Header("Settings")]
    [SerializeField] private int maxFields = 32;
    [SerializeField] private float cullingDistance = 50f;
    [SerializeField] private bool debugMode = false;

    [Header("Shader Properties")]
    [SerializeField] private string fieldPositionsProperty = "_FieldPositions";
    [SerializeField] private string fieldRadiiProperty = "_FieldRadii";
    [SerializeField] private string fieldStrengthsProperty = "_FieldStrengths";
    [SerializeField] private string fieldCountProperty = "_FieldCount";
    [SerializeField] private string fieldTypesProperty = "_FeildTypes";
    [SerializeField] private string fieldNegativeProperty = "_FieldNegatives";

    private List<MetaballFieldContributor> allFields = new List<MetaballFieldContributor>();
    private List<FieldData> activeFieldData = new List<FieldData>();
    private bool isDirty = true;

    private Vector4[] fieldPositions;
    private float[] fieldRadii;
    private float[] fieldStrengths;
    private float[] fieldTypes;
    private float[] fieldNegatives;

    private Camera mainCamera;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializeArrays();

        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = FindAnyObjectByType<Camera>();

    }

    void InitializeArrays()
    {
        fieldPositions = new Vector4[maxFields];
        fieldRadii = new float[maxFields];
        fieldStrengths = new float[maxFields];
        fieldTypes = new float[maxFields];
        fieldNegatives = new float[maxFields];
    }
    public void RegisterField(MetaballFieldContributor field)
    {
        if (!allFields.Contains(field))
        {
            allFields.Add(field);
            MakeDirty();
            if (debugMode)
            {
                Debug.Log($"Registered field: {field.name}");
            }
        }
    }

    public void UnregisterField(MetaballFieldContributor field)
    {
        if (allFields.Contains(field))
        {
            allFields.Remove(field);
            MakeDirty();
            if (debugMode)
            {
                Debug.Log($"Unregistered field: {field.name}");
            }
        }
    }

    public void MakeDirty()
    {
        isDirty = true;
    }

    private void RebuildFieldDataArray()
    {
        activeFieldData.Clear();
        Vector3 cameraPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;

        foreach (var field in allFields)
        {
            if (field == null || !field.gameObject.activeInHierarchy) continue;
            float distance = Vector3.Distance(field.transform.position, cameraPos);
            if (distance > cullingDistance) continue;
            activeFieldData.Add(field.GetFieldData());

        }
        activeFieldData.Sort((a, b) =>
             {
                 float distA = Vector3.Distance(a.position, cameraPos);
                 float distB = Vector3.Distance(b.position, cameraPos);
                 return distA.CompareTo(distB);
             });
        if (activeFieldData.Count > maxFields)
        {
            activeFieldData.RemoveRange(maxFields, activeFieldData.Count - maxFields);
        }

        if (debugMode)
        {
            Debug.Log($"Active fields: {activeFieldData.Count}/{allFields.Count}");
        }
    }

    private void SendDataToShader()
    {
        int fieldCount = activeFieldData.Count;
        System.Array.Clear(fieldPositions, 0, fieldPositions.Length);
        System.Array.Clear(fieldStrengths, 0, fieldStrengths.Length);
        System.Array.Clear(fieldRadii, 0, fieldRadii.Length);
        System.Array.Clear(fieldTypes, 0, fieldTypes.Length);
        System.Array.Clear(fieldNegatives, 0, fieldNegatives.Length);

        for (int i = 0; i < fieldCount; i++)
        {
            FieldData data = activeFieldData[i];
            fieldPositions[i] = new Vector4(data.position.x, data.position.y, data.position.z, 0);
            fieldRadii[i] = data.radius;
            fieldStrengths[i] = data.strength;
            fieldTypes[i] = data.type;
            fieldNegatives[i] = data.isNegative;
        }
        Shader.SetGlobalVectorArray(fieldPositionsProperty, fieldPositions);
        Shader.SetGlobalFloatArray(fieldRadiiProperty, fieldRadii);
        Shader.SetGlobalFloatArray(fieldStrengthsProperty, fieldStrengths);
        Shader.SetGlobalFloatArray(fieldTypesProperty, fieldTypes);
        Shader.SetGlobalFloatArray(fieldNegativeProperty, fieldNegatives);
        Shader.SetGlobalInt(fieldCountProperty, fieldCount);

        if (debugMode) Debug.Log($"Sent {fieldCount} fields to shader.");
    }

    public int ActiveFieldCount() => activeFieldData.Count;
    public int GetMaxFields() => maxFields;
    public List<FieldData> GetActiveFields() => new(activeFieldData);

    void OnDrawGizmos()
    {
        if (debugMode && mainCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(mainCamera.transform.position, cullingDistance);
        }
    }

    void Update()
    {
        if (isDirty)
        {
            RebuildFieldDataArray();
            SendDataToShader();
            isDirty = false;
        }
    }


}
