using UnityEngine;

public class SimpleMetaballMover : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float moveRadius = 3f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float moveTimer;
    void Start()
    {
        startPosition = transform.position;
        ChooseNewTarget();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Choose new target when reached
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            ChooseNewTarget();
        }
    }


    private void ChooseNewTarget()
    {
        // Random position within move radius
        Vector3 randomOffset = Random.insideUnitSphere * moveRadius;
        targetPosition = startPosition + randomOffset;
    }
}
