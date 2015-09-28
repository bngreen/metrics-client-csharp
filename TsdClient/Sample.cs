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
    public class Sample
    {
        public Sample(
            IDictionary<string, string> annotations,
            IDictionary<string, IEnumerable<IQuantity<ConcurrentInt64>>> timerSamples,
            IDictionary<string, IEnumerable<IQuantity<ConcurrentInt64>>> counterSamples,
            IDictionary<string, IEnumerable<IQuantity<Decimal>>> gaugesSamples)
        {
            Annotations = annotations;
            TimerSamples = timerSamples;
            CounterSamples = counterSamples;
            GaugesSamples = gaugesSamples;
        }
        public IDictionary<string, string> Annotations { get; private set; }
        public IDictionary<string, IEnumerable<IQuantity<ConcurrentInt64>>> TimerSamples { get; private set; }
        public IDictionary<string, IEnumerable<IQuantity<ConcurrentInt64>>> CounterSamples { get; private set; }
        public IDictionary<string, IEnumerable<IQuantity<Decimal>>> GaugesSamples { get; private set; }
    }
}
