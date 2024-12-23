﻿using AI.BT.ControlNodes;
using Utility;


namespace AI.BT.Tests
{
    public class FallBackNodeTest : ITest
    {
        public ITest.Result Execute()
        {
            ITest.Result result;
            result.ErrorMessage = "";
            result.Success = true;


            Door door = new();
            
            OpenDoor openDoorAction = new(door);
            CloseDoor closeDoorAction = new(door);
            Walk walkAction = new(door);

            Fallback sequence = new();

            sequence.AddChildren(openDoorAction);
            sequence.AddChildren(walkAction);
            sequence.AddChildren(closeDoorAction);

            door.CanOpen = true;
            door.CanClose = true;
            door.LaserOn = false;

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

            if (result2 != Node.Result.Success)
            {
                result.Success = false;
                result.ErrorMessage += $"Second Test Failed!{result2.ToString()}\n";

            }

            door.CanOpen = true;
            door.CanClose = true;
            door.LaserOn = true;

            var result3 = sequence.Tick();

            if (result3 != Node.Result.Success)
            {
                result.Success = false;
                result.ErrorMessage += "Third Test Failed!\n";

            }

            door.CanOpen = true;
            door.CanClose = false;
            door.LaserOn = false;

            var result4 = sequence.Tick();

            if (result4 != Node.Result.Success)
            {
                result.Success = false;
                result.ErrorMessage += "Fourth Test Failed!\n";

            }

            door.CanOpen = false;
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
