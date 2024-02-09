public class BTNode_Action : BTNodeBase
{
    public BTNode_Action(string _Name = "",
        System.Func<BehaviourTree.ENodeStatus> _OnEnterFn = null,
        System.Func<BehaviourTree.ENodeStatus> _OnTickFn = null) :
        base(_Name, _OnEnterFn, _OnTickFn)
    {

    }
}
