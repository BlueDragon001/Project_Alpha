using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;




[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(AnimationHandler))]
[RequireComponent(typeof(PhysicsBasedPlayerController))]
public class InputHandler : MonoBehaviour
{
    private InputBuffer inputBuffer = new();
    private Queue<InputBuffer.InputCommand> inputBufferQueue = new();
    private readonly float inputDelay = 0.05f;

    private RingBuffer<InputType> previousActionsBuffer = new(1);
    private CombatAnimationController combatAnimationController;

    private AnimationHandler animationHandler;
    private PhysicsBasedPlayerController physicsBasedPlayerController;

    //private bool isRunning = false;

    Vector2 moveInput = new Vector2();

    void Start()
    {
        animationHandler = GetComponent<AnimationHandler>();
        physicsBasedPlayerController = GetComponent<PhysicsBasedPlayerController>();
    }

    void Update()
    {
        inputBuffer.ProcessInputBuffer(inputBufferQueue, ExecuteCommand);
        if (moveInput != Vector2.zero)
        {
            Move();
        }
        else
        {

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
        StartCoroutine(RegisterInputWithDelay(InputType.Move, moveInput));
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



    private IEnumerator RegisterInputWithDelay(InputType commandName, Vector2 inputValue = new Vector2())
    {
        yield return new WaitForSeconds(inputDelay);
        inputBufferQueue.Enqueue(new InputBuffer.InputCommand(commandName, Time.time, inputValue));
    }

    private void ExecuteCommand(InputBuffer.InputCommand command = null)
    {
        if (command == null) { Idle(); return; }

        switch (command.inputType)
        {
            case InputType.Attack:
                Attack();
                break;
            case InputType.Jump:
                Jump();
                break;
            case InputType.Move:
                Vector2 moveInput = command.inputValue;
                MoveHandler(moveInput);
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
        animationHandler.Idle();

    }
    void Attack()
    {
        animationHandler.AttackAnimation();
    }

    void Jump()
    {
        animationHandler.JumpAnimation();
        physicsBasedPlayerController.HandleJumpInput();
    }

    void MoveHandler(Vector2 moveInput) { this.moveInput = moveInput; }


    void Move()
    {
        animationHandler.MoveAnimation(moveInput);
        physicsBasedPlayerController.HandleMovementInput(moveInput);
    }

    void Block()
    {

    }


}
