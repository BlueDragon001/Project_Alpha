
using UnityEngine;
public class PerceptionModule : MonoBehaviour
{
    public float PerceptionRadius = 100f;
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


    public bool IsPlayerInRange(GameObject player)
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance <= PerceptionRadius;
    }
// ************* TODO: Intergrate it with newer StateMachine system *************
    public void IsPlayerInLineOfSight(GameObject player)
    {
        if (IsPlayerInRange(player) == false && StateMachineHandler.enemyState.GetCurrentState() != StateMachineHandler.EnemyState.Dead) return;
        Vector3 directionToPlayer = player.transform.position - transform.position;
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, PerceptionRadius))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                StateMachineHandler.enemyState.ChangeState(StateMachineHandler.EnemyState.Chase);
            }
        }

    }

    private bool IsPlayerInFOV(Vector3 playerDirection)
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        float playerDistance = Vector3.Distance(transform.position, player.transform.position);

        return angle <= 45f && playerDistance <= PerceptionRadius * 100;
    }


    public bool ScanForPlayer()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;

        // Correctly calculate the angle between the forward direction and the direction to the player
        //float yAngle = Vector3.SignedAngle(transform.forward, directionToPlayer, Vector3.up);

        // float t = Mathf.PingPong(Time.time * 2, 1f);
        // float lerpYAngle = Mathf.LerpAngle(yAngle + 45f, yAngle - 45f, t);

        // Quaternion targetRotation = Quaternion.Euler(0, lerpYAngle, 0);

        // Vector3 newDirection = targetRotation * Vector3.forward;

        // Debug.DrawRay(transform.position, Vector3.up * 10, Color.red);
        // Debug.DrawRay(transform.position, newDirection * PerceptionRadius * 100, Color.green, 1f);

        if (IsPlayerInFOV(directionToPlayer) == false) return false;

        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, PerceptionRadius * 100))
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
