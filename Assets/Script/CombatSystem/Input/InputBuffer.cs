using UnityEngine;
using System.Collections.Generic;
using System;
using Mono.Cecil.Cil;

/// <summary>
/// Manages a buffer of input commands to ensure smooth and responsive input handling.
/// </summary>
public class InputBuffer
{
    
    private readonly float bufferTime = 0.2f; // Time in seconds to keep inputs in the buffer
    
    /// <summary>
    /// Processes the input buffer, executing the oldest command if it is still within the buffer time.
    /// </summary>
    /// <param name="EexecuteCommand">The action to execute with the dequeued input command.</param>
    public void ProcessInputBuffer( Queue<InputCommand> inputBuffer, Action<InputCommand> EexecuteCommand)
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

    /// <summary>
    /// Represents an input command with a name and timestamp.
    /// </summary>
    public class InputCommand
    {
        public string name;
        public float timestamp;
        public InputType inputType;
        public dynamic inputValue;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="InputCommand"/> class.
        /// </summary>
        /// <param name="name">The name of the input command.</param>
        /// <param name="timestamp">The time at which the command was issued.</param>
        public InputCommand(InputType inputType, float timestamp, dynamic inputValue = null)
        {
            this.inputType = inputType;
            this.timestamp = timestamp;
            this.inputValue = inputValue;
        }
    }
}
