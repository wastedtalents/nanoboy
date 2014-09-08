using System;
using System.Text;
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
    private static readonly Dictionary<AggregateRuleOp, string> _opMappers;
    private static readonly Dictionary<AggregateRuleOp, Func<PropagationRule, byte[][], int, int, bool>> _aggreagateMappers;

    private Func<int, int, bool> _predicate;
    public string name;
    public PropagationRuleType type;
    public RecipientTileType forTileType = RecipientTileType.Dynamic; 
    public AggregateRuleOp ruleOp;
    public IntTuple numbers;
    public bool forLiveCell;

    private float tempf;

    public ByteArrayWrapper[] preCondition;
    
    

    static PropagationRule() {
        _opMappers = new Dictionary<AggregateRuleOp, string>();
        _opMappers.Add(AggregateRuleOp.Equals, "==");
        _opMappers.Add(AggregateRuleOp.Lesser, "<");
        _opMappers.Add(AggregateRuleOp.Greater, ">");
        _opMappers.Add(AggregateRuleOp.LesserEq, "<=");
        _opMappers.Add(AggregateRuleOp.GreaterEq, ">=");
        _opMappers.Add(AggregateRuleOp.Between, "between");

        // mappers
        _aggreagateMappers = new Dictionary<AggregateRuleOp, Func<PropagationRule, byte[][], int, int, bool>>();
        _aggreagateMappers.Add(AggregateRuleOp.Between, (rule, grid, row, col) =>
        {
            var tempf = PropagationRule.SumNeightbours(rule, grid, row, col);
            return tempf >= rule.numbers.value1 && tempf <= rule.numbers.value2;
        });
        _aggreagateMappers.Add(AggregateRuleOp.Equals, (rule, grid, row, col) =>
        {
            var tempf = PropagationRule.SumNeightbours(rule, grid, row, col);
            return tempf == rule.numbers.value1;
        });
        _aggreagateMappers.Add(AggregateRuleOp.Greater, (rule, grid, row, col) =>
        {
            var tempf = PropagationRule.SumNeightbours(rule, grid, row, col);
            return tempf > rule.numbers.value1;
        });
        _aggreagateMappers.Add(AggregateRuleOp.GreaterEq, (rule, grid, row, col) =>
        {
            var tempf = PropagationRule.SumNeightbours(rule, grid, row, col);
            return tempf >= rule.numbers.value1;
        });
        _aggreagateMappers.Add(AggregateRuleOp.Lesser, (rule, grid, row, col) =>
        {
            var tempf = PropagationRule.SumNeightbours(rule, grid, row, col);
            return tempf < rule.numbers.value1;
        });
        _aggreagateMappers.Add(AggregateRuleOp.LesserEq, (rule, grid, row, col) =>
        {
            var tempf = PropagationRule.SumNeightbours(rule, grid, row, col);
            return tempf <= rule.numbers.value1;
        });

    }

    private static int _sum;

    public static int SumNeightbours(PropagationRule rule, byte[][] grid, int row, int col)
    {
        _sum = 0;
        if (rule.type == PropagationRuleType.Aggregate) // 3v3
        {
            for (int i = (row == 0 ? 0 : row - 1); i <= ((row >= grid.Length - 1) ? row : row + 1); i++) {
                            for (int j = (col == 0 ? 0 : col - 1); j <= ((col >= grid[0].Length - 1) ? col : col + 1); j++)
                            {
                                if (i != row || j != col)
                                    _sum += grid[i][j] == 0 ? 0 : 1; // that is to be changed to account for any board element.
                            }
                        }
        }
        else if (rule.type == PropagationRuleType.R_3X3) // 3x3 precondition
        {
            var isFulfilled = true;

            try
            {
                for (int i = (row == 0 ? 0 : row - 1), ii = (row == 0 ? 1 : 0); i <= ((row >= grid.Length - 1) ? row : row + 1); i++, ii++)
                {
                    for (int j = (col == 0 ? 0 : col - 1), jj = (col == 0 ? 1 : 0); j <= ((col >= grid[0].Length - 1) ? col : col + 1); j++, jj++)
                    {
                        if (i != row || j != col)
                        {
                            isFulfilled = isFulfilled && ((grid[i][j] == 0 && rule.preCondition[ii][jj] == 0) ||
                                          (grid[i][j] != 0 && rule.preCondition[ii][jj] != 0)); // to bef ixed.
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            return isFulfilled ? 1 : 0; // specific condition.
        }

        return _sum;

//            if (row > 0)
//            {
//                 if(col > 0)  // [y-1][x-1]
//                    _sum += grid[row - 1][col - 1] == 0 ? 0 : 1;
//                        // poki co dodajemy jeden! ale potem trzebna pod typy jeszcze rozkminke puscic.
//            }

//        }
//        for (int i = (row == 0 ? 0 : row - 1); i <= ((row >= rows - 1) ? row : row + 1); i++)
//        {
//            for (int j = (col == 0 ? 0 : col - 1); j <= ((col >= cols - 1) ? col : col + 1); j++)
//            {
//                if (i != row || j != col)
//                    sum += board[i][j]; // that is to be changed to account for any board element.
//            }
//        }
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

    /// <summary>
    /// Compile the predicate.
    /// </summary>
    public bool IsAlive(byte[][] grid, int row, int col)
    {
        switch (type)
        {
            case PropagationRuleType.Aggregate:
            {
                return _aggreagateMappers[ruleOp](this, grid, row, col);
            }
            case PropagationRuleType.R_3X3:
            {
                var ret =  SumNeightbours(this, grid, row, col) == 1;
                return ret;
            }
        }
        return false;
    }
}
