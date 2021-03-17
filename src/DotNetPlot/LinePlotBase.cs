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
using System.Drawing;

namespace DotNetPlot
{
    public abstract class LinePlotBase<TPlot> : Plot<TPlot>
         where TPlot : LinePlotBase<TPlot>
    {
        private protected LinePlotBase(Plotter plotter) : base(plotter)
        {
            Debug.Assert(this is TPlot);
        }

        public LinePlotType Type { get; set; }

        protected abstract ReadOnlySpan<double> XValues { get; }

        protected abstract ReadOnlySpan<double> YValues { get; }

        public TPlot WithType(LinePlotType type)
        {
            Type = type;
            return (TPlot)this;
        }

        public TPlot ResetType()
        {
            return WithType(default);
        }

        protected abstract PlotValueMarker GetMarker();

        internal sealed override void Execute(IGraphicsPlotContext plotContext)
        {
            Debug.Assert(plotContext is not null);

            var xValues = XValues;
            var yValues = YValues;
            var count = Math.Min(XValues.Length, YValues.Length);

            if (count == 0)
            {
                return;
            }

            var marker = GetMarker();
            var color = Color ?? PlotColorManager.GetColorManager(plotContext).GetPlotColor();
            var pointsBuffer = ArrayPool<Point>.Shared.Rent(count);

            try
            {
                var points = pointsBuffer.AsMemory(0, count);

                for (var i = 0; i < count; i++)
                    points.Span[i] = plotContext.GetPixelFromLocation(xValues[i], yValues[i]);

                if (Type == LinePlotType.CardinalSpline && count >= 3)
                {
                    plotContext.DrawCardinalSpline(color, points);
                }
                else
                {
                    plotContext.DrawPolyline(color, points);
                }

                if (marker == PlotValueMarker.Circle)
                {
                    foreach (var point in points.Span)
                    {
                        plotContext.DrawCircle(color, point, 5);
                    }
                }
                else if (marker == PlotValueMarker.Cross)
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
                LegendManager.GetLegendManager(plotContext).DrawLegend(color, marker, Name);
            }
        }
    }
}