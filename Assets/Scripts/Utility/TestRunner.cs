
using UnityEngine;
using AI.BT.Tests;
namespace Utility
{
    public class TestRunner :MonoBehaviour
    {
        public ITest test;

       
        public void ExecuteAndPrintTest()
        {
            ITest test = new FallBackNodeTest();

            var result = test.Execute();

            if (result.Success)
            {
                Debug.Log("Test Succeed!");
            }
            else
            {
                Debug.Log("Test Failed!");
                Debug.Log(result.ErrorMessage);
            }
           
        }


    }
}
