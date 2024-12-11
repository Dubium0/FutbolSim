using System;
using System.Collections.Generic;

namespace AI.BT.DecoratorNodes
{
    public class Inverter : Node
    {
        public override Result Tick()
        {
            if(Children.Count > 0)
            {

            var child = Children[0];// excpects 1 children 


            var result = child.Tick();

            if (result != Result.Running) { 
                return (Result)(-(int)result);
            }

            return Result.Running;
            }
            else
            {
                throw new InvalidOperationException("Inverter should have 1 child in order to tick!");
            }

        }
    }
}
