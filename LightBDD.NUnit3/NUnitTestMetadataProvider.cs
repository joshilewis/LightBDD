﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace LightBDD
{
    [DebuggerStepThrough]
    internal class NUnitTestMetadataProvider : TestMetadataProvider
    {
        public static readonly TestMetadataProvider Instance = new NUnitTestMetadataProvider();

        /// <summary>
        /// Returns implementation specific feature description or null if such is not provided.
        /// If test class is annotated with [Description] attribute, it's content is used as feature description.
        /// </summary>
        /// <param name="testClass">Class to analyze.</param>
        /// <returns>Feature description or null.</returns>
        protected override string GetImplementationSpecificFeatureDescription(Type testClass)
        {
            return ExtractAttributePropertyValue<DescriptionAttribute>(testClass, a => a.Properties["Description"].Cast<string>().Single());
        }

        /// <summary>
        /// Returns implementation specific scenario categories or empty collection if no categories are provided.
        /// If test class is annotated with [Category] attribute, it's content is used as scenario category.
        /// </summary>
        /// <param name="member">Scenario method or feature test class to analyze.</param>
        /// <returns>Scenario categories or empty collection.</returns>
        protected override IEnumerable<string> GetImplementationSpecificScenarioCategories(MemberInfo member)
        {
            return ExtractAttributePropertyValues<CategoryAttribute>(member, a => a.Name);
        }
    }
}