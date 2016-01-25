using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LightBDD.Results;
using LightBDD.Results.Implementation;

namespace LightBDD.Execution.Implementation
{
    [DebuggerStepThrough]
    internal class Step : IStep
    {
        private readonly Func<Task> _action;
        private readonly Func<Type, ResultStatus> _mapping;
        private readonly StepResult _result;
        public IStepResult GetResult() { return _result; }

        public Step(Func<Task> action, string stepTypeName, string stepName, int stepNumber, Func<Type, ResultStatus> mapping)
        {
            _action = action;
            _mapping = mapping;
            _result = new StepResult(stepNumber, new StepName(stepName, stepTypeName), ResultStatus.NotRun);
        }

        public Task Invoke(ExecutionContext context)
        {
            context.CurrentStep = this;
            context.ProgressNotifier.NotifyStepStart(_result.Name, _result.Number, context.TotalStepCount);
            _result.SetExecutionStart(DateTimeOffset.UtcNow);

            return MeasuredInvoke().ContinueWith(t =>
            {
                context.CurrentStep = null;
                context.ProgressNotifier.NotifyStepFinished(_result, context.TotalStepCount);
                return StepHelper.CaptureStepFinalStatus(t, _result, _mapping);
            }).Unwrap();
        }

        public void Comment(ExecutionContext context, string comment)
        {
            _result.AddComment(comment);
            context.ProgressNotifier.NotifyStepComment(_result.Number, context.TotalStepCount, comment);
        }

        private Task MeasuredInvoke()
        {
            var watch = new Stopwatch();
            _result.SetExecutionStart(DateTimeOffset.UtcNow);
            watch.Start();

            return _action().ContinueWith(t =>
            {
                _result.SetExecutionTime(watch.Elapsed);
                return t;
            }).Unwrap();
        }

        public override string ToString()
        {
            return _result.ToString();
        }
    }
}