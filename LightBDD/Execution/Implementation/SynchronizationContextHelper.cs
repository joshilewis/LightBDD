using System;
using System.Threading;

namespace LightBDD.Execution.Implementation
{
    internal static class SynchronizationContextHelper
    {
        public static void WithSynchronizationContext(SynchronizationContext ctx, Action action)
        {
            var current = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(ctx);
            try
            {
                action();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(current);
            }
        }

        public static T WithSynchronizationContext<T>(SynchronizationContext ctx, Func<T> action)
        {
            var current = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(ctx);
            try
            {
                return action();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(current);
            }
        }
    }
}