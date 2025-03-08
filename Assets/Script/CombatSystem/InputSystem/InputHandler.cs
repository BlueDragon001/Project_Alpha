using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;



[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CombatAnimationController))]
public class InputHandler : MonoBehaviour
{
    private InputBuffer inputBuffer = new();
    private Queue<InputBuffer.InputCommand> inputBufferQueue = new();
    private readonly float inputDelay = 0.05f;

    private RingBuffer<InputType> previousActionsBuffer = new(1);
    private CombatAnimationController combatAnimationController;

    //private bool isRunning = false;
    void Start()
    {
        combatAnimationController = GetComponent<CombatAnimationController>();
    }

    void Update()
    {
        inputBuffer.ProcessInputBuffer(inputBufferQueue, ExecuteCommand);

        if (inputBufferQueue.Count > 0)
        {
            var command = inputBufferQueue.Dequeue();
            if (command != null)
            {
                Debug.Log(command.inputType.ToString());
            }
        }


    }

    // Update is called once per frame
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartCoroutine(RegisterInputWithDelay(InputType.Attack));
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartCoroutine(RegisterInputWithDelay(InputType.Jump));
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Implement movement input handling here
        Vector2 moveInput = context.ReadValue<Vector2>();
        StartCoroutine(RegisterInputWithDelay(InputType.MoveBackward, moveInput));
    }

    public void OnBlock(InputAction.CallbackContext context)
    {
        // Implement block input handling here
        if (context.started) StartCoroutine(RegisterInputWithDelay(InputType.Block));
    }

    public void ActionModifier(InputAction.CallbackContext context)
    {
        // Implement sprint input handling here
        if (context.started)
        {
           
        }
        else if (context.canceled)
        {
            

        }
    }



    private IEnumerator RegisterInputWithDelay(InputType commandName, dynamic inputValue = null)
    {
        yield return new WaitForSeconds(inputDelay);
        inputBufferQueue.Enqueue(new InputBuffer.InputCommand(commandName, Time.time));
    }

    private void ExecuteCommand(InputBuffer.InputCommand command = null)
    {
        if (command == null)
        {
            Idle();
            return;
        }
        var attack = InputType.Null;
        if (previousActionsBuffer.Count > 0)
        {
            attack = previousActionsBuffer.Dequeue();
        }
        switch (command.inputType)
        {
            case InputType.Attack:
                if (attack == InputType.Attack)
                {
                    Attack(true);
                }
                else
                {
                    Attack(false);
                }

                break;
            case InputType.Jump:
                Jump();
                break;
            case InputType.MoveForward:
                Move();
                break;
            case InputType.Block:
                Block();
                break;
        }
        previousActionsBuffer.Enqueue(command.inputType);
        // Add more command executions as needed
    }

    void Idle()
    {
       combatAnimationController.PlayAnimation(InputType.Idle);
    }
    void Attack(bool isConsecutiveAttack)
    {
        combatAnimationController.PlayAnimation(InputType.Attack);
    }

    void Jump()
    {
        
    }

    void Move()
    {
       
    }

    void Block()
    {
        
    }


}
