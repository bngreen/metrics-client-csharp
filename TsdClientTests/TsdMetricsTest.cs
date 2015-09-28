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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using TsdClient;
using Common.Logging;
using System.Threading;

namespace TsdClientTests
{
    [TestFixture]
    public class TsdMetricsTest
    {
        private const String FINAL_TIMESTAMP_KEY = "_end";
        private const String INITIAL_TIMESTAMP_KEY = "_start";

        [Test]
        public void testEmptySingleSink()
        {
            SimpleTest((s, metrics, l) =>
            {
                metrics.Dispose();
                SinkVerify(s, x => VerifyEmptySample(x));
            });
        }

        [Test]
        public void testEmptyMultipleSinks()
        {
            var sink1 = new Mock<ISink>();
            var sink2 = new Mock<ISink>();
            var metrics = new TsdMetrics(new[] { sink1.Object, sink2.Object }, "test", "test", "test");
            metrics.Dispose();
            SinkVerify(sink1, x => VerifyEmptySample(x));
            SinkVerify(sink2, x => VerifyEmptySample(x));
        }

        [Test]
        public void testCounterOnly()
        {
            SimpleTest((s, metrics, l) =>
            {
                metrics.IncrementCounter("counter");
                metrics.Dispose();
                SinkVerify(s, x =>
                    VerifySampleTimestampAnnotations(x) &&
                    VerifyQuantity(x.CounterSamples, "counter", 1, null) &&
                    x.GaugesSamples.Count == 0 &&
                    x.TimerSamples.Count == 0
                );
            });
        }

        [Test]
        public void testTimerOnly()
        {
            SimpleTest((s, m, l) =>
            {
                m.SetTimer("timer", 1L, Units.MilliSecond);
                m.Dispose();
                SinkVerify(s, x =>
                    VerifySampleTimestampAnnotations(x) &&
                    VerifyQuantity(x.TimerSamples, "timer", 1L, Units.MilliSecond) &&
                    x.CounterSamples.Count == 0 &&
                    x.GaugesSamples.Count == 0
                );
            });
        }

        [Test]
        public void testGaugeOnly()
        {
            SimpleTest(
                (s, m, l) =>
                {
                    m.SetGauge("gauge", 3.14);
                    m.Dispose();
                    SinkVerify(s, x =>
                        VerifySampleTimestampAnnotations(x) &&
                        VerifyQuantity(x.GaugesSamples, "gauge", (decimal)3.14, null) &&
                        x.CounterSamples.Count == 0 &&
                        x.TimerSamples.Count == 0
                    );
                }
            );
        }

        [Test]
        public void testTimerCounterGauge()
        {
            SimpleTest((s, metrics, l) =>
            {
                metrics.IncrementCounter("counter");
                metrics.SetTimer("timer", 1L, Units.MilliSecond);
                metrics.SetGauge("gauge", 3.14);
                metrics.Dispose();
                SinkVerify(s, x =>
                    VerifySampleTimestampAnnotations(x) &&
                    VerifyQuantity(x.GaugesSamples, "gauge", (decimal)3.14, null) &&
                    VerifyQuantity(x.TimerSamples, "timer", 1L, Units.MilliSecond) &&
                    VerifyQuantity(x.CounterSamples, "counter", 1, null)
                );
            });
        }

        [Test]
        public void testIsOpen()
        {
            SimpleTest((s, metrics, l) =>
            {
                Assert.True(metrics.IsOpen);
                metrics.Dispose();
                Assert.False(metrics.IsOpen);
            });
        }

        [Test]
        public void testCreateCounterNotOpen()
        {
            MetricNotOpenOperationTest(m =>
            {
                var counter = m.CreateCounter("counter-closed");
                Assert.NotNull(counter);
            });
        }

        [Test]
        public void testIncrementCounterNotOpen()
        {
            MetricNotOpenOperationTest(m => m.IncrementCounter("counter-closed"));
        }

        [Test]
        public void testResetCounterNotOpen()
        {
            MetricNotOpenOperationTest(m => m.ResetCounter("counter-closed"));
        }

        [Test]
        public void testSetGaugeDoubleNotOpen()
        {
            MetricNotOpenOperationTest(m => m.SetGauge("gauge-closed", 3.14));
        }

        [Test]
        public void testSetGaugeLongNotOpen()
        {
            MetricNotOpenOperationTest(m => m.SetGauge("gauge-closed", 10L));
        }

