using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : MonoBehaviour
{
    public enum ENodeStatus
    {
        Unknown,
        InProgress,
        Failed,
        Succeeded
    }

    public BTNodeBase RootNode { get; private set; } = new BTNodeBase("ROOT");

    // Start is called before the first frame update
    void Start()
    {
        RootNode.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        RootNode.Tick(Time.deltaTime);
    }

    public string GetDebugText()
    {
        return RootNode.GetDebugText();
    }
}
