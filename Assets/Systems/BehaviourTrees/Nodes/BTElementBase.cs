using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class BTElementBase
{
    public string Name { get; protected set; } = "-NO NAME-";

    public string GetDebugText(int indentLevel = 0)
    {
        StringBuilder debugTextBuilder = new StringBuilder();

        GetDebugTextInternal(debugTextBuilder, indentLevel);

        return debugTextBuilder.ToString();
    }

    protected abstract void GetDebugTextInternal(StringBuilder debugTextBuilder, int indentLevel = 0);
}
