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

namespace TsdClient.JSONLogFormat
{
    public class JsonLogSerializerFactory
    {
        public static Versions LatestVersion { get { return Versions.Version2F; } }

        public static IJsonLogSerializer CreateSerializer()
        {
            return CreateSerializer(LatestVersion);
        }
        public static IJsonLogSerializer CreateSerializer(Versions version)
        {
            switch (version)
            {
                case Versions.Version2F:
                    return new TsdClient.JSONLogFormat.Version2F.JsonLogSerializer();
                default:
                    throw new InvalidOperationException("Invalid Version");
            }
        }
    }
}
