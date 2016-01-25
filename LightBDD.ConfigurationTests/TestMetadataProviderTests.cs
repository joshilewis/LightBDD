using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace LightBDD.ConfigurationTests
{
    [TestFixture]
    public class TestMetadataProviderTests
    {
        public class TestableTestMetadataProvider : TestMetadataProvider
        {
            protected override string GetImplementationSpecificFeatureDescription(Type testClass)
            {
                throw new NotImplementedException();
            }

            protected override IEnumerable<string> GetImplementationSpecificScenarioCategories(MemberInfo member)
            {
                throw new NotImplementedException();
            }

            public override bool IsScenarioMethod(MethodBase method)
            {
                return method.GetCustomAttributes(typeof (TestAttribute), true).Any();
            }
        }

        [Test]
        public void Should_initialize_object_with_values_from_app_config()
        {
            var subject = new TestableTestMetadataProvider();
            Assert.That(subject.RepeatedStepReplacement, Is.EqualTo("and also"));
            Assert.That(subject.PredefinedStepTypes, Is.EquivalentTo(new[] { "call", "invoke", "exec" }));
        }
    }
}
