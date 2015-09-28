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

namespace TsdClient
{
    public class MetricsFactoryBuilder
    {
        private IList<ISink> Sinks { get; set; }
        private string Host { get; set; }
        private string Service { get; set; }
        private string Cluster { get; set; }
        public MetricsFactoryBuilder(string service, string cluster)
        {
            Sinks = new List<ISink>();
            Host = System.Environment.MachineName;
            SetService(service);
            SetCluster(cluster);
        }

        public MetricsFactoryBuilder SetHost(string host)
        {
            if (host == null)
                throw new InvalidOperationException("Host can't be null");
            Host = host;
            return this;
        }

        public MetricsFactoryBuilder SetService(string service)
        {
            if(service == null)
                throw new InvalidOperationException("Service can't be null");
            Service = service;
            return this;
        }

        public MetricsFactoryBuilder SetCluster(string cluster)
        {
            if(cluster == null)
                throw new InvalidOperationException("Cluster can't be null");
            Cluster = cluster;
            return this;
        }

        public MetricsFactoryBuilder AddSink(params ISink[] sinks)
        {
            foreach (var x in sinks)
                Sinks.Add(x);
            return this;
        }

        public MetricsFactory Build()
        {
            return new MetricsFactory(Host, Service, Cluster, Sinks);
        }
    }
}
