
using BT_Implementation;
using BT_Implementation.Control;

public class DefenseAIFacade : BTRoot
{
    private BTNode entryPoint = new NullBTNode();
    public DefenseAIFacade(Blackboard blackBoard) : base(blackBoard)
    {
    }

    public override void ConstructBT()
    {
        SequenceNode defenseSequence = new SequenceNode("Defense Sequence");

        defenseSequence.AddChild(new ActiveDefense("Active Defense", blackBoard));

        ////////////////////////////////
        entryPoint = defenseSequence;

    }

    public override void ExecuteBT()
    {
        entryPoint.Execute();
    }
}
