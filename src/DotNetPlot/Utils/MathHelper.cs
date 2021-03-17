/* License
 * --------------------------------------------------------------------------------------------------------------------
 * (C) Copyright 2021 Cato Léan Trütschel and contributors (https://github.com/CatoLeanTruetschel/DotNetPlot)
 *
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
 * --------------------------------------------------------------------------------------------------------------------
 */

using System;

namespace DotNetPlot.Utils
{
    internal static class MathHelper
    {
        // TODO: Vectorize
        public static double Min(in ReadOnlySpan<double> values)
        {
            if (values.Length == 0)
                return double.NaN;

            var result = values[0];

            for (var i = 1; i < values.Length; i++)
            {
                result = Math.Min(result, values[i]);
            }

            return result;
        }

        // TODO: Vectorize
        public static double Max(in ReadOnlySpan<double> values)
        {
            if (values.Length == 0)
                return double.NaN;

            var result = values[0];

            for (var i = 1; i < values.Length; i++)
            {
                result = Math.Max(result, values[i]);
            }

            return result;
        }
    }
}
