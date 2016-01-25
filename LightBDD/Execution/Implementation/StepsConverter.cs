using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using LightBDD.Execution.Implementation.Parameters;
using LightBDD.Results;

namespace LightBDD.Execution.Implementation
{
    [DebuggerStepThrough]
    internal class StepsConverter : IStepsConverter
    {
        class ConversionContext
        {
            private readonly TestMetadataProvider _metadataProvider;
            private int _stepNumber;
            private string _lastStepTypeName;

            public ConversionContext(TestMetadataProvider metadataProvider)
            {
                _metadataProvider = metadataProvider;
            }

            public int NextStepNumber() { return ++_stepNumber; }

            public string NormalizeStepTypeName(string stepTypeName)
            {
                var normalized = _metadataProvider.NormalizeStepTypeName(stepTypeName, _lastStepTypeName);
                _lastStepTypeName = stepTypeName;
                return normalized;
            }

            public void StoreLastStepTypeName(string stepTypeName)
            {
                _lastStepTypeName = stepTypeName;
            }
        }
        private readonly Func<Type, ResultStatus> _mapExceptionToStatus;
        private readonly TestMetadataProvider _metadataProvider;

        public StepsConverter(TestMetadataProvider metadataProvider, Func<Type, ResultStatus> mapExceptionToStatus)
        {
            _mapExceptionToStatus = mapExceptionToStatus;
            _metadataProvider = metadataProvider;
        }

        public IEnumerable<IStep> Convert(IEnumerable<Action> steps)
        {
            var ctx = new ConversionContext(_metadataProvider);
            return steps.Select(step => CreateStep(step, step.Method, ctx));
        }

        public IEnumerable<IStep> Convert<TContext>(TContext context, IEnumerable<Action<TContext>> steps)
        {
            var ctx = new ConversionContext(_metadataProvider);
            return steps.Select(step => CreateStep(() => step(context), step.Method, ctx));
        }

        public IEnumerable<IStep> Convert(IEnumerable<Expression<Action<StepType>>> steps)
        {
            var ctx = new ConversionContext(_metadataProvider);
            return steps.Select(step => CreateParameterizedStep(step, ctx));
        }

        public IEnumerable<IStep> Convert(IEnumerable<Func<Task>> steps)
        {
            var ctx = new ConversionContext(_metadataProvider);
            return steps.Select(step => CreateAsyncStep(step, step.Method, ctx));
        }

        public IEnumerable<IStep> Convert(Expression<Func<StepType, Task>>[] steps)
        {
            var ctx = new ConversionContext(_metadataProvider);
            return steps.Select(step => CreateParameterizedAsyncStep(step, ctx));
        }

        public IEnumerable<IStep> Convert<TContext>(TContext context, IEnumerable<Expression<Action<StepType, TContext>>> steps)
        {
            var ctx = new ConversionContext(_metadataProvider);
            return steps.Select(step => CreateParameterizedStep(context, step, ctx));
        }

        public IEnumerable<IStep> Convert<TContext>(TContext context, Func<TContext, Task>[] steps)
        {
            var ctx = new ConversionContext(_metadataProvider);
            return steps.Select(step => CreateAsyncStep(() => step(context), step.Method, ctx));
        }

        public IEnumerable<IStep> Convert<TContext>(TContext context, Expression<Func<StepType, TContext, Task>>[] steps)
        {
            var ctx = new ConversionContext(_metadataProvider);
            return steps.Select(step => CreateParameterizedAsyncStep(context, step, ctx));
        }

        private Step CreateStep(Action step, MethodInfo stepMethod, ConversionContext conversionContext)
        {
            var stepName = _metadataProvider.GetStepName(stepMethod);
            var stepTypeName = _metadataProvider.GetStepTypeNameFromFormattedStepName(ref stepName);
            Func<Task> asyncStep = () => { step(); return TaskEx.FromResult(0); };
            return new Step(asyncStep, conversionContext.NormalizeStepTypeName(stepTypeName), stepName, conversionContext.NextStepNumber(), _mapExceptionToStatus);
        }

