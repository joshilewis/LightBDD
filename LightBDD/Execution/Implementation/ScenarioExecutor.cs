using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LightBDD.Notification;
using LightBDD.Results;
using LightBDD.Results.Implementation;

namespace LightBDD.Execution.Implementation
{
    internal class ScenarioExecutor : IScenarioExecutor
    {
        private readonly IProgressNotifier _progressNotifier;
        public event Action<IScenarioResult> ScenarioExecuted;

        public ScenarioExecutor(IProgressNotifier progressNotifier)
        {
            _progressNotifier = progressNotifier;
        }

        [DebuggerStepThrough]
        public Task Execute(Scenario scenario, IEnumerable<IStep> steps)
        {
            _progressNotifier.NotifyScenarioStart(scenario.Name, scenario.Label);
            var stepsToExecute = PrepareSteps(scenario, steps);

            var watch = new Stopwatch();
            var scenarioStartTime = DateTimeOffset.UtcNow;
            var lastContext = SynchronizationContext.Current;
            var synchronizationContext = new LightBDDSynchronizationContext(lastContext, new ExecutionContext(_progressNotifier, stepsToExecute.Length));

            watch.Start();

            return ExecuteSteps(synchronizationContext, stepsToExecute)
                .ContinueWith(t =>
                {
                    watch.Stop();
                    var result = new ScenarioResult(scenario.Name, stepsToExecute.Select(s => s.GetResult()), scenario.Label, scenario.Categories)
                    .SetExecutionStart(scenarioStartTime)
                    .SetExecutionTime(watch.Elapsed);

                    if (ScenarioExecuted != null)
                        ScenarioExecuted.Invoke(result);

                    _progressNotifier.NotifyScenarioFinished(result);
                    return t;
                })
                .Unwrap();

        }

        [DebuggerStepThrough]
        private IStep[] PrepareSteps(Scenario scenario, IEnumerable<IStep> steps)
        {
            try
            {
                return steps.ToArray();
            }
            catch (Exception e)
            {
                var result = new ScenarioResult(scenario.Name, new IStepResult[0], scenario.Label, scenario.Categories)
                    .SetFailureStatus(e);

                if (ScenarioExecuted != null)
                    ScenarioExecuted.Invoke(result);

                _progressNotifier.NotifyScenarioFinished(result);
                throw;
            }
        }

        private Task ExecuteSteps(LightBDDSynchronizationContext synchronizationContext, IStep[] stepsToExecute)
        {
            return ExecuteStep(synchronizationContext, stepsToExecute, 0);
        }

        private Task ExecuteStep(LightBDDSynchronizationContext synchronizationContext, IStep[] stepsToExecute, int i)
        {
            if (i >= stepsToExecute.Length)
                return TaskExtensions.CreateCompletedTask();

            return SynchronizationContextHelper.WithSynchronizationContext(synchronizationContext, () =>
             stepsToExecute[i].Invoke(synchronizationContext.ExecutionContext)
                 .ContinueWith(t => (t.Status == TaskStatus.RanToCompletion) ? ExecuteStep(synchronizationContext, stepsToExecute, i + 1) : t)
                 .Unwrap());
        }
    }
}