using System.Collections.Generic;
namespace AI.BT
{
    

    public abstract class Node
    {
        public enum Result
        {
            Failure =-1,
            Running = 0,
            Success = 1,
        };

        private Node parent_ = null;
        private List<Node> children_ = new ();

        public Node Parent { get => parent_; }
        public List<Node> Children { get => children_; }

        public abstract Result Tick();

        public void RemoveChildren(Node child)
        {
            children_.Remove(child);//
        }

        public void AddChildren(Node child)
        {
            children_.Add(child);
            child.SetParent(this);
        }
        
        public void SetParent(Node parent)
        {
            if (parent_ != null)
            {
                parent_.RemoveChildren(this);
            }
            parent_ = parent;
        }

    }

  

}