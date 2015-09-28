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
    public interface ICounter : IQuantity<ConcurrentInt64>, IDisposable
    {
        /**
         * Increment the counter sample by 1.
         */
        void Increment();

        /**
         * Decrement the counter sample by 1.
         */
        void Decrement();

        /**
         * Increment the counter sample by the specified value.
         * 
         * @param value The value to increment the counter by.
         */
        void Increment(ConcurrentInt64 value);

        /**
         * Decrement the counter sample by the specified value.
         * 
         * @param value The value to decrement the counter by.
         */
        void Decrement(ConcurrentInt64 value);

        ConcurrentInt64 InitialValue { get; }
    }
}
