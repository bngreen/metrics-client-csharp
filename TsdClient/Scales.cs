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
    public class Scales
    {
        public static Scale Yocto { get; private set; }
        public static Scale Zepto { get; private set; }
        public static Scale Femto { get; private set; }
        public static Scale Pico { get; private set; }
        public static Scale Nano { get; private set; }
        public static Scale Micro { get; private set; }
        public static Scale Milli { get; private set; }
        public static Scale Centi { get; private set; }
        public static Scale Deci { get; private set; }
        public static Scale Deca { get; private set; }
        public static Scale Hecto { get; private set; }
        public static Scale Kilo { get; private set; }
        public static Scale Mega { get; private set; }
        public static Scale Giga { get; private set; }
        public static Scale Tera { get; private set; }
        public static Scale Peta { get; private set; }
        public static Scale Exa { get; private set; }
        public static Scale Zetta { get; private set; }
        public static Scale Yotta { get; private set; }
        public static Scale Kibi { get; private set; }
        public static Scale Mebi { get; private set; }
        public static Scale Gibi { get; private set; }
        public static Scale Tebi { get; private set; }
        public static Scale Pebi { get; private set; }
        public static Scale Exbi { get; private set; }
        public static Scale Zebi { get; private set; }
        public static Scale Yobi { get; private set; }
        static Scales()
        {
            Yocto = new Scale("yocto");
            Zepto = new Scale("zepto");
            Femto = new Scale("femto");
            Pico = new Scale("pico");
            Nano = new Scale("nano");
            Micro = new Scale("micro");
            Milli = new Scale("milli");
            Centi = new Scale("centi");
            Deci = new Scale("deci");
            Deca = new Scale("deca");
            Hecto = new Scale("hecto");
            Kilo = new Scale("kilo");
            Mega = new Scale("mega");
            Giga = new Scale("giga");
            Tera = new Scale("tera");
            Peta = new Scale("peta");
            Exa = new Scale("exa");
            Zetta = new Scale("zetta");
            Yotta = new Scale("yotta");
            Kibi = new Scale("kibi");
            Mebi = new Scale("mebi");
            Gibi = new Scale("gibi");
            Tebi = new Scale("tebi");
            Pebi = new Scale("pebi");
            Exbi = new Scale("exbi");
            Zebi = new Scale("zebi");
            Yobi = new Scale("yobi");
        }
    }
}
