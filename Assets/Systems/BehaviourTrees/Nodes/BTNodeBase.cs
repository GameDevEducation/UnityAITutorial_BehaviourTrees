using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BTNodeBase
{
    public string Name { get; protected set; } = "-NO NAME-";

    protected List<BTNodeBase> Children = new List<BTNodeBase>();

    protected System.Func<BehaviourTree.ENodeStatus> OnEnterFn;
    protected System.Func<BehaviourTree.ENodeStatus> OnTickFn;

    public BehaviourTree.ENodeStatus LastStatus { get; protected set; } = BehaviourTree.ENodeStatus.Unknown;

    public BTNodeBase(string _Name = "",
        System.Func<BehaviourTree.ENodeStatus> _OnEnterFn = null,
        System.Func<BehaviourTree.ENodeStatus> _OnTickFn = null)
    {
        Name = _Name;
        OnEnterFn = _OnEnterFn;
        OnTickFn = _OnTickFn;
    }

    public BTNodeBase Add<T>(string _Name,
        System.Func<BehaviourTree.ENodeStatus> _OnEnterFn = null,
        System.Func<BehaviourTree.ENodeStatus> _OnTickFn = null) where T : BTNodeBase, new()
    {
        T newNode = new T();
        newNode.Name = _Name;
        newNode.OnEnterFn = _OnEnterFn;
        newNode.OnTickFn = _OnTickFn;

        return Add(newNode);
    }

    public BTNodeBase Add<T>(T newNode) where T : BTNodeBase
    {
        Children.Add(newNode);

        return newNode;
    }

    public virtual void Reset()
    {
        LastStatus = BehaviourTree.ENodeStatus.Unknown;

        foreach(var child in Children)
            child.Reset();
    }

    public void Tick(float deltaTime)
    {
        bool tickedAnyNodes = OnTick(deltaTime);

        // no actions were performed - reset and start over
        if (!tickedAnyNodes)
            Reset();
    }

    protected virtual void OnEnter()
    {
        if (OnEnterFn != null)
            LastStatus = OnEnterFn.Invoke();
        else
            LastStatus = Children.Count > 0 ? BehaviourTree.ENodeStatus.InProgress : BehaviourTree.ENodeStatus.Succeeded;
    }

    protected virtual bool OnTick(float deltaTime) 
    {
        bool tickedAnyNodes = false;

        // are we entering this node for the first time?
        if (LastStatus == BehaviourTree.ENodeStatus.Unknown)
        {
            OnEnter();
            tickedAnyNodes = true;
        }

        // is there a tick function defined?
        if (OnTickFn != null)
        {
            LastStatus = OnTickFn.Invoke();
            tickedAnyNodes = true;

            // if we succeeded or failed then exit
            if (LastStatus != BehaviourTree.ENodeStatus.InProgress)
                return tickedAnyNodes;
        }

        // are there no children?
        if (Children.Count == 0)
        {
            if (OnTickFn == null)
                LastStatus = BehaviourTree.ENodeStatus.Succeeded;

            return tickedAnyNodes;
        }

        // run the tick on any children
        foreach(var child in Children) 
        { 
            // if the child is in progress then early out after ticking
            if (child.LastStatus == BehaviourTree.ENodeStatus.InProgress)
            {
                tickedAnyNodes |= child.OnTick(deltaTime);
                return tickedAnyNodes;
            }

            // ignore if the node result has already been recorded
            if (child.LastStatus != BehaviourTree.ENodeStatus.Unknown)
                continue;

            tickedAnyNodes |= child.OnTick(deltaTime);

            // inherit the child's status by default
            LastStatus = child.LastStatus;

            if (child.LastStatus == BehaviourTree.ENodeStatus.InProgress)
                return tickedAnyNodes;
            else if (child.LastStatus == BehaviourTree.ENodeStatus.Failed &&
                !ContinueEvaluatingIfChildFailed())
            {
                return tickedAnyNodes;
            }
            else if (child.LastStatus == BehaviourTree.ENodeStatus.Succeeded &&
                !ContinueEvaluatingIfChildSucceeded())
            {
                return tickedAnyNodes;
            }
        }

        OnTickedAllChildren();

        return tickedAnyNodes;
    }

    protected virtual bool ContinueEvaluatingIfChildFailed()
    {
        return true;
    }

    protected virtual bool ContinueEvaluatingIfChildSucceeded()
    {
        return true;
    }

    protected virtual void OnTickedAllChildren()
    {

    }

    public string GetDebugText()
    {
        StringBuilder debugTextBuilder = new StringBuilder();

        GetDebugTextInternal(debugTextBuilder);

        return debugTextBuilder.ToString();
    }

    protected virtual void GetDebugTextInternal(StringBuilder debugTextBuilder, int indentLevel = 0)
    {
        // apply the indent
        for (int index = 0; index < indentLevel; ++index)
            debugTextBuilder.Append(' ');

        debugTextBuilder.Append($"{Name} [{LastStatus.ToString()}]");

        foreach(var child in Children)
        {
            debugTextBuilder.AppendLine();
            child.GetDebugTextInternal(debugTextBuilder, indentLevel + 1);
        }
    }
}
