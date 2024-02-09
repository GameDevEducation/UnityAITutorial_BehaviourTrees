using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTNode_ReturnResult : BTNodeBase
{
    BehaviourTree.ENodeStatus StatusToReturn = BehaviourTree.ENodeStatus.Succeeded;

    public BTNode_ReturnResult(BehaviourTree.ENodeStatus _Status, string _Name = "")
    {
        StatusToReturn = _Status;
        Name = string.IsNullOrEmpty(_Name) ? $"Return {StatusToReturn}" : _Name;
    }

    protected override void OnEnter()
    {
        base.OnEnter();

        LastStatus = StatusToReturn;
    }

    protected override bool OnTick(float deltaTime)
    {
        base.OnTick(deltaTime);

        LastStatus = StatusToReturn;

        return true;
    }
}