        [Test]
        public void testCreateTimerNotOpen()
        {
            MetricNotOpenOperationTest(m =>
            {
                var timer = m.CreateTimer("timer-closed");
                Assert.NotNull(timer);
            });
        }

        [Test]
        public void testSetTimerNotOpenTimeUnit()
        {
            MetricNotOpenOperationTest(m => m.SetTimer("timer-closed", 1, Units.MilliSecond));
        }

        [Test]
        public void testStartTimerNotOpen()
        {
            MetricNotOpenOperationTest(m => m.StartTimer("timer-closed"));
        }

        [Test]
        public void testStopTimerNotOpen()
        {
            MetricNotOpenOperationTest(m => m.StopTimer("timer-closed"));
        }

        [Test]
        public void testAnnotateNotOpen()
        {
            MetricNotOpenOperationTest(m => m.Annotate("key", "value"));
        }

        [Test]
        public void testCloseNotOpen()
        {
            SimpleTest((s, m, l) =>
            {
                m.Dispose();
                l.Setup(x => x.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
                m.Dispose();
                l.VerifyAll();
            }, true);
        }

        [Test]
        public void testStartTimerAlreadyStarted()
        {
            SimpleTest((s, m, l) =>
            {
                m.StartTimer("timer-already-started");
                l.Setup(x => x.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
                m.StartTimer("timer-already-started");
                l.VerifyAll();
            }, true);
        }

        [Test]
        public void testStopTimerNotStarted()
        {
            SimpleTest((s, m, l) =>
            {
                l.Setup(x => x.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
                m.StopTimer("timer-not-started");
                l.VerifyAll();
            }, true);
        }

        [Test]
        public void testStopTimerAlreadyStopped()
        {
            SimpleTest((s, m, l) =>
            {
                m.StartTimer("timer-already-stopped");
                m.StopTimer("timer-already-stopped");
                l.Setup(x => x.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()));
                m.StopTimer("timer-already-stopped");
                l.VerifyAll();
            }, true);
        }

        //TODO: public void testCloseTryWithResource() {

        [Test]
        public void testTimerMetrics()
        {
            var earliestDate = DateTime.Now;
            SimpleTest((s, metrics, l) =>
            {
                metrics.SetTimer("timerA", 100L, Units.MilliSecond);
                metrics.StartTimer("timerB");
                metrics.StopTimer("timerB");
                metrics.StartTimer("timerC");
                metrics.StopTimer("timerC");
                metrics.StartTimer("timerC");
                metrics.StopTimer("timerC");
                metrics.StartTimer("timerD");
                metrics.StopTimer("timerD");
                metrics.SetTimer("timerD", 1L, Units.MilliSecond);

                Thread.Sleep(10);

                metrics.Dispose();

                var latestDate = DateTime.Now;

                SinkVerify(s, sa =>
                    VerifySampleTimestampAnnotations(sa) &&
                    VerifyQuantity(sa.TimerSamples, "timerA", 100L, Units.MilliSecond) &&
                    VerifyQuantity(sa.TimerSamples, "timerB", q=>q.Unit == Units.NanoSecond) &&
                    VerifyQuantity(sa.TimerSamples, "timerC", q=>q.Unit == Units.NanoSecond) &&
                    VerifyQuantity(sa.TimerSamples, "timerD", q=>q.Unit == Units.NanoSecond) &&
                    VerifyQuantity(sa.TimerSamples, "timerD", 1, Units.MilliSecond) &&
                    sa.CounterSamples.Count == 0 &&
                    sa.GaugesSamples.Count == 0 &&
                    VerifyTimestamps(sa, earliestDate, latestDate)
                );

            });
        }

        [Test]
        public void testCounterMetrics()
        {
            var earliestDate = DateTime.Now;
            SimpleTest((s, metrics, l) =>
            {
                metrics.IncrementCounter("counterA");
                metrics.IncrementCounter("counterB", 2L);
                metrics.DecrementCounter("counterC");
                metrics.DecrementCounter("counterD", 2L);
                metrics.ResetCounter("counterE");
                metrics.ResetCounter("counterF");
                metrics.ResetCounter("counterF");
                metrics.IncrementCounter("counterF");
                metrics.ResetCounter("counterF");
                metrics.IncrementCounter("counterF");
                metrics.IncrementCounter("counterF");

                Thread.Sleep(10);

                metrics.Dispose();

                var latestDate = DateTime.Now;

                SinkVerify(s, sa =>
                    VerifySampleTimestampAnnotations(sa) &&
                    VerifyQuantity(sa.CounterSamples, "counterA", 1, null) &&
                    VerifyQuantity(sa.CounterSamples, "counterB", 2, null) &&
                    VerifyQuantity(sa.CounterSamples, "counterC", -1, null) &&
                    VerifyQuantity(sa.CounterSamples, "counterD", -2, null) &&
                    VerifyQuantity(sa.CounterSamples, "counterE", 0, null) &&
                    VerifyQuantity(sa.CounterSamples, "counterF", 0, null) &&
                    VerifyQuantity(sa.CounterSamples, "counterF", 1, null) &&
                    VerifyQuantity(sa.CounterSamples, "counterF", 2, null) &&
                    sa.TimerSamples.Count==0 &&
                    sa.GaugesSamples.Count == 0 &&
                    VerifyTimestamps(sa, earliestDate, latestDate)
                );

            });
        }

        [Test]
        public void testGaugeMetrics()
        {
            var earliestDate = DateTime.Now;
            SimpleTest((s, metrics, l) =>
            {
                metrics.SetGauge("gaugeA", 10L);
                metrics.SetGauge("gaugeB", 3.14);
                metrics.SetGauge("gaugeC", 10L);
                metrics.SetGauge("gaugeC", 20L);
                metrics.SetGauge("gaugeD", 2.07);
                metrics.SetGauge("gaugeD", 3.14);

                Thread.Sleep(10);

                metrics.Dispose();

                var latestDate = DateTime.Now;

                SinkVerify(s, sa =>
                    VerifySampleTimestampAnnotations(sa) &&
                    VerifyQuantity(sa.GaugesSamples, "gaugeA", 10, null) &&
                    VerifyQuantity(sa.GaugesSamples, "gaugeB", (decimal)3.14, null) &&
                    VerifyQuantity(sa.GaugesSamples, "gaugeC", 10, null) &&
                    VerifyQuantity(sa.GaugesSamples, "gaugeC", 20, null) &&
                    VerifyQuantity(sa.GaugesSamples, "gaugeD", (decimal)2.07, null) &&
                    VerifyQuantity(sa.GaugesSamples, "gaugeD", (decimal)3.14, null) &&
                    sa.CounterSamples.Count==0&&
                    sa.TimerSamples.Count == 0 &&
                    VerifyTimestamps(sa, earliestDate, latestDate)
                );

            });
        }

        [Test]
        public void testAnnotationMetrics()
        {
            var earliestDate = DateTime.Now;
            SimpleTest((s, metrics, l) =>
            {
                metrics.Annotate("foo", "bar");
                metrics.Annotate("dup", "cat");
                metrics.Annotate("dup", "dog");

                Thread.Sleep(10);

                metrics.Dispose();

                var latestDate = DateTime.Now;

                SinkVerify(s, sa =>
                    VerifySampleTimestampAnnotations(sa) &&
                    VerifyAnnotation(sa, "foo", "bar") &&
                    VerifyAnnotation(sa, "dup", "dog") &&
                    sa.CounterSamples.Count==0 &&
                    sa.GaugesSamples.Count == 0 &&
                    sa.TimerSamples.Count == 0 &&
                    VerifyTimestamps(sa, earliestDate, latestDate)
                );

            });
        }

        [Test]
        public void testUnits()
        {
            var earliestDate = DateTime.Now;
            SimpleTest((s, metrics, l) =>
            {

                metrics.SetGauge("bySize", 21L, Units.Byte);
                metrics.SetGauge("bySize", 22L, Units.KiloByte);
                metrics.SetGauge("bySize", 23L, Units.MegaByte);
                metrics.SetGauge("bySize", 24L, Units.GigaByte);

                // You should never do this but the library cannot prevent it because
                // values are combined across instances, processes and hosts:
                metrics.SetGauge("mixedUnit", 3.14, Units.Byte);
                metrics.SetGauge("mixedUnit", 2.07, Units.Second);

                metrics.SetTimer("withTsdUnit", 1L, Units.NanoSecond);
                metrics.SetTimer("withTsdUnit", 2L, Units.MicroSecond);
                metrics.SetTimer("withTsdUnit", 3L, Units.MilliSecond);
                metrics.SetTimer("withTsdUnit", 4L, Units.Second);
                metrics.SetTimer("withTsdUnit", 5L, Units.Minute);
                metrics.SetTimer("withTsdUnit", 6L, Units.Hour);
                metrics.SetTimer("withTsdUnit", 7L, Units.Day);

                metrics.Dispose();

                var latestDate = DateTime.Now;

                SinkVerify(s, sa =>
                    VerifySampleTimestampAnnotations(sa) &&
                    VerifyQuantity(sa.GaugesSamples, "bySize", 21, Units.Byte) &&
                    VerifyQuantity(sa.GaugesSamples, "bySize", 22, Units.KiloByte) &&
                    VerifyQuantity(sa.GaugesSamples, "bySize", 23, Units.MegaByte) &&
                    VerifyQuantity(sa.GaugesSamples, "bySize", 24, Units.GigaByte) &&
                    VerifyQuantity(sa.GaugesSamples, "mixedUnit", (decimal)3.14, Units.Byte) &&
                    VerifyQuantity(sa.GaugesSamples, "mixedUnit", (decimal)2.07, Units.Second) &&
                    VerifyQuantity(sa.TimerSamples, "withTsdUnit", 1, Units.NanoSecond) &&
                    VerifyQuantity(sa.TimerSamples, "withTsdUnit", 2, Units.MicroSecond) &&
                    VerifyQuantity(sa.TimerSamples, "withTsdUnit", 3, Units.MilliSecond) &&
                    VerifyQuantity(sa.TimerSamples, "withTsdUnit", 4, Units.Second) &&
                    VerifyQuantity(sa.TimerSamples, "withTsdUnit", 5, Units.Minute) &&
                    VerifyQuantity(sa.TimerSamples, "withTsdUnit", 6, Units.Hour) &&
                    VerifyQuantity(sa.TimerSamples, "withTsdUnit", 7, Units.Day) &&
                    sa.CounterSamples.Count == 0 &&
                    VerifyTimestamps(sa, earliestDate, latestDate)
                );

            });
        }

        [Test]
        public void testTimerObjects()
        {
            var earliestDate = DateTime.Now;
            SimpleTest((s, metrics, l) =>
            {
                var timerObjectA = metrics.CreateTimer("timerObjectA");
                var timerObjectB1 = metrics.CreateTimer("timerObjectB");
                var timerObjectB2 = metrics.CreateTimer("timerObjectB");

                Thread.Sleep(10);

                timerObjectA.Dispose();
                timerObjectB2.Dispose();

                Thread.Sleep(10);

                timerObjectB1.Dispose();

                metrics.Dispose();

                var latestDate = DateTime.Now;

                SinkVerify(s, sa =>
                    VerifySampleTimestampAnnotations(sa) &&
                    VerifyQuantity(sa.TimerSamples, "timerObjectA", q=>q.Value*1e-6 >= 10) &&
                    VerifyQuantity(sa.TimerSamples, "timerObjectB", q => q.Value*1e-6 >= 10) &&
                    VerifyQuantity(sa.TimerSamples, "timerObjectB", q => q.Value*1e-6 >= 20) &&
                    sa.CounterSamples.Count==0&&
                    sa.GaugesSamples.Count==0&&
                    VerifyTimestamps(sa, earliestDate, latestDate)
                );

            });
        }

        [Test]
        public void testSkipUnclosedTimerSample()
        {
            var earliestDate = DateTime.Now;
            SimpleTest((s, metrics, l) =>
            {
                metrics.CreateTimer("timerObjectA");
                metrics.SetTimer("timerObjectA", 1, Units.Second);

                metrics.Dispose();

                var latestDate = DateTime.Now;

                SinkVerify(s, sa =>
                    VerifySampleTimestampAnnotations(sa) &&
                    VerifyQuantity(sa.TimerSamples, "timerObjectA", 1, Units.Second) &&
                    sa.TimerSamples["timerObjectA"].Count() == 1 &&
                    sa.CounterSamples.Count == 0 &&
                    sa.GaugesSamples.Count == 0 &&
                    VerifyTimestamps(sa, earliestDate, latestDate)
                );

            });
        }

        [Test]
        public void testTimerWithoutClosedSample()
        {
            var earliestDate = DateTime.Now;
            SimpleTest((s, metrics, l) =>
            {
                metrics.CreateTimer("timerObjectB");
                metrics.SetTimer("timerObjectA", 1, Units.Second);

                metrics.Dispose();

                var latestDate = DateTime.Now;

                SinkVerify(s, sa =>
                    VerifySampleTimestampAnnotations(sa) &&
                    VerifyQuantity(sa.TimerSamples, "timerObjectA", 1, Units.Second) &&
                    sa.TimerSamples["timerObjectA"].Count() == 1 &&
                    sa.TimerSamples["timerObjectB"].Count() == 0 &&
                    sa.CounterSamples.Count == 0 &&
                    sa.GaugesSamples.Count == 0 &&
                    VerifyTimestamps(sa, earliestDate, latestDate)
                );

            });
        }

        [Test]
        public void testOnlyTimerWithClosedSample()
        {
            var earliestDate = DateTime.Now;
            SimpleTest((s, metrics, l) =>
            {
                metrics.CreateTimer("timerObjectB");

                metrics.Dispose();

                var latestDate = DateTime.Now;

                SinkVerify(s, sa =>
                    VerifySampleTimestampAnnotations(sa) &&
                    sa.TimerSamples["timerObjectB"].Count() == 0 &&
                    sa.CounterSamples.Count == 0 &&
                    sa.GaugesSamples.Count == 0 &&
                    VerifyTimestamps(sa, earliestDate, latestDate)
                );

            });
        }

        private bool VerifyAnnotation(Sample sa, string key, string value)
        {
            string v;
            if (!sa.Annotations.TryGetValue(key, out v))
                return false;
            return v == value;
        }

        private bool VerifyTimestamps(Sample sa, DateTime earliestStartDate, DateTime latestEndDate)
        {
            var actualStart = DateTime.Parse(sa.Annotations[INITIAL_TIMESTAMP_KEY]);
            var actualEnd = DateTime.Parse(sa.Annotations[FINAL_TIMESTAMP_KEY]);
            return earliestStartDate <= actualStart &&
                    latestEndDate >= actualStart &&
                    latestEndDate >= actualEnd &&
                    earliestStartDate <= actualEnd;
        }

        private void MetricNotOpenOperationTest(Action<TsdMetrics> op)
        {
            SimpleTest((s, metrics, l) =>
            {
                metrics.Dispose();
                op(metrics);
                l.Verify(x => x.Error(It.IsAny<Action<Common.Logging.FormatMessageHandler>>()), Times.Once);
            });
        }

        private void SimpleTest(Action<Mock<ISink>, TsdMetrics, Mock<ILog>> test, bool loggerStrict = false)
        {
            var sink = new Mock<ISink>();
            var logger = new Mock<ILog>();
            if (loggerStrict)
                logger = new Mock<ILog>(MockBehavior.Strict);
            var metrics = new TsdMetrics(new[] { sink.Object }, "test", "test", "test", logger.Object);
            test(sink, metrics, logger);
        }

        private bool VerifyQuantity<T>(IDictionary<string, IEnumerable<IQuantity<T>>> dict, string name, T value, IUnit unit) where T:IEquatable<T>
        {
            return VerifyQuantity(dict, name, q => q.Value.Equals(value) && q.Unit == unit);
        }

        private bool VerifyQuantity<T>(IDictionary<string, IEnumerable<IQuantity<T>>> dict, string name, Func<IQuantity<T>, bool> expression) where T : IEquatable<T>
        {
            IEnumerable<IQuantity<T>> en;
            if (dict.TryGetValue(name, out en))
                return en.Where(expression).Count() != 0;
            return false;
        }

        private void SinkVerify(Mock<ISink> sink, Func<Sample, bool> expression)
        {
            sink.Verify(x => x.Record(It.Is<Sample>(sa => expression(sa))));
        }

        private bool VerifySampleTimestampAnnotations(Sample sa)
        {
            return sa.Annotations.ContainsKey(INITIAL_TIMESTAMP_KEY) &&
                    sa.Annotations.ContainsKey(FINAL_TIMESTAMP_KEY);
        }

        private bool VerifyEmptySample(Sample sa)
        {
            return  VerifySampleTimestampAnnotations(sa) &&
                    sa.CounterSamples.Count == 0 &&
                    sa.GaugesSamples.Count == 0 &&
                    sa.TimerSamples.Count == 0;
        }

    }
}
