﻿using System;
using System.Globalization;

namespace LightBDD.Formatting.Parameters
{
    /// <summary>
    /// Parameter formatter attribute, allowing to define custom step parameter formatting method
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ParameterFormatterAttribute : Attribute
    {
        /// <summary>
        /// Formats given parameter.
        /// </summary>
        public abstract string Format(object parameter);
        /// <summary>
        /// Formatter culture.
        /// </summary>
        protected internal CultureInfo Culture { get; set; }
    }
}