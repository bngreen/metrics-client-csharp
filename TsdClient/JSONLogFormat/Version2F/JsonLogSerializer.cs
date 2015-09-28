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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsdClient.JSONLogFormat.Version2F
{
    public class JsonLogSerializer : IJsonLogSerializer
    {
        protected static void NormalizeUnit(IUnit unit, IList<ImmediateUnit> numerator, IList<ImmediateUnit> denominator)
        {
            var cU = unit as CompoundUnit;
            if (cU != null)
            {
                foreach (var x in cU.NumeratorUnits)
                    NormalizeUnit(x, numerator, denominator);
                foreach (var x in cU.DenominatorUnits)
                    NormalizeUnit(x, denominator, numerator);
            }
            else
                numerator.Add(unit as ImmediateUnit);
        }

        protected static IEnumerable<JProperty> SerializeUnit(IUnit unit)
        {
            if (unit == null)
                return new JProperty[0];
            var numerator = new List<ImmediateUnit>();
            var denominator = new List<ImmediateUnit>();
            NormalizeUnit(unit, numerator, denominator);
            Func<ImmediateUnit, JObject> sUnit = u =>
            {
                var lst = new List<JProperty>();
                if (u.Scale != null)
                    lst.Add(new JProperty("scale", u.Scale.Name));
                lst.Add(new JProperty("unit", u.Name));
                return new JObject(lst);
            };
            return new JProperty[] { 
                new JProperty("unitNumerators", 
                        new JArray(
                                from x in numerator select sUnit(x)
                            )
                    ),
                new JProperty("unitDenominators", 
                        new JArray(
                                from x in denominator select sUnit(x)
                            )
                    )
            };
        }

        private static JObject SerializeEntry<T>(IDictionary<string, IEnumerable<T>> values, Func<T, JObject> serializer)
        {
            return new JObject(
                from x in values select
                    new JProperty(x.Key,
                        new JObject(
                            new JProperty("values",
                                new JArray(
                                    from y in x.Value select serializer(y) 
                                )  
                            )
                        )
                    )
            );
        }

        private static JObject SerializeQuantity<T, T2>(IQuantity<T> val, Func<T, T2> mapper) where T:IEquatable<T>
        {
            return new JObject(
                new JProperty("value", mapper(val.Value)),
                SerializeUnit(val.Unit)
            );
        }

        private static JObject SerializeQuantity<T>(IQuantity<T> val) where T:IEquatable<T>
        {
            return SerializeQuantity(val, x => x);
        }

        public string Serialize(Sample sample)
        {
            return new JObject(
                new JProperty("version", "2f"),
                new JProperty("annotations", new JObject(
                    from x in sample.Annotations select new JProperty(x.Key, x.Value)
                )),
                new JProperty("timers",
                    SerializeEntry(sample.TimerSamples, x => SerializeQuantity(x, y => y.Value))
                ),
                new JProperty("gauges",
                    SerializeEntry(sample.GaugesSamples, SerializeQuantity)
                ),
                new JProperty("counters",
                    SerializeEntry(sample.CounterSamples, x => SerializeQuantity(x, y => y.Value))
                )
            ).ToString(Newtonsoft.Json.Formatting.None);
        }

    }
}
