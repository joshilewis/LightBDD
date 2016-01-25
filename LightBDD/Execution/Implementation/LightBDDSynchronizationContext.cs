using System;
using System.Threading;

namespace LightBDD.Execution.Implementation
{
    internal class LightBDDSynchronizationContext : SynchronizationContext
    {
        private readonly SynchronizationContext _previous;
        public ExecutionContext ExecutionContext { get; private set; }

        public LightBDDSynchronizationContext(SynchronizationContext previous, ExecutionContext executionContext)
        {
            _previous = previous;
            ExecutionContext = executionContext;
        }

        public new static LightBDDSynchronizationContext Current
        {
            get
            {
                var ctx = SynchronizationContext.Current as LightBDDSynchronizationContext;
                if (ctx == null)
                    throw new InvalidOperationException("Current thread is not executing any scenarios. Please ensure that ExecutionContext is accessed from a running scenario and SynchronizationContext.Current is preserved.");
                return ctx;
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            if (_previous != null)
                _previous.Post(d, state);
            else
                base.Post(d, state);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (_previous != null)
                _previous.Send(d, state);
            else
                base.Send(d, state);
        }

        public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            return _previous != null
                ? _previous.Wait(waitHandles, waitAll, millisecondsTimeout)
                : base.Wait(waitHandles, waitAll, millisecondsTimeout);
        }
    }
}