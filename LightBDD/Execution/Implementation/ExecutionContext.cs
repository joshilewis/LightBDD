using System;
using System.Threading;
using LightBDD.Notification;

namespace LightBDD.Execution.Implementation
{
    internal class ExecutionContext
    {
        private IStep _currentStep;

        public ExecutionContext(IProgressNotifier progressNotifier, int totalStepCount)
        {
            ProgressNotifier = progressNotifier;
            TotalStepCount = totalStepCount;
        }

        public IProgressNotifier ProgressNotifier { get; private set; }
        public int TotalStepCount { get; private set; }

        public IStep CurrentStep
        {
            get
            {
                if (_currentStep == null)
                    throw new InvalidOperationException("Currently, no steps are being executed in this context.");
                return _currentStep;
            }
            set { _currentStep = value; }
        }

        public static ExecutionContext Instance
        {
            get
            {
                return LightBDDSynchronizationContext.Current.ExecutionContext;
            }
        }
    }

    internal class LightBDDSynchronizationContext : SynchronizationContext
    {
        public ExecutionContext ExecutionContext { get; private set; }

        public LightBDDSynchronizationContext(ExecutionContext executionContext)
        {
            ExecutionContext = executionContext;
        }

        public new static LightBDDSynchronizationContext Current
        {
            get
            {
                var ctx = SynchronizationContext.Current as LightBDDSynchronizationContext;
                if(ctx==null)
                    throw new InvalidOperationException("Current thread is not executing any scenarios. Please ensure that ExecutionContext is accessed from a running scenario and SynchronizationContext.Current is preserved.");
                return ctx;
            }
        }
    }
}