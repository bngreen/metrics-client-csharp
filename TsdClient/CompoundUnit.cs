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
    public class CompoundUnit : IUnit
    {
        public string Name { get; private set; }
        public CompoundUnit(string name)
        {
            Name = name;
            NumeratorUnits = new List<ImmediateUnit>();
            DenominatorUnits = new List<ImmediateUnit>();
        }
        public CompoundUnit AddNumeratorUnit(ImmediateUnit unit)
        {
            NumeratorUnits.Add(unit);
            return this;
        }
        public CompoundUnit AddDenominatorUnit(ImmediateUnit unit)
        {
            DenominatorUnits.Add(unit);
            return this;
        }
        public IList<ImmediateUnit> NumeratorUnits { get; private set; }
        public IList<ImmediateUnit> DenominatorUnits { get; private set; }
    }
}
