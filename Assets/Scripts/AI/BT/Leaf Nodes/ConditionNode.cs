using AI.BT.LeafNodes;
using static UnityEngine.UI.Image;
using System;


namespace Assets.Scripts.AI.BT.LeafNodes
{
    public abstract class ConditionNode : LeafNode
    {
        public override Result Tick()
        {
            var result = ConditionFunc();
            if(result == Result.Running)
            {
                throw new ArgumentException("A Condition Node cannot return Result.Running", nameof(result));
            }
            return result;

        }
        public abstract Result ConditionFunc();

    }
}
