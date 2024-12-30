
using BT_Implementation;
using BT_Implementation.Control;

public class MidfieldAIFacade : BTRoot
{
    private BTNode entryPoint = new NullBTNode();
    public MidfieldAIFacade(Blackboard blackBoard) : base(blackBoard)
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
