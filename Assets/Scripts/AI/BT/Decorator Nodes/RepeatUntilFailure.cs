
namespace AI.BT.DecoratorNodes
{
    public class RepeatUntilFailure : DecoratorNode
    {
        public override Result Tick()
        {
            var result = child.Tick();

            if (result != Result.Failure) {

                return Result.Running;
            }

            return Result.Failure;
        }
    }
}
