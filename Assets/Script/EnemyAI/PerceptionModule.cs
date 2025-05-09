
using UnityEngine;
public class PerceptionModule  : MonoBehaviour
{
    public float PerceptionRadius = 5f;
    public float DetectionDistance = 10f;
    private GameObject player;


    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found in the scene. Make sure the player has the 'Player' tag.");
        }
    }

    

    public bool IsPlayerInRange()
    {
        if (player == null) return false;
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance <= PerceptionRadius;
    }

    public bool IsPlayerInLineOfSight()
    {
        if (IsPlayerInRange() == false && CombatStateMachine.currentState != CombatState.Dieing) return false;
        Vector3 directionToPlayer = player.transform.position - transform.position;
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, PerceptionRadius))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPlayerInFOV()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        float playerDistance = Vector3.Distance(transform.position, player.transform.position);

        return angle <= 45f && playerDistance <= PerceptionRadius;
    }

    public bool ScanForPlayer()
    {
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float yAngle = Vector3.SignedAngle(transform.position, player.transform.position, Vector3.up);
        float t = Mathf.PingPong(Time.time * 2, 1f);
        float lerpYAngle = Mathf.LerpAngle(yAngle + 45f, yAngle - 45f, t);

        Quaternion targetRotation = Quaternion.Euler(0, lerpYAngle, 0);
        Vector3 newDirection = targetRotation * directionToPlayer;
        if (Physics.Raycast(transform.position, newDirection, out RaycastHit hit, PerceptionRadius))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                Debug.Log("Player detected in FOV!");
                return true;
            }
        }

        return false;
    }

}
