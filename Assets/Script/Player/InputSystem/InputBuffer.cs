using UnityEngine;
using System.Collections.Generic;
using System;
/// <summary>
/// Manages a buffer of input commands to ensure smooth and responsive input handling.
/// </summary>
public class InputBuffer
{

    private readonly float bufferTime = 0.01f; // Time in seconds to keep inputs in the buffer

    /// <summary>
    /// Processes the input buffer, executing commands within the buffer time window.
    /// Commands older than bufferTime are automatically removed.
    /// </summary>
    /// <param name="inputBuffer">Queue containing input commands</param>
    /// <param name="EexecuteCommand">Callback to execute when processing a command</param>
    public void ProcessInputBuffer(ref Queue<InputCommand> inputBuffer, Action<InputCommand> EexecuteCommand)
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
        else if (inputBuffer.Count == 0)
        {
            InputCommand command = null;
            EexecuteCommand(command);
        }
    }

    /// <summary>
    /// Represents a single input command with its associated data.
    /// Stores the input type, timestamp, and any additional input values.
    /// </summary>
    public class InputCommand
    {
        public string name;
        public float timestamp;
        public InputType inputType;
        public Vector2 inputValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputCommand"/> class.
        /// </summary>
        /// <param name="name">The name of the input command.</param>
        /// <param name="timestamp">The time at which the command was issued.</param>
        public InputCommand(InputType inputType, float timestamp, Vector2 inputValue = new Vector2())
        {
            this.inputType = inputType;
            this.timestamp = timestamp;
            this.inputValue = inputValue;
        }
    }
}
