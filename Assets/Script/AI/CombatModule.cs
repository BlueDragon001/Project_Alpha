using UnityEngine;

public class CombatModule : MonoBehaviour
{
    public Transform player;



    void Start()
    {
    }

    void Update()
    {
        if (player == null) return;
        ObservePlayerAndAct();
    }

    private void ObservePlayerAndAct()
    {
        
        int action = Random.Range(0, 3);
        switch (action)
        {
            case 0:
                Attack();
                StateMachineHandler.combatState.ChangeState(StateMachineHandler.CombatState.Attacking);
                break;
            case 1:
                Defend();
                StateMachineHandler.combatState.ChangeState(StateMachineHandler.CombatState.Blocking);
                break;
            case 2:
                Dodge();
                StateMachineHandler.combatState.ChangeState(StateMachineHandler.CombatState.Dodging);
                break;
            case 3:
                StateMachineHandler.combatState.ChangeState(StateMachineHandler.CombatState.Stunned);
                break;
        }
    }

    public void Attack()
    {
        Debug.Log("AI Attack performed");   
    }

    public void Defend()
    {
        Debug.Log("AI Defense performed");
    }

    public void Dodge()
    {
        Debug.Log("AI Dodge performed");
    }
}
