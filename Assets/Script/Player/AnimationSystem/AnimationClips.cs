using UnityEngine;

[CreateAssetMenu(fileName = "AnimationHandler", menuName = "Scriptable Objects/AnimationHandler")]
public class AnimationClips : ScriptableObject
{
    public AnimationClip Idle;
    public AnimationClip attack1;
    public AnimationClip attack2;
    public AnimationClip sweepAttack;
    public AnimationClip block;
    public AnimationClip jump;

    public AnimationClip MoveForward;
    public AnimationClip MoveBackward; 
    public AnimationClip MoveLeft;
    public AnimationClip MoveRight;
}
