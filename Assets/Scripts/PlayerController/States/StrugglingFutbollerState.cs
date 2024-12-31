using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player.Controller.States
{
    public class StrugglingFutbollerState : IPlayerState
    {
        private IFootballAgent controller_;

      
        public StrugglingFutbollerState(IFootballAgent controller)
        {
            controller_ = controller;
           
        }

        public void HandleTransition()
        {
            if(!Football.Instance.IsPlayerStruggling(controller_))
            {
                if(Football.Instance.CurrentOwnerPlayer == controller_)
                {
                    controller_.SetState(new DribblingFutbollerState(controller_));
                }
                else
                {
                    controller_.SetState(new FreeFutbollerState(controller_));
                }
            }
        }

        public void Move()
        {
            throw new NotImplementedException();
        }

        public void OnEnter()
        {
            throw new NotImplementedException();
        }

        public void OnExit()
        {
            throw new NotImplementedException();
        }

        public void OnFixedUpdate()
        {
            throw new NotImplementedException();
        }

        public void OnHighActionAEnter()
        {
            throw new NotImplementedException();
        }

        public void OnHighActionAExit()
        {
            throw new NotImplementedException();
        }

        public void OnHighActionBEnter()
        {
            throw new NotImplementedException();
        }

        public void OnHighActionBExit()
        {
            throw new NotImplementedException();
        }

        public void OnLowActionAEnter()
        {
            throw new NotImplementedException();
        }

        public void OnLowActionAExit()
        {
            throw new NotImplementedException();
        }

        public void OnLowActionBEnter()
        {
            throw new NotImplementedException();
        }

        public void OnLowActionBExit()
        {
            throw new NotImplementedException();
        }

        public void OnSprintEnter()
        {
            // NO ACTION
        }

        public void OnSprintExit()
        {
            throw new NotImplementedException();
        }
    }
}
