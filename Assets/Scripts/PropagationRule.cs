using System;
using System.Text;
using UnityEditor;
using UnityEngine;

public enum PropagationRuleType
{
    R_3X3 = 0,
    R_5V5 = 1
}

[Serializable]
public class PropagationRule : ScriptableObject, IDisposable
{
    public string name;
    public PropagationRuleType type;
   
    public byte[][] preCondition;
    public byte[][] postCondition;

    public PropagationRule()
    {
        Debug.Log("UH UH");
        UpdateType();
    }

    // Update this rule.
    public void UpdateType()
    {
        DisposeGrid(preCondition);
        DisposeGrid(postCondition);

        switch (type)
        {
            case PropagationRuleType.R_3X3:
            {
                preCondition = new byte[3][];
                postCondition = new byte[3][];
                for (int row = 0; row < 3; row++)
                {
                    preCondition[row] = new byte[3];
                    postCondition[row] = new byte[3];
                }
                break;
            }
            case PropagationRuleType.R_5V5:
            {

                preCondition = new byte[5][];
                postCondition = new byte[5][];
                for (int row = 0; row < 5; row++)
                {
                    preCondition[row] = new byte[5];
                    postCondition[row] = new byte[5];
                }
                break;
            }
        }
    }

    private void DisposeGrid(byte[][] grid)
    {
        if (grid == null)
            return;
        for (int row = 0; row < grid.Length; row++)
            grid[row] = null;
    }

    public override string ToString()
    {
        var middleIndex = Mathf.Floor(preCondition.Length/2);

        var stb= new StringBuilder(name + " (");
        for (int i = 0; i < preCondition.Length; i++)
        {
            for (int j = 0; j < preCondition[i].Length; j++)
            {
                if (i == middleIndex && j == middleIndex)
                    stb.Append("X ,");
                else
                {
                    stb.Append(Grid.TileTypeAbbrevNames[(byte) preCondition[i][j]]);
                    if (j != preCondition[i].Length - 1)
                        stb.Append(",");
                }
            }
            if(i != preCondition.Length - 1)
                stb.Append(" | ");
        }
        stb.Append(")");
        return stb.ToString();
    }

    public void Dispose()
    {
        DisposeGrid(preCondition);
        DisposeGrid(postCondition);
        preCondition = postCondition = null;
    }
}
