
namespace AI.BT.ControlNodes
{
    public class Fallback : Node
    {
        public override Result Tick()
        {
            Result lastChildResult;
            foreach (var child in Children)
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
