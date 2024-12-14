

namespace AI.BT.DecoratorNodes
{
    public abstract class DecoratorNode : Node
    {

        protected Node child;

        public void SetChild(Node child)
        {
            this.child.SetParent(null);
            this.child = child;
            this.child.SetParent(this);
        }

    }
}