        private Step CreateAsyncStep(Func<Task> step, MethodInfo stepMethod, ConversionContext conversionContext)
        {
            var stepName = _metadataProvider.GetStepName(stepMethod);
            var stepTypeName = _metadataProvider.GetStepTypeNameFromFormattedStepName(ref stepName);
            return new Step(step, conversionContext.NormalizeStepTypeName(stepTypeName), stepName, conversionContext.NextStepNumber(), _mapExceptionToStatus);
        }

        private IStep CreateParameterizedStep(Expression<Action<StepType>> stepExpression, ConversionContext conversionContext)
        {
            var stepTypeParameter = stepExpression.Parameters[0];
            var contextParameter = Expression.Parameter(typeof(object));
            return CreateParameterizedStep((object)null, stepExpression, conversionContext, stepTypeParameter, contextParameter);
        }

        private IStep CreateParameterizedAsyncStep(Expression<Func<StepType,Task>> stepExpression, ConversionContext conversionContext)
        {
            var stepTypeParameter = stepExpression.Parameters[0];
            var contextParameter = Expression.Parameter(typeof(object));
            return CreateParameterizedAsyncStep((object)null, stepExpression, conversionContext, stepTypeParameter, contextParameter);
        }

        private IStep CreateParameterizedStep<TContext>(TContext context, Expression<Action<StepType, TContext>> stepExpression, ConversionContext conversionContext)
        {
            var stepTypeParameter = stepExpression.Parameters[0];
            var contextParameter = stepExpression.Parameters[1];
            return CreateParameterizedStep(context, stepExpression, conversionContext, stepTypeParameter, contextParameter);
        }

        private IStep CreateParameterizedAsyncStep<TContext>(TContext context, Expression<Func<StepType, TContext,Task>> stepExpression, ConversionContext conversionContext)
        {
            var stepTypeParameter = stepExpression.Parameters[0];
            var contextParameter = stepExpression.Parameters[1];
            return CreateParameterizedAsyncStep(context, stepExpression, conversionContext, stepTypeParameter, contextParameter);
        }

        private IStep CreateParameterizedStep<TExpression, TContext>(TContext context, Expression<TExpression> stepExpression, ConversionContext conversionContext, ParameterExpression stepTypeParameter, ParameterExpression contextParameter)
        {
            var methodExpression = GetMethodExpression(stepExpression);
            var stepNameFormat = _metadataProvider.GetStepNameFormat(methodExpression.Method);
            var stepTypeName = _metadataProvider.GetStepTypeName(stepTypeParameter.Name);
            if (stepTypeName == string.Empty)
                stepTypeName = conversionContext.NormalizeStepTypeName(_metadataProvider.GetStepTypeNameFromFormattedStepName(ref stepNameFormat));
            else
                conversionContext.StoreLastStepTypeName(stepTypeName);

            Action<StepType, TContext, object[]> action = CompileAction<TContext>(methodExpression, stepTypeParameter, contextParameter);
            var methodParameterInfo = methodExpression.Method.GetParameters();
            var arguments = methodExpression.Arguments.Select((arg, index) => CompileArgument<TContext>(arg, stepTypeParameter, contextParameter, methodParameterInfo[index])).ToArray();

            Func<StepType, TContext, object[], Task> asyncAction = (stepType, ctx, args) => { action(stepType, ctx, args); return TaskEx.FromResult(0); };
            return new ParameterizedStep<TContext>(methodExpression.Method, context, asyncAction, arguments, stepTypeName, stepNameFormat, conversionContext.NextStepNumber(), _mapExceptionToStatus);
        }

