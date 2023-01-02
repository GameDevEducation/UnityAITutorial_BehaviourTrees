using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BTServiceBase : BTElementBase
{
    protected System.Action<float> OnTickFn;

    public void Initialise(string _Name, System.Action<float> _OnTickFn)
    {
        Name = _Name;
        OnTickFn = _OnTickFn;
    }

    public virtual void OnTick(float deltaTime)
    {
        if (OnTickFn != null)
            OnTickFn(deltaTime);
    }

    protected override void GetDebugTextInternal(StringBuilder debugTextBuilder, int indentLevel = 0)
    {
        // apply the indent
        for (int index = 0; index < indentLevel; ++index)
            debugTextBuilder.Append(' ');

        debugTextBuilder.Append($"S: {Name}");
    }

}
