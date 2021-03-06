﻿using LightBDD.SummaryGeneration;
using LightBDD.UnitTests.Helpers;
using NUnit.Framework;
using Rhino.Mocks;

namespace LightBDD.UnitTests.SummaryGeneration
{
	[TestFixture]
	public class SummaryGeneratorTests
	{
		private SummaryGenerator _subject;
		private ISummaryWriter _writer;

		#region Setup/Teardown

		[SetUp]
		public void SetUp()
		{
			_writer = MockRepository.GenerateMock<ISummaryWriter>();
			_subject = new SummaryGenerator(_writer);
		}

		#endregion

		[Test]
		public void Should_add_and_save_results()
		{
			var feature = Mocks.CreateFeatureResult("name", "description", "label");
			_subject.AddFeature(feature);
			_subject.Finished();
			_writer.AssertWasCalled(w => w.Save(feature));
		}
	}
}