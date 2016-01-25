using System;
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
}