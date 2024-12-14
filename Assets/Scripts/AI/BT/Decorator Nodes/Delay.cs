
using UnityEngine;

namespace AI.BT.DecoratorNodes
{
    public class Delay : DecoratorNode
    {
        public float DelaySeconds = 0;

        private float startTime_ = -1.0f;
        public override Result Tick()
        {
            if (startTime_< 0)
            {
                startTime_ = Time.time;
            }

            if (startTime_ + DelaySeconds < Time.time) { 
                
                var result = child.Tick();  
                return result;
            }
            return Result.Running;
        }
    }
}
