using System;
using System.Collections.Generic;

namespace AI.BT.DecoratorNodes
{
    public class Inverter : DecoratorNode
    {
        public override Result Tick()
        {

            var result = child.Tick();

            if (result != Result.Running)
            {
                return (Result)(-(int)result);
            }

            return Result.Running;

        }
    }
}
