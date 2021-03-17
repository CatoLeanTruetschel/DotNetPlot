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
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace DotNetPlot
{
    public readonly struct AxisLimits : IEquatable<AxisLimits>
    {
        public AxisLimits(double xMin, double xMax, double yMin, double yMax)
        {
            // TODO: Check for NaN and inf

            if (xMin > xMax)
            {
                throw new ArgumentException("The specified x bounds must either span a range or be the same value.");
            }

            if (yMin > yMax)
            {
                throw new ArgumentException("The specified y bounds must either span a range or be the same value.");
            }

            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
        }

        public double XMin { get; }

        public double XMax { get; }

        public double YMin { get; }

        public double YMax { get; }

        bool IEquatable<AxisLimits>.Equals(AxisLimits other)
        {
            return Equals(in other);
        }

        public bool Equals(in AxisLimits other)
        {
            return XMin == other.XMin
                && XMax == other.XMax
                && YMin == other.YMin
                && YMax == other.YMax;
        }

        // TODO
        //public bool EqualsWithTolerance(in AxisLimits other, float tolerance = (float)MathHelper.DEFAULT_TOLERANCE)
        //{
        //    return MathHelper.AreEqualWithTolerance(SampleX, other.SampleX, tolerance)
        //        && MathHelper.AreEqualWithTolerance(SampleY, other.SampleY, tolerance);
        //}

        public override bool Equals(object? obj)
        {
            return obj is AxisLimits axisLimits && Equals(in axisLimits);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(XMin, XMax, YMin, YMax);
        }

        public static bool operator ==(in AxisLimits left, in AxisLimits right)
        {
            return left.Equals(in right);
        }

        public static bool operator !=(in AxisLimits left, in AxisLimits right)
        {
            return !left.Equals(in right);
        }

        public static AxisLimits Fit(IEnumerable<AxisLimits> axisLimits)
        {
            if (axisLimits is null)
                throw new ArgumentNullException(nameof(axisLimits));

            var axisLimitsArray = axisLimits.ToArray();

            var xMin = axisLimitsArray.Min(p => p.XMin);
            var xMax = axisLimitsArray.Max(p => p.XMax);
            var yMin = axisLimitsArray.Min(p => p.YMin);
            var yMax = axisLimitsArray.Max(p => p.YMax);

            return new AxisLimits(xMin, xMax, yMin, yMax);
        }
    }
}