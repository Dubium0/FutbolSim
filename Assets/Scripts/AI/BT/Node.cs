
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        public abstract Result Tick();

        protected Node Parent { get; set; }

        public void SetParent(Node node)
        {
            Parent = node;
        }


        

    }

  

}