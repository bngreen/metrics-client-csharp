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

using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsdClient;

namespace TsdClientTests
{
    [TestFixture]
    public class MetricsFactoryTest
    {
        [Test]
        public void testBuilderServiceNil()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new MetricsFactoryBuilder(null, "test"));
        }
        [Test]
        public void testBuilderClusterNil()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new MetricsFactoryBuilder("test", null));
        }
        [Test]
        public void testBuilderHostNil()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new MetricsFactoryBuilder("test", "test").SetHost(null));
        }
        [Test]
        public void testCreate()
        {
            var sink1 = new Mock<ISink>(MockBehavior.Strict);
            var sink2 = new Mock<ISink>(MockBehavior.Strict);
            var metricsFactory = new MetricsFactoryBuilder("test", "test").AddSink(sink1.Object, sink2.Object).Build();
            var metrics = metricsFactory.CreateMetric();
            Assert.NotNull(metrics);
            Assert.That(metrics is TsdMetrics);
            sink1.Setup(l => l.Record(It.IsAny<Sample>()));
            sink2.Setup(l => l.Record(It.IsAny<Sample>()));
            metrics.Dispose();
            sink1.VerifyAll();
            sink2.VerifyAll();
        }
        [Test]
        public void testCreateEmptySinks()
        {
            var metricsFactory = new MetricsFactoryBuilder("test", "test").AddSink(new ISink[0]).Build();
            var metrics = metricsFactory.CreateMetric();
            Assert.NotNull(metrics);
            Assert.That(metrics is TsdMetrics);
            metrics.Dispose();
        }

    }
}
