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

    private AnimationHandler animationHandler;
    private PhysicsBasedPlayerController physicsBasedPlayerController;

    Vector2 moveInput = new Vector2();

    [SerializeField] private InputActionAsset playerControls;
    private InputAction moveAction, attackAction, jumpAction, blockAction, actionModifier;




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
        //  actionModifier = playerControls.FindAction("ActionModifier");
        moveAction.Enable();
        attackAction.Enable();
        jumpAction.Enable();
        blockAction.Enable();
        //        actionModifier.Enable();
    }
    void Start()
    {
        animationHandler = GetComponent<AnimationHandler>();
        physicsBasedPlayerController = GetComponent<PhysicsBasedPlayerController>();
    }

    void Update()
    {
        inputBuffer.ProcessInputBuffer(inputBufferQueue, ExecuteCommand);
        InputHandlerUpdate();
    }

    void InputHandlerUpdate()
    {
        if (attackAction.triggered)
        {
            StartCoroutine(RegisterInputWithDelay(InputType.Attack));

        }
        else if (moveAction.IsPressed())
        {
            moveInput = moveAction.ReadValue<Vector2>();
            if (moveInput == Vector2.zero) return;
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
            Idle();
            // Debug.Log("hello");
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
                Move(moveInput);
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
        float pauseTime = animationHandler.AttackAnimation();
        StartCoroutine(PauseExecution(pauseTime));

    }

    void Jump()
    {
        animationHandler.JumpAnimation();
        physicsBasedPlayerController.HandleJumpInput();
    }



    void Move(Vector2 Input)
    {
        animationHandler.MoveAnimation(Input);
        physicsBasedPlayerController.HandleMovementInput(Input);
    }

    void Block()
    {

    }

    IEnumerator PauseExecution(float pauseTime)
    {
        yield return new WaitForSeconds(pauseTime);
    }


}
