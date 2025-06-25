using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum FieldType
{
    Polynomial = 0,
    Gaussian = 1,
    Exponential = 2
}

[System.Serializable]
public struct FieldData
{
    public int type;
    public float radius;
    public float strength;
    public Vector3 position;

    public float isNegative;

    public FieldData(FieldType fieldType, float r, float s, Vector3 pos, bool isNeg)
    {
        type = (int)fieldType;
        radius = r;
        strength = s;
        position = pos;
        isNegative = isNeg ? -1.0f : 0.0f;
    }

}
public class MetaballFieldContributor : MonoBehaviour
{
    [Header("Field Properties")]
    [SerializeField] private float FieldStrength = 1.0f;
    [SerializeField] private float FieldRadius = 2.0f;
    [SerializeField] private FieldType fieldType = FieldType.Polynomial;
    [SerializeField] private bool isNegative = false;

    [Header("Debug")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = Color.red;

    private Transform cachedTransform;
    private Vector3 lastPosition;

    public float FieldStrengthValue => FieldStrength;
    public float FieldRadiusValue => FieldRadius;
    public FieldType FieldTypeValue => fieldType;
    public Vector3 Position => cachedTransform.position;
    public bool IsNegative => isNegative;
    public bool HasMoved => lastPosition != Position;
    void Awake()
    {
        cachedTransform = transform;
        lastPosition = Position;

    }

    void Onable()
    {
        MetaballManager.Instance?.RegisterField(this);
    }
    void OnDisable()
    {
        MetaballManager.Instance?.UnregisterField(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (HasMoved)
        {
            MetaballManager.Instance?.MakeDirty();
            lastPosition = Position;
        }
    }

    public FieldData GetFieldData()
    {
        return new FieldData(fieldType, FieldRadius, FieldStrength, Position, isNegative);
    }

    private void OnValidate()
    {
        FieldStrength = Mathf.Max(0.0f, FieldStrength);
        FieldRadius = Mathf.Max(0.0f, FieldRadius);
        if( Application.isPlaying && MetaballManager.Instance != null) MetaballManager.Instance.MakeDirty();


    }
    
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = isNegative ? Color.red : gizmoColor;
            Gizmos.DrawWireSphere(cachedTransform.position, FieldRadius);
             Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawSphere(cachedTransform.position, FieldRadius * 0.1f);
        }
    }
}
