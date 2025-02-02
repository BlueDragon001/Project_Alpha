using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private InputBuffer inputBuffer = new();
    private Queue<InputBuffer.InputCommand> inputBufferQueue;
    private float inputDelay = 0.05f;

    void Start()
    {
        inputBuffer = GetComponent<InputBuffer>();
    }

    void Update()
    {
        inputBuffer.ProcessInputBuffer(ExecuteCommand);
    }

    // Update is called once per frame
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartCoroutine(RegisterInputWithDelay("Attack"));
        }
    }

    private IEnumerator RegisterInputWithDelay(string commandName)
    {
        yield return new WaitForSeconds(inputDelay);
        inputBufferQueue.Enqueue(new InputBuffer.InputCommand(commandName, Time.time));
    }

    private void ExecuteCommand(InputBuffer.InputCommand command)
    {
        if (command.name == "Attack")
        {
            // Execute attack logic
            Debug.Log("Executing Attack");
        }
        // Add more command executions as needed
    }


}
