using AI.BT.ControlNodes;
using System.Diagnostics;
using UnityEngine;
using Utility;

namespace AI.BT.Tests
{

    internal class Door
    {
        public bool CanOpen;
        public bool LaserOn;
        public bool CanClose;
    }
    internal class OpenDoor : Node
    {

        private Door door_;
        public OpenDoor(Door door)
        {
            door_ = door;
        }
        public override Result Tick()
        {
            if (door_.CanOpen)
            {
                return Result.Success;
            }
            else
            {
                return Result.Failure;
            }

        }
    }
    internal class CloseDoor : Node
    {

        private Door door_;
        public CloseDoor(Door door)
        {
            door_ = door;
        }
        public override Result Tick()
        {
            if (door_.CanClose)
            {
                return Result.Success;
            }
            else
            {
                return Result.Failure;
            }

        }
    }
    internal class Walk : Node
    {

        private Door door_;
        public Walk(Door door)
        {
            door_ = door;
        }
        public override Result Tick()
        {
            if (door_.LaserOn)
            {
                return Result.Failure;
            }
            else
            {
                return Result.Success;
            }

        }
    }

 
    public class SequenceNodeTest :  ITest
    {
        public ITest.Result Execute()
        {
            ITest.Result result;
            result.ErrorMessage = "";
            result.Success = true;


            Door door = new();
            door.CanOpen = true;
            door.CanClose = true;
            door.LaserOn = false;
            OpenDoor openDoorAction = new(door);
            CloseDoor closeDoorAction = new(door);
            Walk walkAction = new(door);

            Sequence sequence = new Sequence();

            sequence.AddChildren(openDoorAction);
            sequence.AddChildren(walkAction);
            sequence.AddChildren(closeDoorAction);

            var result1 = sequence.Tick();

            if (result1 != Node.Result.Success)
            {
                result.Success = false;
                result.ErrorMessage += "First Test Failed!\n";

            }

            door.CanOpen = false;
            door.CanClose = true;
            door.LaserOn = false;

            var result2 = sequence.Tick();

            if (result2 != Node.Result.Failure)
            {
                result.Success = false;
                result.ErrorMessage += $"Second Test Failed!{result2.ToString()}\n";

            }

            door.CanOpen = true;
            door.CanClose = true;
            door.LaserOn = true;

            var result3 = sequence.Tick();

            if (result3 != Node.Result.Failure)
            {
                result.Success = false;
                result.ErrorMessage += "Third Test Failed!\n";

            }

            door.CanOpen = true;
            door.CanClose = false;
            door.LaserOn = false;

            var result4 = sequence.Tick();

            if (result4 != Node.Result.Failure)
            {
                result.Success = false;
                result.ErrorMessage += "Fourth Test Failed!\n";

            }

            door.CanOpen = true;
            door.CanClose = false;
            door.LaserOn = true;

            var result5 = sequence.Tick();

            if (result5 != Node.Result.Failure)
            {
                result.Success = false;
                result.ErrorMessage += "Fifth Test Failed!\n";

            }
            return result;

        }
        

    }
}
