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
using log4net;

namespace TsdClient
{
    public class TsdQueryLogSink : ISink
    {
        public class Builder
        {
            public enum RollPeriodE
            {
                Hourly,
                Daily,
                EveryMinute
            }

            public string Path { get; private set; }
            public string Name { get; private set; }
            public string Extension { get; private set; }
            public bool ImmediateFlush { get; private set; }

            private JSONLogFormat.IJsonLogSerializer Serializer{get;set;}

            private string DatePattern
            {
                get
                {
                    switch (RollPeriod)
                    {
                        case RollPeriodE.Daily:
                            return ".yyyy-MM-dd";
                        case RollPeriodE.EveryMinute:
                            return ".yyyy-MM-dd-mm";
                        case RollPeriodE.Hourly:
                            return ".yyyy-MM-dd-HH";
                        default:
                            throw new InvalidOperationException("Invalid Roll Period");
                    }
                }
            }

            public RollPeriodE RollPeriod { get; set; }

            public int MaxRollBackups { get; private set; }

            public Builder(string name)
            {
                Extension = "log";
                Path = "";
                SetName(name);
                ImmediateFlush = true;
                Serializer = JSONLogFormat.JsonLogSerializerFactory.CreateSerializer(JSONLogFormat.JsonLogSerializerFactory.LatestVersion);
                this.RollPeriod = Builder.RollPeriodE.Hourly;
                MaxRollBackups = 24;
            }

            internal Builder(string name, JSONLogFormat.IJsonLogSerializer serializer)
                : this(name)
            {
                if (serializer == null)
                    throw new InvalidOperationException("Serializer can't be null");
                Serializer = serializer;
            }

            public Builder SetName(string value)
            {
                if (String.IsNullOrEmpty(value))
                    throw new InvalidOperationException("Name can't be null or empty");
                Name = value;
                return this;
            }

            public Builder SetPath(string value)
            {
                if (value == null)
                    throw new InvalidOperationException("Path can't be null");
                Path = value;
                return this;
            }
            public Builder SetImmediateFlush(bool value)
            {
                ImmediateFlush = value;
                return this;
            }

            public Builder SetExtension(string value)
            {
                if (value == null)
                    throw new InvalidOperationException("Name can't be null");
                Extension = value;
                return this;
            }

            public Builder RollDaily()
            {
                this.RollPeriod = RollPeriodE.Daily;
                return this;
            }

            public Builder RollEveryMinute()
            {
                this.RollPeriod = RollPeriodE.EveryMinute;
                return this;
            }

            public Builder SetMaxRollBackups(int value)
            {
                if (value < 0)
                    throw new InvalidOperationException("MaxRollBackups can't be negative");
                MaxRollBackups = value;
                return this;
            }

            public string Filename
            {
                get
                {
                    if (Extension == "")
                        return System.IO.Path.Combine(Path, Name);
                    return System.IO.Path.Combine(Path, String.Format("{0}.{1}", Name, Extension));
                }
            }

            public ISink Build()
            {
                return new TsdQueryLogSink(Filename, DatePattern, MaxRollBackups, ImmediateFlush, Serializer);
            }

        }

        private Common.Logging.ILog Logger { get; set; }

        internal ILog SampleLogger { get; set; }
        private JSONLogFormat.IJsonLogSerializer Serializer { get; set; }

        internal log4net.Appender.RollingFileAppender Appender { get { return (SampleLogger.Logger.Repository.GetAppenders().First() as log4net.Appender.RollingFileAppender); } }


        internal TsdQueryLogSink(string filename, string datePattern, int maxRollBackups, bool immediateFlush, JSONLogFormat.IJsonLogSerializer serializer)
        {
            Logger = Common.Logging.LogManager.GetLogger(typeof(TsdQueryLogSink));
            var reposName = Guid.NewGuid().ToString();


            var repos = LogManager.CreateRepository(reposName);
            var appender = new log4net.Appender.RollingFileAppender();
            //appender.MaxFileSize = maxFileSize;
            appender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Date;
            appender.MaxSizeRollBackups = maxRollBackups;
            appender.File = filename;
            appender.Layout = new log4net.Layout.PatternLayout("%message%newline");
            appender.ImmediateFlush = immediateFlush;
            appender.StaticLogFileName = false;
            appender.DatePattern = datePattern;
            appender.ActivateOptions();
            log4net.Config.BasicConfigurator.Configure(repos, appender);
            SampleLogger = LogManager.GetLogger(reposName, "SampleLogger");

            Serializer = serializer;
        }

        public void Record(Sample sample)
        {
            try
            {
                SampleLogger.Info(Serializer.Serialize(sample));
            }
            catch (Exception e)
            {
                Logger.Warn(m => m("Exception serializing and writing metrics {0}", e));
            }
        }
    }
}
