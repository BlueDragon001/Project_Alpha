using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;

public class InputBuffer : MonoBehaviour
{
    private Queue<InputCommand> inputBuffer;
    private float bufferTime = 0.2f; // Time in seconds to keep inputs in the buffer
    private float inputDelay = 0.05f; // Minor delay before registering input

    void Start()
    {
        inputBuffer = new Queue<InputCommand>();
    }



    
    public void ProcessInputBuffer(Action<InputCommand> EexecuteCommand)
    {
        while (inputBuffer.Count > 0 && Time.time - inputBuffer.Peek().timestamp > bufferTime)
        {
            inputBuffer.Dequeue();
        }

        if (inputBuffer.Count > 0)
        {
            InputCommand command = inputBuffer.Dequeue();
            EexecuteCommand(command);
        }
    }

   
    public class InputCommand
    {
        public string name;
        public float timestamp;

        public InputCommand(string name, float timestamp)
        {
            this.name = name;
            this.timestamp = timestamp;
        }
    }
}
