using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public enum PropagationRuleType
{
    R_3X3 = 0,
    R_5V5 = 1,
    Aggregate
}

public enum AggregateRuleOp {
    Equals,
    Lesser,
    Greater,
    LesserEq,
    GreaterEq,
    Between
}

[Serializable]
public class IntTuple {
    public int value1;
    public int value2;
}

[Serializable]
public class PropagationRule : IDisposable
{
    private static Dictionary<AggregateRuleOp, string> _opMappers;

    public string name;
    public PropagationRuleType type;
    public RecipientTileType forTileType = RecipientTileType.Dynamic; 
    public AggregateRuleOp ruleOp;
    public IntTuple numbers;
    public bool forLiveCell;

    public ByteArrayWrapper[] preCondition;

    static PropagationRule() {
        _opMappers = new Dictionary<AggregateRuleOp, string>();
        _opMappers.Add(AggregateRuleOp.Equals, "==");
        _opMappers.Add(AggregateRuleOp.Lesser, "<");
        _opMappers.Add(AggregateRuleOp.Greater, ">");
        _opMappers.Add(AggregateRuleOp.LesserEq, "<=");
        _opMappers.Add(AggregateRuleOp.GreaterEq, ">=");
        _opMappers.Add(AggregateRuleOp.Between, "between");
    }

    public PropagationRule()
    {
        UpdateType();
    }

    // Update this rule.
    public void UpdateType()
    {
        DisposeGrid(preCondition);

        numbers = new IntTuple();
        switch (type)
        {
            case PropagationRuleType.R_3X3:
            {
                preCondition = new ByteArrayWrapper[3];
                for (int row = 0; row < 3; row++)
                preCondition[row] = new ByteArrayWrapper(3);
                break;
            }
            case PropagationRuleType.R_5V5:
            {
                preCondition = new ByteArrayWrapper[5];
                for (int row = 0; row < 5; row++)
                preCondition[row] = new ByteArrayWrapper(5);
                break;
            }
            case PropagationRuleType.Aggregate: { // 3x3 ale ze zliczaniem.
                preCondition = new ByteArrayWrapper[3];
                for (int row = 0; row < 3; row++)
                preCondition[row] = new ByteArrayWrapper(3);
                break;
            }
        }
    }

    private void DisposeGrid(ByteArrayWrapper[] grid)
    {
        if (grid == null)
        return;
        foreach(var row in preCondition)
        row.Dispose();
    }

    public override string ToString()
    {
        var stb= new StringBuilder(String.Format("{0}[{1}] {2} (", forLiveCell ? "+" : "-",
            ((TileTypeAbbrev)(byte)forTileType).ToString(),
            name));

            if(type == PropagationRuleType.Aggregate) {
                stb.Append(String.Format("neighbours {0} {1}{2}" , _opMappers[ruleOp], numbers.value1, 
                    ruleOp == AggregateRuleOp.Between ? (" and " + numbers.value2) : ""));
            }
            else {

                var middleIndex = Mathf.Floor(preCondition.Length/2);

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

            }
            stb.Append(")");
            return stb.ToString();
        }

        public void Dispose()
        {
            DisposeGrid(preCondition);
            preCondition = null;
        }
    }
