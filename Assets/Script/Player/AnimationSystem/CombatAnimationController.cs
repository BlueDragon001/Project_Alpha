using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;

/// <summary>
/// [OBSOLETE] Animation controller that uses Unity's Playables API to handle combat animations.
/// This class is marked as obsolete and will be replaced with a new animation system.
/// </summary>
[System.Obsolete("This animation controller is deprecated. Use the new animation system instead.")]
public class CombatAnimationController : MonoBehaviour
{
    public AnimationClips animationClips;
    private Animator animator;

    //Playebles
    private PlayableGraph playableGraph;
    private AnimationMixerPlayable mixerPlayable;
    private AnimationPlayableOutput animationPlayableOutput;
    private Dictionary<InputType, AnimationClipPlayable> animationClipPlayableDictionary;
    private AnimationClipPlayable[] animationClipPlayable;

    void Start()
    {
        animator = GetComponent<Animator>();
        playableGraph = PlayableGraph.Create();
        animationPlayableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
        int totalInputs = System.Enum.GetValues(typeof(InputType)).Length;
        mixerPlayable = AnimationMixerPlayable.Create(playableGraph, totalInputs);
        animationPlayableOutput.SetSourcePlayable(mixerPlayable);
        animationClipPlayable = new AnimationClipPlayable[totalInputs];
        AddAnimationsToMixer();
        playableGraph.Play();

    }

    // public void PlayAnimation(InputType inputType, float transitionDuration = 0.2f)
    // {
    //     if (inputType == InputType.Idle)
    //     {
    //         bool anyOtherAnimationPlaying = false;
    //         for (int i = 0; i < mixerPlayable.GetInputCount(); i++)
    //         {
    //             if (i != (int)InputType.Idle && mixerPlayable.GetInputWeight(i) > 0)
    //             {
    //                 anyOtherAnimationPlaying = true;
    //                 break;
    //             }
    //         }

    //         if (anyOtherAnimationPlaying)
    //         {
    //             return;
    //         }
    //     }

    //     StartCoroutine(BlendToAnimation(inputType, transitionDuration));
    // }

    private InputType currentAnimation = InputType.Idle;
    public void PlayAnimation(InputType inputType, float transitionDuration = 0.2f)
    {

        if (inputType == InputType.Idle)
        {
            bool anyOtherAnimationPlaying = false;
            for (int i = 0; i < mixerPlayable.GetInputCount(); i++)
            {
                if (i != (int)InputType.Idle && mixerPlayable.GetInputWeight(i) > 0) // Buffer added
                {
                    anyOtherAnimationPlaying = true;
                    break;
                }
            }

            if (anyOtherAnimationPlaying)
            {
                return;
            }
        }

        if (currentAnimation == inputType) return; // Prevent redundant calls

        currentAnimation = inputType; // Track active animation
        StartCoroutine(BlendToAnimation(inputType, transitionDuration));
    }

    // private IEnumerator<WaitForSeconds> BlendToAnimation(InputType inputType, float transitionDuration)
    // {
    //     float time = 0f;
    //     float[] initialWeights = new float[mixerPlayable.GetInputCount()];

    //     for (int i = 0; i < initialWeights.Length; i++)
    //     {
    //         initialWeights[i] = mixerPlayable.GetInputWeight(i);
    //     }

    //     while (time < transitionDuration)
    //     {
    //         time += Time.deltaTime;
    //         float blend = time / transitionDuration;

    //         for (int i = 0; i < initialWeights.Length; i++)
    //         {
    //             float targetWeight = (i == (int)inputType) ? 1f : 0f;
    //             mixerPlayable.SetInputWeight(i, Mathf.Lerp(initialWeights[i], targetWeight, blend));
    //         }

    //         yield return null;
    //     }

    //     for (int i = 0; i < initialWeights.Length; i++)
    //     {
    //         float targetWeight = (i == (int)inputType) ? 1f : 0f;

    //         mixerPlayable.SetInputWeight(i, targetWeight);
    //     }
    // }
    private IEnumerator<WaitForSeconds> BlendToAnimation(InputType inputType, float transitionDuration)
    {
        float time = 0f;
        float[] initialWeights = new float[mixerPlayable.GetInputCount()];

        for (int i = 0; i < initialWeights.Length; i++)
        {
            initialWeights[i] = mixerPlayable.GetInputWeight(i);
        }

        // Ensure PlayableGraph keeps running
        mixerPlayable.GetGraph().GetRootPlayable(0).SetDone(false);
        mixerPlayable.GetGraph().GetRootPlayable(0).Play();

        Playable playable = mixerPlayable.GetInput((int)inputType);
        playable.SetTime(0); // Reset animation to start
        playable.Play(); // Ensure it plays

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float blend = time / transitionDuration;

            for (int i = 0; i < initialWeights.Length; i++)
            {
                float targetWeight = (i == (int)inputType) ? 1f : 0f;
                mixerPlayable.SetInputWeight(i, Mathf.Lerp(initialWeights[i], targetWeight, blend));
            }

            yield return null;
        }

        for (int i = 0; i < initialWeights.Length; i++)
        {
            float targetWeight = (i == (int)inputType) ? 1f : 0f;
            mixerPlayable.SetInputWeight(i, targetWeight);
        }

        // Force evaluation
        mixerPlayable.GetGraph().Evaluate();

        // Wait until the animation completes
        while (playable.IsValid() && playable.GetTime() < playable.GetDuration())
        {
            yield return null;
        }

        // Ensure it lands exactly at the last frame
        playable.SetTime(playable.GetDuration());
       // playable.Pause(); // Stop the animation at the last frame
    }



    void AddAnimationsToMixer()
    {
        animationClipPlayableDictionary = new Dictionary<InputType, AnimationClipPlayable>();
        foreach (InputType inputClip in System.Enum.GetValues(typeof(InputType)))
        {
            AnimationClip animationClip = GetAnimationClip(inputClip);
            if (animationClip != null)
            {
                AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);
                animationClipPlayableDictionary.Add(inputClip, animationClipPlayable);
                playableGraph.Connect(animationClipPlayable, 0, mixerPlayable, (int)inputClip);
                mixerPlayable.SetInputWeight((int)inputClip, 0);
            }
        }
    }

    private void OnDestroy()
    {
        playableGraph.Destroy();
    }

    private AnimationClip GetAnimationClip(InputType inputClip)
    {
        return inputClip switch
        {
            InputType.Attack => animationClips.attack1,
            InputType.Jump => animationClips.jump,
            InputType.Move => animationClips.MoveForward,
            InputType.MoveLeft => animationClips.MoveLeft,
            InputType.MoveRight => animationClips.MoveRight,
            InputType.Block => animationClips.block,
            InputType.Idle => animationClips.Idle,
            _ => null
        };
    }
}