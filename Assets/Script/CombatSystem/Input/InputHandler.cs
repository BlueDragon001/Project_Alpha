using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CombatAnimation))]
[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    private InputBuffer inputBuffer;
    private Queue<InputBuffer.InputCommand> inputBufferQueue;
    private readonly float inputDelay = 0.05f;

    private RingBuffer<InputType> previousActionsBuffer = new(2);
    private CombatAnimation combatAnimation;

    private bool isRunning = false;
    private bool isBlocking = false;
    void Start()
    {
        // inputBuffer = GetComponent<InputBuffer>();
        // inputBufferQueue = new Queue<InputBuffer.InputCommand>();
        combatAnimation = GetComponent<CombatAnimation>();
    }

    void Update()
    {
        inputBuffer.ProcessInputBuffer(inputBufferQueue, ExecuteCommand);
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
        StartCoroutine(RegisterInputWithDelay(InputType.Move, moveInput));
    }

    public void OnBlock(InputAction.CallbackContext context)
    {
        // Implement block input handling here
        if(context.started) StartCoroutine(RegisterInputWithDelay(InputType.Block));
    }

    public void ActionModifier(InputAction.CallbackContext context)
    {
        // Implement sprint input handling here
        if (context.started)
        {
            isRunning = true;
        }else if (context.canceled)
        {
            isRunning = false;
            
        }
    }



    private IEnumerator RegisterInputWithDelay(InputType commandName, dynamic inputValue = null)
    {
        yield return new WaitForSeconds(inputDelay);
        inputBufferQueue.Enqueue(new InputBuffer.InputCommand(commandName, Time.time));
    }

    private void ExecuteCommand(InputBuffer.InputCommand command)
    {
        var attack = previousActionsBuffer.Dequeue();
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
            case InputType.Move:
                Move();
                break;
            case InputType.Block:
                Block();
                break;
        }
        previousActionsBuffer.Enqueue(command.inputType);
        // Add more command executions as needed
    }

    void Attack(bool isConsecutiveAttack)
    {
        if (isConsecutiveAttack) combatAnimation.PlayConsecutiveAttackAniamtion();
        else combatAnimation.PlayAttackAnimation();
    }

    void Jump()
    {
        combatAnimation.PlayJumpAttackAniamtion();
    }

    void Move()
    {
        if(isRunning)
        {
            combatAnimation.PlayRunAnimation(true);
        }
        else
        {
            combatAnimation.PlayWalkAnimation(true);
        }
    }

    void Block()
    {
        combatAnimation.PlayBlockAnimation();
    }


}
