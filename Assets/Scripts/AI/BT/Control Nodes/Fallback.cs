
namespace AI.BT.ControlNodes
{
    public class Fallback : ControlNode
    {
        public override Result Tick()
        {
            Result lastChildResult;
            foreach (var child in children)
            {
                lastChildResult = child.Tick();

                if (lastChildResult != Result.Failure)
                {
                    return lastChildResult;
                }
            }
            return Result.Failure;
        }
    }
}