        private IStep CreateParameterizedAsyncStep<TExpression, TContext>(TContext context, Expression<TExpression> stepExpression, ConversionContext conversionContext, ParameterExpression stepTypeParameter, ParameterExpression contextParameter)
        {
            var methodExpression = GetMethodExpression(stepExpression);
            var stepNameFormat = _metadataProvider.GetStepNameFormat(methodExpression.Method);
            var stepTypeName = _metadataProvider.GetStepTypeName(stepTypeParameter.Name);
            if (stepTypeName == string.Empty)
                stepTypeName = conversionContext.NormalizeStepTypeName(_metadataProvider.GetStepTypeNameFromFormattedStepName(ref stepNameFormat));
            else
                conversionContext.StoreLastStepTypeName(stepTypeName);

            var action = CompileAsyncAction<TContext>(methodExpression, stepTypeParameter, contextParameter);
            var methodParameterInfo = methodExpression.Method.GetParameters();
            var arguments = methodExpression.Arguments.Select((arg, index) => CompileArgument<TContext>(arg, stepTypeParameter, contextParameter, methodParameterInfo[index])).ToArray();

            return new ParameterizedStep<TContext>(methodExpression.Method, context, action, arguments, stepTypeName, stepNameFormat, conversionContext.NextStepNumber(), _mapExceptionToStatus);
        }

        private IStepParameter<TContext> CompileArgument<TContext>(Expression argumentExpression, ParameterExpression stepTypeParameter, ParameterExpression contextParameter, ParameterInfo parameterInfo)
        {
            var formatter = _metadataProvider.GetStepParameterFormatter(parameterInfo);
            var expression = argumentExpression as ConstantExpression;
            if (expression != null)
                return new ConstantStepParameter<TContext>(parameterInfo.Name, expression.Value, formatter);

            var compiledParam = Expression.Lambda<Func<StepType, TContext, object>>(Expression.Convert(argumentExpression, typeof(object)), stepTypeParameter, contextParameter).Compile();
            return new MutableStepParameter<TContext>(parameterInfo.Name, compiledParam, formatter);
        }

        private static Action<StepType, TContext, object[]> CompileAction<TContext>(MethodCallExpression methodCall, ParameterExpression stepTypeParameter, ParameterExpression contextParameter)
        {
            var param = Expression.Parameter(typeof(object[]), "args");
            var args = methodCall.Arguments.Select((arg, index) => Expression.Convert(Expression.ArrayAccess(param, Expression.Constant(index)), arg.Type));
            var methodCallExpression = Expression.Call(methodCall.Object, methodCall.Method, args);
            return Expression.Lambda<Action<StepType, TContext, object[]>>(methodCallExpression, stepTypeParameter, contextParameter, param).Compile();
        }

        private static Func<StepType, TContext, object[],Task> CompileAsyncAction<TContext>(MethodCallExpression methodCall, ParameterExpression stepTypeParameter, ParameterExpression contextParameter)
        {
            var param = Expression.Parameter(typeof(object[]), "args");
            var returnLabel = Expression.Label(typeof (Task));
            var args = methodCall.Arguments.Select((arg, index) => Expression.Convert(Expression.ArrayAccess(param, Expression.Constant(index)), arg.Type));

            var block = Expression.Block(
                Expression.Return(returnLabel, Expression.Call(methodCall.Object, methodCall.Method, args)),
                Expression.Label(returnLabel));
            return Expression.Lambda<Func<StepType, TContext, object[], Task>>(block, stepTypeParameter, contextParameter, param).Compile();
        }

        private static MethodCallExpression GetMethodExpression<T>(Expression<T> stepExpression)
        {
            var methodExpression = stepExpression.Body as MethodCallExpression;
            if (methodExpression == null)
                throw new ArgumentException("Unsupported step expression. Expected MethodCallExpression, got: " + stepExpression);

            if (methodExpression.Method.GetParameters().Any(p => p.IsOut || p.ParameterType.IsByRef))
                throw new ArgumentException("Steps accepting ref or out parameters are not supported: " + stepExpression);
            return methodExpression;
        }
    }
}