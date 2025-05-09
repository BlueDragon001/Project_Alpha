using System.Runtime.Serialization.Configuration;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))] // Ensure this GameObject has a NavMeshAgent component

public class AIMovementModule : MonoBehaviour
{
    private NavMeshAgent agent; // Reference to the NavMeshAgent component
    private Transform player;
    private Rigidbody rigidBody;

    [SerializeField]
    private float directMoveSpeed = 5f; // Speed for direct movement using rigidbody

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Find the player object by tag
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component attached to this GameObject
        rigidBody = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to this GameObject
    }


    public void MoveToTarget(Transform target)
    {
       float distance = Vector3.Distance(transform.position, target.position); // Calculate the distance to the target

        if (distance > 2f) // If the target is more than 1 unit away
        {
            MoveToTargetNavMesh(target); // Move using NavMeshAgent
        }
        else// If the target is within 1 unit
        {
            if(distance > 1f){
                MoveToTargetDirect(target); // Move directly using Rigidbody
            } if(distance <= 1f){
                target.position = -target.position; // Move the target to the opposite side of the enemy
                MoveToTargetDirect(target); // Move directly using Rigidbody
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

    private void MoveToTargetDirect(Transform target)
    {
        if (rigidBody != null && target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            rigidBody.MovePosition(transform.position + direction * directMoveSpeed * Time.fixedDeltaTime);

            // Optional: Make the enemy face the target
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 5f);
            }
        }
    }
}
