using System.Text;

public class BTDecoratorBase : BTElementBase
{
    protected System.Func<bool> OnEvaluateFn;
    public bool LastEvaluationResult { get; protected set; }

    public void Initialise(string _Name, System.Func<bool> _OnEvaluateFn)
    {
        Name = _Name;
        OnEvaluateFn = _OnEvaluateFn;
    }

    public virtual bool Evaluate()
    {
        LastEvaluationResult = OnEvaluateFn != null ? OnEvaluateFn() : false;

        return LastEvaluationResult;
    }

    protected override void GetDebugTextInternal(StringBuilder debugTextBuilder, int indentLevel = 0)
    {
        // apply the indent
        for (int index = 0; index < indentLevel; ++index)
            debugTextBuilder.Append(' ');

        debugTextBuilder.Append($"D: {Name} [{(LastEvaluationResult ? "PASS" : "FAIL")}]");
    }
}
