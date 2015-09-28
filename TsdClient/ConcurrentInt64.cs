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
using System.Threading;
using System.Threading.Tasks;

namespace TsdClient
{
    public class ConcurrentInt64 : ConcurrentNumber, IEquatable<ConcurrentInt64>
    {
        public ConcurrentInt64()
        {

        }
        public ConcurrentInt64(Int64 value)
        {
            Value = value;
        }
        private Int64 value;
        public Int64 Value
        {
            get
            {
                return Interlocked.Read(ref value);
            }
            set
            {
                this.value = value;
            }
        }

        public ConcurrentInt64 Clone()
        {
            return new ConcurrentInt64(Value);
        }

        public void Increment()
        {
            Increment(1);
        }

        public void Increment(ConcurrentInt64 other)
        {
            Value = Interlocked.Add(ref value, other.Value);
        }

        public void Decrement()
        {
            Increment(-1);
        }
        public void Decrement(ConcurrentInt64 other)
        {
            Increment(-other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
        public static implicit operator ConcurrentInt64(Int64 v)
        {
            return new ConcurrentInt64(v);
        }
        public static implicit operator Int64(ConcurrentInt64 v)
        {
            return v.Value;
        }

        public decimal ToDecimal()
        {
            return new Decimal(Value);
        }

        public bool Equals(ConcurrentInt64 other)
        {
            return Value == other.Value;
        }
    }
}
