/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TsdClient;
using Moq;

namespace TsdClientTests
{
    [TestFixture]
    public class CounterTest
    {
        [Test]
        public void testIncrement()
        {
            var metric = new MetricsFactoryBuilder("", "").Build().CreateMetric();
            var counter = metric.CreateCounter("counterName");
            Assert.AreEqual(0L, counter.Value.Value);
            counter.Increment();
            Assert.AreEqual(1L, counter.Value.Value);
            counter.Increment();
            Assert.AreEqual(2L, counter.Value.Value);
        }
        [Test]
        public void testDecrement()
        {
            var metric = new MetricsFactoryBuilder("", "").Build().CreateMetric();
            var counter = metric.CreateCounter("counterName");
            Assert.AreEqual(0L, counter.Value.Value);
            counter.Decrement();
            Assert.AreEqual(-1L, counter.Value.Value);
            counter.Decrement();
            Assert.AreEqual(-2L, counter.Value.Value);
        }
        [Test]
        public void testIncrementByValue()
        {
            var metric = new MetricsFactoryBuilder("", "").Build().CreateMetric();
            var counter = metric.CreateCounter("counterName");
            Assert.AreEqual(0L, counter.Value.Value);
            counter.Increment(2);
            Assert.AreEqual(2L, counter.Value.Value);
            counter.Increment(3);
            Assert.AreEqual(5L, counter.Value.Value);
        }
        [Test]
        public void testDecrementByValue()
        {
            var metric = new MetricsFactoryBuilder("", "").Build().CreateMetric();
            var counter = metric.CreateCounter("counterName");
            Assert.AreEqual(0L, counter.Value.Value);
            counter.Decrement(2);
            Assert.AreEqual(-2L, counter.Value.Value);
            counter.Decrement(3);
            Assert.AreEqual(-5L, counter.Value.Value);
        }
        [Test]
        public void testCombination()
        {
            var metric = new MetricsFactoryBuilder("", "").Build().CreateMetric();
            var counter = metric.CreateCounter("counterName");
            Assert.AreEqual(0L, counter.Value.Value);
            counter.Increment();
            Assert.AreEqual(1L, counter.Value.Value);
            counter.Decrement(3L);
            Assert.AreEqual(-2L, counter.Value.Value);
            counter.Increment(4L);
            Assert.AreEqual(2L, counter.Value.Value);
            counter.Decrement();
            Assert.AreEqual(1L, counter.Value.Value);
        }

        [Test]
        public void testIncrementAfterClose()
        {
            var mock = new Mock<Common.Logging.ILog>(MockBehavior.Strict);
            var counter = new TsdCounter("counterName", 0, null, true, mock.Object);
            counter.Increment();
            counter.Dispose();
            mock.Setup(l => l.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
            counter.Increment();
            mock.VerifyAll();
        }

        [Test]
        public void testDecrementAfterClose()
        {
            var mock = new Mock<Common.Logging.ILog>(MockBehavior.Strict);
            var counter = new TsdCounter("counterName", 0, null, true, mock.Object);
            counter.Decrement();
            counter.Dispose();
            mock.Setup(l => l.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
            counter.Decrement();
            mock.VerifyAll();
        }

        [Test]
        public void testIncrementByValueAfterClose()
        {
            var mock = new Mock<Common.Logging.ILog>(MockBehavior.Strict);
            var counter = new TsdCounter("counterName", 0, null, true, mock.Object);
            counter.Increment(2);
            counter.Dispose();
            mock.Setup(l => l.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
            counter.Increment(2);
            mock.VerifyAll();
        }

        [Test]
        public void testDecrementByValueAfterClose()
        {
            var mock = new Mock<Common.Logging.ILog>(MockBehavior.Strict);
            var counter = new TsdCounter("counterName", 0, null, true, mock.Object);
            counter.Decrement(2);
            counter.Dispose();
            mock.Setup(l => l.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
            counter.Decrement(2);
            mock.VerifyAll();
        }


        [Test]
        public void testToString()
        {
            var metric = new MetricsFactoryBuilder("", "").Build().CreateMetric();
            var asString = metric.CreateCounter("counterName").ToString();
            Assert.IsFalse(String.IsNullOrEmpty(asString));
            Assert.AreEqual(0, String.Compare(asString, "counterName", false));
        }
    }
}
