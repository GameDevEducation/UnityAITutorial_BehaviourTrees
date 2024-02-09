using System.Collections.Generic;
using System.Text;

public class BTNodeBase : BTElementBase
{
    protected List<BTNodeBase> Children = new();
    protected List<BTDecoratorBase> Decorators = new();
    protected List<BTServiceBase> Services = new();

    protected System.Func<BehaviourTree.ENodeStatus> OnEnterFn;
    protected System.Func<BehaviourTree.ENodeStatus> OnTickFn;

    public BehaviourTree.ENodeStatus LastStatus { get; protected set; } = BehaviourTree.ENodeStatus.Unknown;
    public bool DecoratorsPermitRunning { get; protected set; } = true;

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

    public BTNodeBase AddService<T>(string _Name, System.Action<float> _OnTickFn) where T : BTServiceBase, new()
    {
        T newService = new T();
        newService.Initialise(_Name, _OnTickFn);

        Services.Add(newService);

        return this;
    }

    public BTNodeBase AddService<T>(T newService) where T : BTServiceBase
    {
        Services.Add(newService);

        return this;
    }

    public BTNodeBase AddDecorator<T>(string _Name, System.Func<bool> _OnEvaluateFn) where T : BTDecoratorBase, new()
    {
        T newDecorator = new T();
        newDecorator.Initialise(_Name, _OnEvaluateFn);

        Decorators.Add(newDecorator);

        return this;
    }

    public BTNodeBase AddDecorator<T>(T newDecorator) where T : BTDecoratorBase
    {
        Decorators.Add(newDecorator);

        return this;
    }

    public virtual void Reset()
    {
        LastStatus = BehaviourTree.ENodeStatus.Unknown;

        foreach (var child in Children)
            child.Reset();
    }

    public bool Tick(float deltaTime)
    {
        bool tickedAnyNodes = OnTick(deltaTime);

        // no actions were performed - reset and start over
        if (!tickedAnyNodes)
            Reset();

        return tickedAnyNodes;
    }

    protected virtual void OnEnter()
    {
        if (OnEnterFn != null)
            LastStatus = OnEnterFn.Invoke();
        else
            LastStatus = Children.Count > 0 ? BehaviourTree.ENodeStatus.InProgress : BehaviourTree.ENodeStatus.Succeeded;
    }

    public void TickServices(float deltaTime)
    {
        foreach (var service in Services)
            service.OnTick(deltaTime);
    }

    public bool EvaluateDecorators()
    {
        bool canRun = true;

        foreach (var decorator in Decorators)
        {
            canRun = decorator.Evaluate();

            if (!canRun)
                break;
        }

        if (canRun != DecoratorsPermitRunning)
        {
            DecoratorsPermitRunning = canRun;

            // newly able to run?
            if (canRun)
                Reset();
        }

        return canRun;
    }

    protected virtual void OnAbort()
    {
        Reset();
    }

    protected virtual bool OnTick(float deltaTime)
    {
        bool tickedAnyNodes = false;

        if (!DecoratorsPermitRunning)
        {
            LastStatus = BehaviourTree.ENodeStatus.Failed;
            tickedAnyNodes = true;
            return tickedAnyNodes;
        }

        TickServices(deltaTime);

        // are we entering this node for the first time?
        if (LastStatus == BehaviourTree.ENodeStatus.Unknown)
        {
            OnEnter();
            tickedAnyNodes = true;

            if (LastStatus == BehaviourTree.ENodeStatus.Failed)
                return tickedAnyNodes;
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
        for (int childIndex = 0; childIndex < Children.Count; ++childIndex)
        {
            var child = Children[childIndex];

            bool childPreviouslyEnabledByDecorators = child.DecoratorsPermitRunning;
            bool childCurrentlyEnabledByDecorators = child.EvaluateDecorators();

            // if the child is in progress then early out after ticking
            if (child.LastStatus == BehaviourTree.ENodeStatus.InProgress)
            {
                tickedAnyNodes |= child.Tick(deltaTime);
                return tickedAnyNodes;
            }

            // ignore if the node result has already been recorded
            if (child.LastStatus != BehaviourTree.ENodeStatus.Unknown)
                continue;

            tickedAnyNodes |= child.Tick(deltaTime);

            // inherit the child's status by default
            LastStatus = child.LastStatus;

            if (!childPreviouslyEnabledByDecorators && childCurrentlyEnabledByDecorators)
            {
                for (int futureIndex = childIndex + 1; futureIndex < Children.Count; ++futureIndex)
                {
                    var futureChild = Children[futureIndex];
                    if (futureChild.LastStatus == BehaviourTree.ENodeStatus.InProgress)
                        futureChild.OnAbort();
                    else
                        futureChild.Reset();
                }
            }

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

        foreach (var child in Children)
        {
            debugTextBuilder.AppendLine();
            child.GetDebugTextInternal(debugTextBuilder, indentLevel + 2);
        }
    }
}
