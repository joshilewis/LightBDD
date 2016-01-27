using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace LightBDD.UnitTests.Helpers
{
    internal class TestableMetadataProvider : TestMetadataProvider
    {
        public TestableMetadataProvider()
        {
        }

        public TestableMetadataProvider(string[] predefinedStepTypes, string repeatedStepReplacement, CultureInfo culture)
            : base(predefinedStepTypes, repeatedStepReplacement, culture)
        {
        }

        protected override string GetImplementationSpecificFeatureDescription(Type testClass)
        {
            return testClass.GetCustomAttributes(typeof(DescriptionAttribute), true)
                            .OfType<DescriptionAttribute>()
                            .Select(a => a.Description)
                            .SingleOrDefault();
        }

        protected override IEnumerable<string> GetImplementationSpecificScenarioCategories(MemberInfo member)
        {
            return ExtractAttributePropertyValues<CategoryAttribute>(member, a => a.Name);
        }

        public override bool IsScenarioMethod(MethodBase method)
        {
            return method.GetCustomAttributes(typeof(TestAttribute), true).Any();
        }
    }
}