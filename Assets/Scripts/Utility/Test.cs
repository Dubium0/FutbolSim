
namespace Utility
{
    public interface ITest 
    {
        public struct Result {
            public string ErrorMessage;
            public bool Success;
        }
        public Result Execute();



    }
}
