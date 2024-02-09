using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BTNode_Parallel : BTNodeBase
{
    protected bool AlwaysRunSecondary = true;
    protected BTNodeBase PrimaryChild;
    protected BTNodeBase SecondaryChild;

    public new BTNodeBase Add<T>(string _Name,
        System.Func<BehaviourTree.ENodeStatus> _OnEnterFn = null,
        System.Func<BehaviourTree.ENodeStatus> _OnTickFn = null) where T : BTNodeBase, new()
    {
        throw new System.InvalidOperationException("Add is not permitted with BTNode_Parallel. Use SetPrimary or SetSecondary");
    }

    public new BTNodeBase Add<T>(T newNode) where T : BTNodeBase
    {
        throw new System.InvalidOperationException("Add is not permitted with BTNode_Parallel. Use SetPrimary or SetSecondary");
    }

    public BTNodeBase SetPrimary<T>(T newNode) where T : BTNodeBase
    {
        PrimaryChild = newNode;
        return PrimaryChild;
    }

    public BTNodeBase SetSecondary<T>(T newNode) where T : BTNodeBase
    {
        SecondaryChild = newNode;
        return SecondaryChild;
    }

    protected override bool OnTick(float deltaTime)
    {
        bool tickedAnyNodes = false;

        if (!DecoratorsPermitRunning)
        {
            LastStatus = BehaviourTree.ENodeStatus.Failed;
            tickedAnyNodes = true;
            return tickedAnyNodes;
        }

        if (PrimaryChild == null)
        {
            LastStatus = BehaviourTree.ENodeStatus.Failed;
            return false;
        }

        TickServices(deltaTime);

        bool primaryWasEnabled = PrimaryChild.DecoratorsPermitRunning;
        bool primaryIsEnabled = PrimaryChild.EvaluateDecorators();

        // primary child is newly enabled
        if (!primaryWasEnabled && primaryIsEnabled)
            PrimaryChild.Reset();

        if (primaryIsEnabled)
            tickedAnyNodes |= PrimaryChild.Tick(deltaTime);
        else
            LastStatus = BehaviourTree.ENodeStatus.Failed;

        // should we run a secondary and is one present?
        if (AlwaysRunSecondary && (SecondaryChild != null))
        {
            bool secondaryWasEnabled = SecondaryChild.DecoratorsPermitRunning;
            bool secondaryIsEnabled = SecondaryChild.EvaluateDecorators();

            // secondary child is newly enabled
            if (!secondaryWasEnabled && secondaryIsEnabled)
                SecondaryChild.Reset();

            if (secondaryIsEnabled)
                SecondaryChild.Tick(deltaTime);
        }

        LastStatus = PrimaryChild.LastStatus;

        return tickedAnyNodes;
    }

    public override void Reset()
    {
        base.Reset();

        if (PrimaryChild != null)
            PrimaryChild.Reset();
        if (SecondaryChild != null)
            SecondaryChild.Reset();
    }

    public override void GetDebugTextInternal(StringBuilder debugTextBuilder, int indentLevel = 0)
    {
        // apply the indent
        for (int index = 0; index < indentLevel; ++index)
            debugTextBuilder.Append(' ');

        debugTextBuilder.Append($"{Name} [{LastStatus.ToString()}]");

        foreach (var service in Services)
        {
            debugTextBuilder.AppendLine();
            debugTextBuilder.Append(service.GetDebugText(indentLevel + 1));
        }

        foreach (var decorator in Decorators)
        {
            debugTextBuilder.AppendLine();
            debugTextBuilder.Append(decorator.GetDebugText(indentLevel + 1));
        }

        if (PrimaryChild != null)
        {
            debugTextBuilder.AppendLine();
            PrimaryChild.GetDebugTextInternal(debugTextBuilder, indentLevel + 2);
        }
        if (SecondaryChild != null)
        {
            debugTextBuilder.AppendLine();
            SecondaryChild.GetDebugTextInternal(debugTextBuilder, indentLevel + 2);
        }
    }
}
