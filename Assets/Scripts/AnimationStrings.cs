using UnityEngine;

public static class AnimationStrings
{
    public static readonly int IsWalking = Animator.StringToHash("isWalking");
    public static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    public static readonly int InputX = Animator.StringToHash("InputX");
    public static readonly int InputY = Animator.StringToHash("InputY");
    public static readonly int LastInputX = Animator.StringToHash("LastInputX");
    public static readonly int LastInputY = Animator.StringToHash("LastInputY");
}