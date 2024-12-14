using System.Collections.Generic;
using static Unity.VisualScripting.Metadata;

namespace AI.BT.ControlNodes
{
   
    public abstract class ControlNode: Node
    {

        protected List<Node> children = new();

        public void RemoveChildren(Node child)
        {
            children.Remove(child);
            child.SetParent(null);
        }

        public void AddChildren(Node child)
        {
            children.Add(child);
            child.SetParent(this);
           
        }

       

       


    }
}
