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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using DotNetPlot.Utils;

namespace DotNetPlot
{
    public sealed class ScatterPlot : Plot<ScatterPlot>
    {
        private double[]? _buffer;
        private readonly int _count;

        internal ScatterPlot(
            Plotter plotter,
            in ReadOnlySpan<double> xValues,
            in ReadOnlySpan<double> yValues) : base(plotter)
        {
            Debug.Assert(plotter is not null);

            _count = Math.Min(xValues.Length, yValues.Length);

            if (_count > 0)
            {
                _buffer = ArrayPool<double>.Shared.Rent(_count * 2);

                try
                {
                    var bufferXValues = _buffer.AsSpan(0, _count);
                    var bufferYValues = _buffer.AsSpan(_count, _count);

                    xValues[0.._count].CopyTo(bufferXValues);
                    yValues[0.._count].CopyTo(bufferYValues);

                    var xMin = MathHelper.Min(bufferXValues);
                    var xMax = MathHelper.Max(bufferXValues);
                    var yMin = MathHelper.Min(bufferYValues);
                    var yMax = MathHelper.Max(bufferYValues);

                    AxisLimits = new AxisLimits(xMin, xMax, yMin, yMax);
                }
                catch
                {
                    ArrayPool<double>.Shared.Return(_buffer);
                    throw;
                }
            }
        }

        internal ScatterPlot(
            Plotter plotter,
            in ReadOnlySpan<float> xValues,
            in ReadOnlySpan<float> yValues) : base(plotter)
        {
            Debug.Assert(plotter is not null);

            _count = Math.Min(xValues.Length, yValues.Length);

            if (_count > 0)
            {
                _buffer = ArrayPool<double>.Shared.Rent(_count * 2);

                try
                {
                    var bufferXValues = _buffer.AsSpan(0, _count);
                    var bufferYValues = _buffer.AsSpan(_count, _count);

                    for (var i = 0; i < _count; i++)
                    {
                        bufferXValues[i] = xValues[i];
                        bufferYValues[i] = yValues[i];
                    }

                    var xMin = MathHelper.Min(bufferXValues);
                    var xMax = MathHelper.Max(bufferXValues);
                    var yMin = MathHelper.Min(bufferYValues);
                    var yMax = MathHelper.Max(bufferYValues);

                    AxisLimits = new AxisLimits(xMin, xMax, yMin, yMax);
                }
                catch
                {
                    ArrayPool<double>.Shared.Return(_buffer);
                    throw;
                }
            }
        }

        public PlotValueMarker Marker { get; set; }

        public override AxisLimits AxisLimits { get; }

        private ReadOnlySpan<double> XValues => GetBufferOrThrow().AsSpan(0, _count);
        private ReadOnlySpan<double> YValues => GetBufferOrThrow().AsSpan(_count, _count);

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

        internal override void Execute(IGraphicsPlotContext plotContext)
        {
            if (Marker != PlotValueMarker.Circle && Marker != PlotValueMarker.Cross)
                return;

            var xValues = XValues;
            var yValues = YValues;
            var count = Math.Min(XValues.Length, YValues.Length);

            var color = Color ?? PlotColorManager.GetColorManager(plotContext).GetPlotColor();
            var pointsBuffer = ArrayPool<Point>.Shared.Rent(count);

            try
            {
                var points = pointsBuffer.AsMemory(0, count);

                for (var i = 0; i < count; i++)
                    points.Span[i] = plotContext.GetPixelFromLocation(xValues[i], yValues[i]);

                if (Marker == PlotValueMarker.Circle)
                {
                    foreach (var point in points.Span)
                    {
                        plotContext.DrawCircle(color, point, 5);
                    }
                }
                else if (Marker == PlotValueMarker.Cross)
                {
                    foreach (var point in points.Span)
                    {
                        plotContext.DrawCross(color, point, 5);
                    }
                }
            }
            finally
            {
                ArrayPool<Point>.Shared.Return(pointsBuffer);
            }

            if (Name is not null)
            {
                LegendManager.GetLegendManager(plotContext).DrawLegend(color, Marker, Name, drawLine: false);
            }
        }

        public ScatterPlot WithMarker(PlotValueMarker marker)
        {
            Marker = marker;
            return this;
        }

        public ScatterPlot ResetMarker()
        {
            return WithMarker(PlotValueMarker.None);
        }
    }
}
