using System.Threading.Tasks;
using LightBDD.Results;

namespace LightBDD.Execution.Implementation
{
    internal interface IStep
    {
        IStepResult GetResult();
        Task Invoke(ExecutionContext context);
        void Comment(ExecutionContext context, string comment);
    }
}