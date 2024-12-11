using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.BT.ControlNodes
{
  
    public class Sequence : Node
    {
        public override Result Tick()
        {
            Result lastChildResult;
            foreach (var child in Children) {
                lastChildResult = child.Tick();

                if (lastChildResult != Result.Success) { 
                
                    return lastChildResult;
                }
            }
            return Result.Success;


        }
    }
}
