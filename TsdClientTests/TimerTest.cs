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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TsdClient;
using Moq;
using Common.Logging;

namespace TsdClientTests
{
    [TestFixture]
    public class TimerTest
    {

        [Test]
        public void TimerOONotNullTest()
        {
            var m = new MetricsFactoryBuilder("a", "a").Build().CreateMetric();
            var t = m.CreateTimer("foo");
            Assert.NotNull(t);
        }

        [Test]
        public void TimerOOStartedTest()
        {
            var m = new MetricsFactoryBuilder("a", "a").Build().CreateMetric();
            var t = m.CreateTimer("foo");
            Assert.True(t.IsStarted);
            Assert.False(t.IsStopped);
        }
        [Test]
        public void TimerOOStoppedTest()
        {
            var m = new MetricsFactoryBuilder("a", "a").Build().CreateMetric();
            var t = m.CreateTimer("foo");
            t.Stop();
            Assert.False(t.IsStarted);
            Assert.True(t.IsStopped);
        }

        [Test]
        public void TimerOOValueTest()
        {
            var m = new MetricsFactoryBuilder("a", "a").Build().CreateMetric();
            var t = m.CreateTimer("foo");
            Thread.Sleep(800);
            t.Stop();
            var ratio = ((800e6) / t.Value);
            Assert.True(Math.Abs(ratio-1) < 0.1);
        }

        [Test]
        public void testClose()
        {
            int minimumTimeInMilliseconds = 100;
            long startTime = DateTime.Now.Ticks;
            ITimer timer = new TsdTimer("timerName", true);
            Assert.NotNull(timer);
            Thread.Sleep(minimumTimeInMilliseconds);
            timer.Dispose();
            long elapsedTimeInNanoseconds = (long)((DateTime.Now.Ticks - startTime) *100);
            Assert.GreaterOrEqual(timer.Value, minimumTimeInMilliseconds * 1e6);
            Assert.LessOrEqual(timer.Value, elapsedTimeInNanoseconds);
        }

        [Test]
        public void testAlreadyStopped()
        {
            var isOpen = true;
            var logger = new Mock<ILog>(MockBehavior.Strict);

            TsdTimer timer = new TsdTimer("timerName", isOpen, logger.Object);
            timer.Stop();
            logger.Setup(l => l.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
            timer.Stop();
            logger.VerifyAll();
        }

        [Test]
        public void testStopAfterMetricsClosed()
        {
            var isOpen = true;
            var logger = new Mock<ILog>(MockBehavior.Strict);
            TsdTimer timer = new TsdTimer("timerName", isOpen, logger.Object);
            timer.Dispose();
            logger.Setup(l => l.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
            timer.Stop();
            logger.VerifyAll();
        }

        [Test]
        public void testGetElapsedAfterStop()
        {
            var logger = new Mock<ILog>(MockBehavior.Strict);
            var isOpen = true;

            TsdTimer timer = new TsdTimer("timerName", isOpen, logger.Object);
            logger.Setup(l => l.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
            var elap = timer.Elapsed;
            logger.VerifyAll();
            
        }

        [Test]
        public void testIsStopped()
        {
            var isOpen = true;
            TsdTimer timer = new TsdTimer("timerName", isOpen);
            Assert.False(timer.IsStopped);
            timer.Stop();
            Assert.True(timer.IsStopped);
        }

        [Test]
        public void testToString()
        {
            var isOpen = true;
            String asString = new TsdTimer("timerName", isOpen).ToString();
            Assert.NotNull(asString);
            Assert.False(String.IsNullOrEmpty(asString));
            Assert.That(asString.Contains("timerName"));
        }

        [Test]
        public void testConstructor()
        {
            var isOpen = true;
            TsdTimer timer = new TsdTimer("timerName", isOpen);
            Assert.False(timer.IsStopped);
        }

    }
}
