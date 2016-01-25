using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LightBDD.Results;

namespace LightBDD.Execution.Implementation
{
    internal interface IScenarioExecutor
    {
        Task Execute(Scenario scenario, IEnumerable<IStep> steps);
        event Action<IScenarioResult> ScenarioExecuted;
    }
}