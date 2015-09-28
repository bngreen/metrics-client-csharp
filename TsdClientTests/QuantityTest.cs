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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsdClient;

namespace TsdClientTests
{
    [TestFixture]
    public class QuantityTest
    {
        [Test]
        public void testQuantity()
        {
            long expectedValue = 1;
            IUnit expectedUnit = Units.Byte;
            var q = new Int64Quantity(expectedValue, expectedUnit);
            Assert.AreEqual(expectedValue, q.Value);
            Assert.AreEqual(expectedUnit, q.Unit);
        }

        [Test]
        public void testToString()
        {
            String asString = new Int64Quantity(1, Units.Byte).ToString();
            Assert.NotNull(asString);
            Assert.False(String.IsNullOrEmpty(asString));
        }
    }
}
