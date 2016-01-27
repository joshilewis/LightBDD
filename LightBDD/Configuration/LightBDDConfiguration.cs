using System.Configuration;
using System.Globalization;

namespace LightBDD.Configuration
{
    /// <summary>
    /// LightBDD configuration section allowing to configure feature summary writers.
    /// </summary>
    internal class LightBDDConfiguration : ConfigurationSection
    {
        private const string SUMMARY_WRITERS_FIELD = "summaryWriters";
        private const string STEP_TYPES_FIELD = "stepTypes";
        private const string FORMATTING_CULTURE_FIELD = "formattingCulture";

        public static LightBDDConfiguration GetConfiguration()
        {
            return ConfigurationManager.GetSection("lightbdd") as LightBDDConfiguration ?? new LightBDDConfiguration();
        }

        /// <summary>
        /// Returns summary writers collection.
        /// </summary>
        [ConfigurationProperty(SUMMARY_WRITERS_FIELD)]
        [ConfigurationCollection(typeof(SummaryWriterCollection), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public SummaryWriterCollection SummaryWriters
        {
            get { return (SummaryWriterCollection)this[SUMMARY_WRITERS_FIELD]; }
        }

        [ConfigurationProperty(STEP_TYPES_FIELD)]
        public StepTypesElement StepTypes
        {
            get { return (StepTypesElement)this[STEP_TYPES_FIELD]; }
            set { this[STEP_TYPES_FIELD] = value; }
        }

        [ConfigurationProperty(FORMATTING_CULTURE_FIELD)]
        public CultureInfo FormattingCulture
        {
            get { return (CultureInfo)this[FORMATTING_CULTURE_FIELD]; }
            set { this[FORMATTING_CULTURE_FIELD] = value; }
        }
    }
}