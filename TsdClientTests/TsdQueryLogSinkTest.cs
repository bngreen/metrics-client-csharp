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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsdClient;

namespace TsdClientTests
{
    [TestFixture]
    public class TsdQueryLogSinkTest
    {
        [Test]
        public void testWithNameNil()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new TsdQueryLogSink.Builder(null));
            Assert.Throws(typeof(InvalidOperationException), () => new TsdQueryLogSink.Builder("query").SetName(null));
        }
        [Test]
        public void testWithNameEmpty()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new TsdQueryLogSink.Builder(""));
            Assert.Throws(typeof(InvalidOperationException), () => new TsdQueryLogSink.Builder("query").SetName(""));
        }
        [Test]
        public void testBuilderWithDefaults()
        {
            var sinkBuilder = new TsdQueryLogSink.Builder("query");
            Assert.AreEqual("query", sinkBuilder.Name);
            Assert.AreEqual("", sinkBuilder.Path);
            Assert.AreEqual(TsdQueryLogSink.Builder.RollPeriodE.Hourly, sinkBuilder.RollPeriod);
            Assert.AreEqual("log", sinkBuilder.Extension);
            Assert.AreEqual(24, sinkBuilder.MaxRollBackups);
            Assert.AreEqual(true, sinkBuilder.ImmediateFlush);
            var sink = sinkBuilder.Build() as TsdQueryLogSink;
            Assert.AreEqual(24, sink.Appender.MaxSizeRollBackups);
            Assert.AreEqual(true, sink.Appender.ImmediateFlush);
            var file = sink.Appender.File;
            var date = DateTime.Now;
            Assert.AreEqual(String.Format("query.log.{0}", date.ToString("yyyy-MM-dd-HH")), System.IO.Path.GetFileName(file));
        }
        [Test]
        public void testCustomBuilder()
        {
            string name = "customName";
            string extension = "custom";
            var sinkBuilder = new TsdQueryLogSink.Builder(name).SetExtension(extension).SetImmediateFlush(false).SetMaxRollBackups(12).RollDaily();
            Assert.AreEqual(name, sinkBuilder.Name);
            Assert.AreEqual("", sinkBuilder.Path);
            Assert.AreEqual(TsdQueryLogSink.Builder.RollPeriodE.Daily, sinkBuilder.RollPeriod);
            Assert.AreEqual(extension, sinkBuilder.Extension);
            Assert.AreEqual(12, sinkBuilder.MaxRollBackups);
            Assert.AreEqual(false, sinkBuilder.ImmediateFlush);
            var sink = sinkBuilder.Build() as TsdQueryLogSink;
            Assert.AreEqual(12, sink.Appender.MaxSizeRollBackups);
            Assert.AreEqual(false, sink.Appender.ImmediateFlush);
            var file = sink.Appender.File;
            var date = DateTime.Now;
            Assert.AreEqual(String.Format("{1}.{2}.{0}", date.ToString("yyyy-MM-dd"), name, extension), System.IO.Path.GetFileName(file));
        }

        [Test]
        public void testBuilderNullPath()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new TsdQueryLogSink.Builder("query")
                    .SetPath(null)
                    );
        }

        [Test]
        public void testBuilderNullExtension()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new TsdQueryLogSink.Builder("query")
                    .SetExtension(null)
                    );
        }

        [Test]
        public void testBuilderNullName()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new TsdQueryLogSink.Builder("query")
                    .SetName(null)
                    );
        }

        [Test]
        public void testBuilderEmptyName()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new TsdQueryLogSink.Builder("query")
                    .SetName("")
                    );
        }

        [Test]
        public void testBuilderNegativeMaxHistory()
        {
            Assert.Throws(typeof(InvalidOperationException), () => new TsdQueryLogSink.Builder("query")
                    .SetMaxRollBackups(-1)
                    );
        }

    }
}
