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
    public class ConcurrentDecimal
    {
        public ConcurrentDecimal()
        {

        }
        public ConcurrentDecimal(Decimal value)
        {
            Value = value;
        }

        private Decimal value;
        public Decimal Value
        {
            get
            {
                lock (this)
                {
                    return value;
                }
            }
            set
            {
                lock (this)
                {
                    this.value = value;
                }
            }
        }

        public void Increment()
        {
            Increment(1);
        }
        public void Increment(ConcurrentDecimal other)
        {
            lock (this)
            {
                value += other.Value;
            }
        }
        public void Decrement()
        {
            Increment(-1);
        }
        public void Decrement(ConcurrentDecimal other)
        {
            Increment(-other.Value);
        }

        public static implicit operator ConcurrentDecimal(Decimal number)
        {
            return new ConcurrentDecimal(number);
        }
        public static implicit operator Decimal(ConcurrentDecimal number)
        {
            return number.Value;
        }
        public static explicit operator ConcurrentDecimal(ConcurrentInt64 number)
        {
            return new ConcurrentDecimal(number.Value);
        }
        public static explicit operator ConcurrentInt64(ConcurrentDecimal number)
        {
            return new ConcurrentInt64((Int64)number.Value);
        }
        public override string ToString()
        {
            return Value.ToString();
        }


        public decimal ToDecimal()
        {
            return Value;
        }
    }
}
