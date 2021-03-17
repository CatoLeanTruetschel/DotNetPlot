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

/* Based on
 * --------------------------------------------------------------------------------------------------------------------
 * C# Data Visualization (https://github.com/swharden/Csharp-Data-Visualization)
 * MIT License
 * 
 * Copyright (c) 2017 Scott W Harden
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * --------------------------------------------------------------------------------------------------------------------
 */

using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace DotNetPlot
{
    public sealed class FunctionLinePlot : LinePlotBase<FunctionLinePlot>
    {
        private double[]? _buffer;
        private readonly int _count;

        internal FunctionLinePlot(
            Plotter plotter,
            Func<double, double> function,
            double start,
            double end,
            double? step) : base(plotter)
        {
            Debug.Assert(plotter is not null);
            Debug.Assert(function is not null);

            var nonNullStep = step ?? (end - start) / 100; // TODO: Adapt step via error checking automatically.

            _count = (int)((end - start) / nonNullStep) + 1;
            _buffer = ArrayPool<double>.Shared.Rent(_count * 2);

            try
            {
                var yMin = double.MaxValue;
                var yMax = double.MinValue;
                var bufferXValues = _buffer.AsSpan(0, _count);
                var bufferYValues = _buffer.AsSpan(_count, _count);

                for (var i = 0; i < _count; i++)
                {
                    var x = Math.Min(start + i * nonNullStep, end);
                    var y = function(x);

                    bufferXValues[i] = x;
                    bufferYValues[i] = y;

                    yMin = Math.Min(y, yMin);
                    yMax = Math.Max(y, yMax);
                }

                AxisLimits = new AxisLimits(start, end, yMin, yMax);
            }
            catch
            {
                ArrayPool<double>.Shared.Return(_buffer);
                throw;
            }
        }

        internal FunctionLinePlot(
            Plotter plotter,
            Func<float, float> function,
            float start,
            float end,
            float? step) : this(plotter, ToFunctionOfDoubles(function)!, start, end, step)
        { }

        private static Func<double, double>? ToFunctionOfDoubles(Func<float, float>? function)
        {
            if (function is null)
                return null;

            return x => function((float)x);
        }

        protected override ReadOnlySpan<double> XValues => GetBufferOrThrow().AsSpan(0, _count);
        protected override ReadOnlySpan<double> YValues => GetBufferOrThrow().AsSpan(_count, _count);

        public override AxisLimits AxisLimits { get; }

        private double[] GetBufferOrThrow()
        {
            ThrowIfDisposed();
            return _buffer;
        }

        [MemberNotNull(nameof(_buffer))]
        private void ThrowIfDisposed()
        {
            if (_buffer is null)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && _count > 0)
            {
                var buffer = Interlocked.Exchange(ref _buffer, null);

                if (buffer is not null)
                {
                    ArrayPool<double>.Shared.Return(buffer);
                }
            }
        }

        protected override PlotValueMarker GetMarker()
        {
            // Function line plots cannot have markers.
            return PlotValueMarker.None;
        }
    }
}