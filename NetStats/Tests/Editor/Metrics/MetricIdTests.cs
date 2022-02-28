using NUnit.Framework;

using Unity.Multiplayer.Tools.MetricTypes;

namespace Unity.Multiplayer.Tools.NetStats.Tests
{
    internal class MetricIdTests
    {
        [TestCase(TestMetric.A)]
        [TestCase(TestMetric.B)]
        [TestCase(TestMetric.C)]
        [TestCase(TestMetric.D)]
        [TestCase(TestMetric.E)]
        [TestCase(TestMetric.F)]
        [TestCase(TestMetric.G)]
        public void GivenMetricId_WhenToString_ReturnsCorrectValue(TestMetric value)
        {
            var metricId = MetricId.Create(value);

            var stringValue = metricId.ToString();

            Assert.AreEqual(typeof(TestMetric), metricId.EnumType);
            Assert.AreEqual((int)value, metricId.EnumValue);
            Assert.AreEqual(value.ToString(), stringValue);
        }
    }
}
