using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))] // Ensure this GameObject has a NavMeshAgent component

public class AIMovementModule : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float maxVelocity = 8f;
    [SerializeField] private float stoppingDistance = 5f; // Distance to stop from the target
    [SerializeField] private float switchDistance = 1f; // Distance to switch between NavMesh and Rigidbody movement

    [Header("NavMesh Settings")]
    private NavMeshAgent agent; // Reference to the NavMeshAgent component

    [SerializeField]
    private float minDistance = 1.2f; // Minimum distance to maintain from the target

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveToWaypoint(Transform waypoint)
    {
        if (waypoint == null) return;
        MoveToTarget(waypoint);
    }

    public void MoveToTarget(Transform target)
    {
        float distance = Vector3.Distance(transform.position, target.position); // Calculate the distance to the target

        if (distance > stoppingDistance)
        {
            agent.isStopped = false;
            MoveToTargetNavMesh(target);
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath(); // Optional: clear the path to prevent jittering
        }

    }

    private void MoveToTargetNavMesh(Transform target)
    {
        if (agent != null && target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    #region Obsolete MovementLogic
    // private void MoveToTargetRigidbody(Transform target)
    // {

    //     if (rigidBody == null || target == null) return;

    //     Vector3 targetVelocity = (target.position - transform.position).normalized * moveSpeed;
    //     float distance = Vector3.Distance(transform.position, target.position);
    //     targetVelocity.y = rigidBody.linearVelocity.y; // Preserve the vertical velocity
    //     float currentAccel = moveSpeed > 0 ? acceleration : deceleration;
    //     Vector3 velocityDiff = targetVelocity - rigidBody.linearVelocity;
    //     velocityDiff.y = 0f; // Ignore vertical velocity for movement
    //     Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rigidBody.linearVelocity, Vector3.up);
    //     if (horizontalVelocity.magnitude > maxVelocity)
    //     {
    //         Vector3 limitedVelocity = horizontalVelocity.normalized * maxVelocity;
    //         rigidBody.linearVelocity = new Vector3(limitedVelocity.x, rigidBody.linearVelocity.y, limitedVelocity.z);
    //         return;
    //     }

    //     if (distance <= 2f) velocityDiff = -velocityDiff;
    //     else if (distance <= stoppingDistance) { rigidBody.linearVelocity = Vector3.zero; return; }
    //     // if (distance <= 5f)
    //     // {
    //     //     velocityDiff = Vector3.zero;
    //     //     rigidBody.linearVelocity = Vector3.zero;
    //     //     if (distance <= 2f)
    //     // }
    //     rigidBody.AddForce(currentAccel * Time.deltaTime * velocityDiff, ForceMode.Acceleration);
    //     //rigidBody.linearVelocity = Vector3.zero; // Reset the z component of the velocity

    // }
    #endregion
}
