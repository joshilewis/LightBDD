using System;
using System.Linq;
using System.Threading.Tasks;
using LightBDD.Execution.Exceptions;
using LightBDD.Results;
using LightBDD.Results.Implementation;

namespace LightBDD.Execution.Implementation
{
    internal static class StepHelper
    {
        internal static Task CaptureStepFinalStatus(Task task, StepResult stepResult, Func<Type, ResultStatus> exceptionMapping)
        {
            if (task.Status == TaskStatus.RanToCompletion)
                stepResult.SetStatus(ResultStatus.Passed);
            else if (task.Exception != null)
            {
                var bypass = task.Exception.InnerExceptions.OfType<StepBypassException>().FirstOrDefault();
                if (bypass != null)
                {
                    stepResult.SetStatus(ResultStatus.Bypassed, bypass.Message);
                    return TaskExtensions.CreateCompletedTask();
                }
                var e = task.Exception.InnerException;
                stepResult.SetStatus(exceptionMapping(e.GetType()), e.Message);
            }
            else
                stepResult.SetStatus(ResultStatus.Failed, "Task finished with unexpected reason: " + task.Status);
            return task;
        }
    }
}