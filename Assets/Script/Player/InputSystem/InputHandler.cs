using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Data.Common;
using System.Security.Permissions;

/// <summary>
/// Handles player input and manages the input buffer system for combat actions.
/// Requires PlayerInput, AnimationHandler, PhysicsBasedPlayerController, and ActionHandler components.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(AnimationHandler))]
[RequireComponent(typeof(PhysicsBasedPlayerController))]
[RequireComponent(typeof(ActionHandler))]
public class InputHandler : MonoBehaviour
{
    private InputBuffer inputBuffer = new();
    private Queue<InputBuffer.InputCommand> inputBufferQueue = new();
    private readonly float inputDelay = 0.05f;

    private RingBuffer<InputType> previousActionsBuffer = new(1);

    private AnimationHandler animationHandler;
    private PhysicsBasedPlayerController physicsBasedPlayerController;
    private ActionHandler actionHandler;
    Vector2 moveInput = new Vector2();

    [SerializeField] private InputActionAsset playerControls;
    private InputAction moveAction, attackAction, jumpAction, blockAction, actionModifier;
    bool delayMovement = false;


    void Awake()
    {
        InitActions();
    }
    private void InitActions()
    {
        playerControls.Enable();
        moveAction = playerControls.FindAction("Move");
        attackAction = playerControls.FindAction("Attack");
        jumpAction = playerControls.FindAction("Jump");
        blockAction = playerControls.FindAction("Block");
        //actionModifier = playerControls.FindAction("ActionModifier");
        moveAction.Enable();
        attackAction.Enable();
        jumpAction.Enable();
        blockAction.Enable();
        //actionModifier.Enable();
    }
    void Start()
    {
        animationHandler = GetComponent<AnimationHandler>();
        physicsBasedPlayerController = GetComponent<PhysicsBasedPlayerController>();
        actionHandler = GetComponent<ActionHandler>();
    }

    void Update()
    {
        InputHandlerUpdate();
        inputBuffer.ProcessInputBuffer(ref inputBufferQueue, ExecuteCommand);
    }

    void InputHandlerUpdate()
    {
        if (attackAction.triggered)
        {
            inputBufferQueue.Clear();
            StartCoroutine(RegisterInputWithDelay(InputType.Attack));
        }
        if (moveAction.IsPressed() && !delayMovement)
        {
            moveInput = moveAction.ReadValue<Vector2>();
            StartCoroutine(RegisterInputWithDelay(InputType.Move, moveInput));
        }

        if (jumpAction.triggered)
        {
            StartCoroutine(RegisterInputWithDelay(InputType.Jump));
        }
        if (blockAction.triggered)
        {
            ///  StartCoroutine(RegisterInputWithDelay(InputType.Block));
        }
        else if (!moveAction.IsPressed() && !attackAction.triggered && !jumpAction.triggered && !blockAction.triggered)
        {
            actionHandler.Idle();

        }

    }


    #region Obsolete Input Handling
    /* public void OnAttack(InputAction.CallbackContext context)
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
         Debug.Log("Hello World");

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
 */
    #endregion

    private IEnumerator RegisterInputWithDelay(InputType commandName, Vector2 inputValue = new Vector2())
    {
        yield return new WaitForSeconds(inputDelay);
        inputBufferQueue.Enqueue(new InputBuffer.InputCommand(commandName, Time.time, inputValue));
    }

    private void ExecuteCommand(InputBuffer.InputCommand command = null)
    {
        if (command == null) { return; }


        switch (command.inputType)
        {
            case InputType.Attack:
                float attackTime = actionHandler.Attack();
                StartCoroutine(DelayMovement(attackTime));
                break;
            case InputType.Jump:
                float jumpTime = actionHandler.Jump();
                StartCoroutine(DelayMovement(jumpTime));
                break;
            case InputType.Move:
                if (!delayMovement)
                {
                    Vector2 moveInput = command.inputValue;
                    actionHandler.Move(moveInput);
                }
                break;
            case InputType.Block:
                actionHandler.Block();
                break;
        }
        previousActionsBuffer.Enqueue(command.inputType);
        // Add more command executions as needed
    }

    private void OnDisable()
    {
        moveAction.Disable();
        attackAction.Disable();
        jumpAction.Disable();
        blockAction.Disable();
        // actionModifier.Disable();
    }

    IEnumerator DelayMovement(float time)
    {
        delayMovement = true;
        yield return new WaitForSeconds(time - 0.4f);
        delayMovement = false;
    }







}
