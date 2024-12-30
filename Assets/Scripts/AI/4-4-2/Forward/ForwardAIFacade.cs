
using BT_Implementation;
using BT_Implementation.Control;

public class ForwardAIFacade : BTRoot
{
    private BTNode entryPoint = new NullBTNode();
    public ForwardAIFacade(Blackboard blackBoard) : base(blackBoard)
    {
    }

    public override void ConstructBT()
    {
    
    }

    public override void ExecuteBT()
    {
        entryPoint.Execute();
    }

}
