using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightBDD.Formatting.Helpers;

namespace LightBDD.Results.Formatters
{
    /// <summary>
    /// Formats feature results as plain text.
    /// </summary>
    public class PlainTextResultFormatter : IResultFormatter
    {
        #region IResultFormatter Members

        /// <summary>
        /// Formats feature results.
        /// </summary>
        /// <param name="features">Features to format.</param>
        public string Format(params IFeatureResult[] features)
        {
            var builder = new StringBuilder();
            FormatSummary(builder, features);
            foreach (var feature in features)
                FormatFeature(builder, feature);
            return builder.ToString();
        }

        #endregion

        private static void FormatDetails(StringBuilder builder, IScenarioResult scenario)
        {
            if (string.IsNullOrWhiteSpace(scenario.StatusDetails))
                return;

            builder.AppendLine("\t\tDetails:")
                   .Append("\t\t\t")
                   .Append(scenario.StatusDetails.Trim().Replace(Environment.NewLine, Environment.NewLine + "\t\t\t"))
                   .AppendLine();
        }

        private static void FormatFeature(StringBuilder builder, IFeatureResult feature)
        {
            if (builder.Length > 0)
                builder.AppendLine();

            builder.Append("Feature: ");
            builder.Append(feature.Name);
            if (!string.IsNullOrWhiteSpace(feature.Label))
                builder.Append(" [").Append(feature.Label).Append("]");
            builder.AppendLine();

            if (!string.IsNullOrWhiteSpace(feature.Description))
                builder.Append("\t").Append(feature.Description.Replace(Environment.NewLine, Environment.NewLine + "\t")).AppendLine();

            foreach (var scenario in feature.Scenarios)
                FormatScenario(builder, scenario);
        }

        private static void FormatScenario(StringBuilder builder, IScenarioResult scenario)
        {
            if (builder.Length > 0)
                builder.AppendLine();
            builder.Append("\tScenario: ");
            builder.Append(scenario.Name);
            if (!string.IsNullOrWhiteSpace(scenario.Label))
                builder.Append(" [").Append(scenario.Label).Append("]");
            builder.Append(" - ").Append(scenario.Status);

            if (scenario.ExecutionTime != null)
                builder.Append(" (").Append(scenario.ExecutionTime.FormatPretty()).Append(")");
            builder.AppendLine();

            if (scenario.Categories.Any())
                builder.Append("\t\tCategories: ").AppendLine(string.Join(", ", scenario.Categories));

            StringBuilder commentBuilder = new StringBuilder();
            foreach (var step in scenario.Steps)
            {
                FormatStep(builder, step);
                CollectComments(step, commentBuilder);
            }
            FormatDetails(builder, scenario);
            FormatComments(builder, commentBuilder);
        }

        private static void CollectComments(IStepResult step, StringBuilder commentBuilder)
        {
            foreach (var comment in step.Comments)
            {
                commentBuilder.Append("\t\t\tStep ")
                    .Append(step.Number)
                    .Append(": ")
                    .AppendLine(comment.Replace(Environment.NewLine, Environment.NewLine + "\t\t\t\t"));
            }
        }

        private static void FormatComments(StringBuilder builder, StringBuilder commentBuilder)
        {
            if (commentBuilder.Length > 0)
                builder.AppendLine("\t\tComments:").Append(commentBuilder);
        }

        private static void FormatStep(StringBuilder builder, IStepResult step)
        {
            builder.Append("\t\tStep ")
                .Append(step.Number).Append(": ")
                .Append(step.Name).Append(" - ").Append(step.Status);
            if (step.ExecutionTime != null)
                builder.Append(" (").Append(step.ExecutionTime.FormatPretty()).Append(")");
            builder.AppendLine();
        }

        private static void FormatSummary(StringBuilder builder, IFeatureResult[] features)
        {
            builder.AppendLine("Summary:");
            var summary = new Dictionary<string, object>
            {
                {"Test execution start time", features.GetTestExecutionStartTime().ToString("yyyy-MM-dd HH:mm:ss UTC")},
                {"Test execution time", features.GetTestExecutionTime().FormatPretty()},
                {"Number of features", features.Length},
                {"Number of scenarios", features.CountScenarios()},
                {"Passed scenarios", features.CountScenariosWithStatus(ResultStatus.Passed)},
                {"Bypassed scenarios", features.CountScenariosWithStatus(ResultStatus.Bypassed)},
                {"Failed scenarios", features.CountScenariosWithStatus(ResultStatus.Failed)},
                {"Ignored scenarios", features.CountScenariosWithStatus(ResultStatus.Ignored)},
                {"Number of steps", features.CountSteps()},
                {"Passed steps", features.CountStepsWithStatus(ResultStatus.Passed)},
                {"Bypassed steps", features.CountStepsWithStatus(ResultStatus.Bypassed)},
                {"Failed steps", features.CountStepsWithStatus(ResultStatus.Failed)},
                {"Ignored steps", features.CountStepsWithStatus(ResultStatus.Ignored)},
                {"Not Run steps", features.CountStepsWithStatus(ResultStatus.NotRun)}
            };

            var maxLength = summary.Keys.Max(k => k.Length);
            var format = string.Format("\t{{0,-{0}}}: {{1}}", maxLength);
            foreach (var row in summary)
                builder.AppendFormat(format, row.Key, row.Value).AppendLine();
        }
    }
}