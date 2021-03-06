using System;
using System.Diagnostics;

namespace LightBDD.Execution.Implementation.Parameters
{
    [DebuggerStepThrough]
    internal class ConstantStepParameter<TContext> : IStepParameter<TContext>
    {
        private readonly Func<object, string> _formatFunction;

        public ConstantStepParameter(string name, object value, Func<object, string> formatFunction)
        {
            Name = name;
            Value = value;
            _formatFunction = formatFunction;
        }

        public void Evaluate(TContext context)
        {
        }

        public bool IsEvaluated { get { return true; } }
        public object Value { get; private set; }
        public string Name { get; private set; }

        public string Format()
        {
            return _formatFunction(Value);
        }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }
    }
}