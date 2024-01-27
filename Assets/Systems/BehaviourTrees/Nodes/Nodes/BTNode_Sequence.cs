public class BTNode_Sequence : BTNodeBase
{
    protected override bool ContinueEvaluatingIfChildFailed()
    {
        return false;
    }

    protected override bool ContinueEvaluatingIfChildSucceeded()
    {
        return true;
    }

    protected override void OnTickedAllChildren()
    {
        LastStatus = Children[^1].LastStatus;
    }
}
