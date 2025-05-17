using System.Runtime.Serialization.Configuration;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))] // Ensure this GameObject has a NavMeshAgent component

public class AIMovementModule : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float airControl = 0.3f;
    [SerializeField] private float maxVelocity = 8f;

    [Header("NavMesh Settings")]
    private NavMeshAgent agent; // Reference to the NavMeshAgent component
    private Transform player;
    private Rigidbody rigidBody;


    [SerializeField]
    private float minDistance = 1.2f; // Minimum distance to maintain from the target

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Find the player object by tag
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component attached to this GameObject
        rigidBody = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to this GameObject
    }

    void Update()
    {
        MoveToTarget(player); // Call the MoveToTarget method with the player as the target
    }


    public void MoveToTarget(Transform target)
    {
        float distance = Vector3.Distance(transform.position, target.position); // Calculate the distance to the target

        if (distance > 2f) // If the target is more than 2 units away
        {
            agent.isStopped = false; // Allow the NavMeshAgent to move
            MoveToTargetNavMesh(target); // Move using NavMeshAgent
        }
        else // If the target is within or equal to 2 units
        {
            agent.isStopped = true; // Stop the NavMeshAgent
            // Only move directly if not too close
            if (distance > minDistance)
            {
                MoveToTargetRigidbody(target); // Move using Rigidbody
            }
            else
            {
                // Stop movement to avoid overlap
                rigidBody.linearVelocity = Vector3.zero;
            }
        }

    }

    private void MoveToTargetNavMesh(Transform target)
    {
        if (agent != null && target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    private void MoveToTargetRigidbody(Transform target)
    {
        
        if (rigidBody == null || target == null) return;

        Vector3 targetVelocity = (target.position - transform.position).normalized * moveSpeed;
        targetVelocity.y = rigidBody.linearVelocity.y; // Preserve the vertical velocity
        float currentAccel = moveSpeed > 0 ? acceleration : deceleration;
        Vector3 velocityDiff = targetVelocity - rigidBody.linearVelocity;
        velocityDiff.y = 0f; // Ignore vertical velocity for movement
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rigidBody.linearVelocity, Vector3.up);
        if (horizontalVelocity.magnitude > maxVelocity)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * maxVelocity;
            rigidBody.linearVelocity = new Vector3(limitedVelocity.x, rigidBody.linearVelocity.y, limitedVelocity.z);
            return;
        }
        rigidBody.AddForce(velocityDiff * currentAccel, ForceMode.Acceleration);

    }
}
