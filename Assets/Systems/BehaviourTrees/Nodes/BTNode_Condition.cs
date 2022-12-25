using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTNode_Condition : BTNodeBase
{
    protected System.Func<bool> ConditionFn;
    protected bool WasPreviouslyAbleToRun = false;

    public BTNode_Condition(string _Name, System.Func<bool> _ConditionFn) :
        base(_Name)
    {
        ConditionFn = _ConditionFn;
        OnEnterFn = EvaluateCondition;
        OnTickFn = EvaluateCondition;
    }

    protected BehaviourTree.ENodeStatus EvaluateCondition()
    {
        bool canRun = ConditionFn != null ? ConditionFn.Invoke() : false;

        if (canRun != WasPreviouslyAbleToRun)
        {
            WasPreviouslyAbleToRun = canRun;

            foreach (var child in Children)
                child.Reset();
        }

        return canRun ? BehaviourTree.ENodeStatus.InProgress : BehaviourTree.ENodeStatus.Failed;
    }
}
