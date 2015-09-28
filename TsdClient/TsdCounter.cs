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
using Common.Logging;
using System.Runtime.CompilerServices;

namespace TsdClient
{
    
    internal class TsdCounter : Int64Quantity, ICounter
    {
        private volatile Boolean isOpen;
        private Boolean IsOpen { get { return isOpen; } set { isOpen = value; } }

        private ILog Log { get; set; }

        public string Name { get; private set; }
        public ConcurrentInt64 InitialValue { get; private set; }
        public TsdCounter(string name, ConcurrentInt64 value, IUnit unit, bool isOpen)
            : base(value.Clone(), unit)
        {
            Log = LogManager.GetLogger(typeof(TsdCounter));
            IsOpen = isOpen;
            Name = name;
            InitialValue = value.Clone();
        }

        public TsdCounter(string name, ConcurrentInt64 value, IUnit unit, bool isOpen, ILog logger)
            : base(value.Clone(), unit)
        {
            Log = logger;
            IsOpen = isOpen;
            Name = name;
            InitialValue = value.Clone();
        }

        public void Increment()
        {
            Increment(1);
        }
        public void Increment(ConcurrentInt64 value)
        {
            if (!IsOpen)
            {
                Log.Error(m=>m("Trying to operate a closed counter: {0}", Name));
            }
            Value.Increment(value);
        }
        public void Decrement()
        {
            Decrement(1);
        }
        public void Decrement(ConcurrentInt64 value)
        {
            Increment(-value);
        }

        public override string ToString()
        {
            return Name;
        }


        public void Dispose()
        {
            IsOpen = false;
        }
    }
}
