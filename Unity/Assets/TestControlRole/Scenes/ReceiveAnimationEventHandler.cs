using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AnimationEventName
{
    [InspectorName("GH安徽")]
    FootR,
    FootL,
    Hit
}

public class ReceiveAnimationEventHandler : MonoBehaviour
{
    public bool beAttack;
    public AnimationEventName tmp;

    private void Awake()
    {
        int[] data = { 1, 2, 3, 4, 5 };
        Span<int> slice = data.AsSpan(1, 3); // 不复制，不分配
        
        foreach (var x in slice)
        {
            Debug.Log(x);
            // dWriteLine(x);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Attacker")
        {
            beAttack = true;
        }
    }

    public void FootR(AnimationEvent evt)
    {
        Debug.Log("Animation Event" + evt.functionName);
    }

    public void FootL(AnimationEvent evt)
    {
        Debug.Log("Animation Event" + evt.functionName);
    }

    public void Hit(AnimationEvent evt)
    {
        Debug.Log("Animation Event" + evt.functionName);
    }
}