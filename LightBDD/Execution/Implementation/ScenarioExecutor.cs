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
        public async Task Execute(Scenario scenario, IEnumerable<IStep> steps)
        {
            _progressNotifier.NotifyScenarioStart(scenario.Name, scenario.Label);
            var stepsToExecute = PrepareSteps(scenario, steps);

            var watch = new Stopwatch();
            var scenarioStartTime = DateTimeOffset.UtcNow;
            var lastContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(new LightBDDSynchronizationContext(new ExecutionContext(_progressNotifier, stepsToExecute.Length)));
                watch.Start();
                await ExecuteSteps(stepsToExecute);
            }
            finally
            {
                watch.Stop();
                SynchronizationContext.SetSynchronizationContext(lastContext);
                var result = new ScenarioResult(scenario.Name, stepsToExecute.Select(s => s.GetResult()), scenario.Label, scenario.Categories)
                .SetExecutionStart(scenarioStartTime)
                .SetExecutionTime(watch.Elapsed);

                if (ScenarioExecuted != null)
                    ScenarioExecuted.Invoke(result);

                _progressNotifier.NotifyScenarioFinished(result);
            }
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

        private async Task ExecuteSteps(IStep[] stepsToExecute)
        {
            foreach (var step in stepsToExecute)
               await step.Invoke(ExecutionContext.Instance);
        }
    }
}