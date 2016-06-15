using UnityEngine;
using System.Collections;

public class HashIDs : MonoBehaviour
{
    public int idleState;
    public int runState;
    public int attackState;

    public int runBool;
    public int attackBool;


    void Awake()
    {
        idleState = Animator.StringToHash("Base Layer.Idle");
        runState = Animator.StringToHash("Base Layer.Run");
        attackState = Animator.StringToHash("Base Layer.Attack");
        runBool = Animator.StringToHash("WalkGo");
        attackBool = Animator.StringToHash("Attacking");

    }
}

