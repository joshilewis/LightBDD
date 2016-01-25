using System;
using System.Reflection;
using System.Threading.Tasks;

namespace LightBDD.Execution.Implementation
{
    internal static class TaskExtensions
    {
        public static void Await(this Task task)
        {
            if (!task.IsCompleted)
            {
                try
                {
                    task.Wait();
                }
                catch { }
            }
            if(task.IsCanceled)
                throw new TaskCanceledException(task);
            if (task.IsFaulted)
                throw PreserveStackTrace(task.Exception.InnerException);
        }

        public static Task CreateCompletedTask()
        {
            var completionSource = new TaskCompletionSource<int>();
            completionSource.SetResult(0);
            return completionSource.Task;
        }

        private static Exception PreserveStackTrace(Exception ex)
        {
            var ctor=ex.GetType()
                .GetConstructor(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.NonPublic, null,new[] {typeof (string), typeof (Exception)}, null);
            if (ctor != null)
                return (Exception) ctor.Invoke(new object[] {ex.Message, ex});
            return ex;
        }
    }
}