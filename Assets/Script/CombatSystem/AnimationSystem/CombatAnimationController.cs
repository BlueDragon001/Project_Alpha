using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;

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

    public void PlayAnimation(InputType inputType, float transitionDuration = 0.2f)
    {
        StartCoroutine(BlendToAnimation(inputType, transitionDuration));
    }

    private IEnumerator<WaitForSeconds> BlendToAnimation(InputType inputType, float transitionDuration)
    {
        float time = 0f;
        float[] initialWeights = new float[mixerPlayable.GetInputCount()];

        for (int i = 0; i < initialWeights.Length; i++)
        {
            initialWeights[i] = mixerPlayable.GetInputWeight(i);
        }

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float blend = time / transitionDuration;

            for (int i = 0; i < initialWeights.Length; i++)
            {
                float targetWeight = (i == (int)inputType) ? 1f : 0f;
                if (inputType == InputType.Idle && initialWeights[i] > 0)
                {
                    targetWeight = initialWeights[i];
                }
                mixerPlayable.SetInputWeight(i, Mathf.Lerp(initialWeights[i], targetWeight, blend));
            }

            yield return null;
        }

        for (int i = 0; i < initialWeights.Length; i++)
        {
            float targetWeight = (i == (int)inputType) ? 1f : 0f;
            if (inputType == InputType.Idle && initialWeights[i] > 0)
            {
                targetWeight = initialWeights[i];
            }
            mixerPlayable.SetInputWeight(i, targetWeight);
        }
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
            InputType.MoveForward => animationClips.MoveForward,
            InputType.MoveBackward => animationClips.MoveBackward,
            InputType.MoveLeft => animationClips.MoveLeft,
            InputType.MoveRight => animationClips.MoveRight,
            InputType.Block => animationClips.block,
            InputType.Idle => animationClips.Idle,
            _ => null
        };
    }
}