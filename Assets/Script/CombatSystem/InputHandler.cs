using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputBuffer))]
[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    private InputBuffer inputBuffer;
    private Queue<InputBuffer.InputCommand> inputBufferQueue;
    private readonly float inputDelay = 0.05f;

    void Start()
    {
        inputBuffer = GetComponent<InputBuffer>();
        inputBufferQueue = new Queue<InputBuffer.InputCommand>();
    }

    void Update()
    {
       // inputBuffer.ProcessInputBuffer(inputBufferQueue, ExecuteCommand);
       Debug.Log("InputBufferQueue Count: " + inputBufferQueue.Count);
    }

    // Update is called once per frame
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartCoroutine(RegisterInputWithDelay(InputType.Attack));
        }
    }

    private IEnumerator RegisterInputWithDelay(InputType commandName)
    {
        yield return new WaitForSeconds(inputDelay);
        inputBufferQueue.Enqueue(new InputBuffer.InputCommand(commandName, Time.time));
    }

    private void ExecuteCommand(InputBuffer.InputCommand command)
    {
        switch (command.inputType)
        {
            case InputType.Attack:
                Debug.Log("Attack command executed.");
                break;
        }
        // Add more command executions as needed
    }


}
