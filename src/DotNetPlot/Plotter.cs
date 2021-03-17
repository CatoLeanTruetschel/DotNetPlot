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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Threading;

namespace DotNetPlot
{
    public sealed class Plotter : PlotBase
    {
        private List<Plot>? _plots = new();

        public Plotter() { }

        public AxisLimits? AxisLimits { get; set; }

        public Color? AxisColor { get; set; }

        public Color? TextColor { get; set; }

        public string? Title { get; set; }

        public string? XLabel { get; set; }

        public string? YLabel { get; set; }

        public Color ClearColor { get; private set; } = Color.White;

        public override Plotter Clear()
        {
            ThrowIfDisposed();
            UncheckedClear(ClearColor);
            return this;
        }

        public override Plotter Clear(Color clearColor)
        {
            ThrowIfDisposed();
            UncheckedClear(clearColor);
            return this;
        }

        private void UncheckedClear(Color clearColor)
        {
            Debug.Assert(_plots is not null);

            ClearColor = clearColor;

            try
            {
                foreach (var plot in _plots)
                {
                    plot.Dispose();
                }
            }
            finally
            {
                _plots.Clear();
            }
        }

        internal TPlot RegisterPlot<TPlot>(TPlot plot)
            where TPlot : Plot
        {
            Debug.Assert(plot is not null);
            ThrowIfDisposed();
            _plots.Add(plot);
            return plot;
        }

        [MemberNotNull(nameof(_plots))]
        private void ThrowIfDisposed()
        {
            if (_plots is null)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                var plots = Interlocked.Exchange(ref _plots, null);

                if (plots is not null)
                {
                    foreach (var plot in plots)
                    {
                        plot.Dispose();
                    }
                }
            }
        }

        public TPlotContext Execute<TPlotContext>(IGraphicsPlotContextFactory<TPlotContext> plotContextFactory)
             where TPlotContext : IGraphicsPlotContext
        {
            if (plotContextFactory is null)
                throw new ArgumentNullException(nameof(plotContextFactory));

            var plotContext = plotContextFactory.Create(this);

            if (plotContext is null)
            {
                throw new InvalidOperationException();
            }

            Execute(plotContext);
            return plotContext;
        }

        public IEnumerable<Plot> Plots
        {
            get
            {
                ThrowIfDisposed();
                return _plots.ToImmutableList();
            }
        }

        private void Execute(IGraphicsPlotContext plotContext)
        {
            if (plotContext is null)
                throw new ArgumentNullException(nameof(plotContext));

            ThrowIfDisposed();

            // TODO: Use proper FP comparison
            if (plotContext.AxisLimits.XMax - plotContext.AxisLimits.XMin > 0
             && plotContext.AxisLimits.YMax - plotContext.AxisLimits.YMin > 0)
            {
                // Draw the coordinate system
                DrawCoordinateSystem(plotContext);

                // Draw each plot separately.
                foreach (var plot in _plots)
                {
                    plot.Execute(plotContext);
                }

                // Draw the title
                if (!string.IsNullOrWhiteSpace(Title))
                {
                    plotContext.DrawText(
                        Title,
                        TextColor ?? AxisColor ?? Color.DarkGray,
                        new Point(5, 5),
                        maxWidth: plotContext.Width / 4,
                        textSize: 2f);
                }
            }
        }

        private const int PADDING = 5;

        private void DrawCoordinateSystem(IGraphicsPlotContext plotContext)
        {
            var axisColor = AxisColor ?? Color.DarkGray;
            var textColor = TextColor ?? AxisColor ?? Color.DarkGray;
            var origin = plotContext.GetPixelFromLocation(0, 0);

            Rectangle xAxisLabelRect = new(plotContext.Width, origin.Y, 0, 0),
                      yAxisLabelRect = new(origin.X, 0, 0, 0);

            // Draw X-Axis label
            if (!string.IsNullOrWhiteSpace(XLabel))
            {
                xAxisLabelRect = plotContext.DrawText(
                    XLabel,
                    textColor,
                    new Point(plotContext.Width - PADDING, origin.Y - 2 * PADDING),
                    anchor: TextAnchor.BottomRight,
                    textSize: 1.1f,
                    style: TextStyles.Bold,
                    maxWidth: plotContext.Width / 8);
            }

            // Draw Y-Axis label
            if (!string.IsNullOrWhiteSpace(YLabel))
            {
                yAxisLabelRect = plotContext.DrawText(
                    YLabel,
                    textColor,
                    new Point(origin.X + 2 * PADDING, PADDING),
                    anchor: TextAnchor.TopLeft,
                    textSize: 1.1f,
                    style: TextStyles.Bold,
                    maxWidth: plotContext.Width / 8);
            }

            // X-Axis
            plotContext.DrawLine(axisColor, new Point(0, origin.Y), new Point(plotContext.Width, origin.Y));
            plotContext.DrawLine(axisColor, new Point(plotContext.Width - 5, origin.Y - 3), new Point(plotContext.Width, origin.Y));
            plotContext.DrawLine(axisColor, new Point(plotContext.Width - 5, origin.Y + 3), new Point(plotContext.Width, origin.Y));

            var xFactorPowerOfTen = 10;
            var xStep = double.NaN;
            for (; xFactorPowerOfTen > -10; xFactorPowerOfTen--)
            {
                xStep = Math.Pow(10, xFactorPowerOfTen);
                var pxPerUnitX = plotContext.Width / (plotContext.AxisLimits.XMax - plotContext.AxisLimits.XMin);

                if (xStep * pxPerUnitX <= plotContext.Width / 4)
                    break;
            }

            var i = 0;
            for (var x = xStep; x < plotContext.AxisLimits.XMax; x += xStep)
            {
                var p = plotContext.GetPixelFromLocation(x, y: 0);

                if (p.X > plotContext.Width - PADDING)
                    break;

                plotContext.DrawLine(axisColor, new Point(p.X, origin.Y - 2), new Point(p.X, origin.Y + 2));

                if (++i % 2 != 0)
                {
                    var text = Format(i, xFactorPowerOfTen);

                    plotContext.DrawText(
                        text,
                        textColor,
                        new Point(p.X, p.Y + PADDING),
                        TextAnchor.TopCenter,
                        condition: rect => rect.Right <= xAxisLabelRect.Left - PADDING);
                }
            }

            i = 0;
            for (var x = -xStep; x >= plotContext.AxisLimits.XMin; x -= xStep)
            {
                var p = plotContext.GetPixelFromLocation(x, y: 0);

                plotContext.DrawLine(axisColor, new Point(p.X, origin.Y - 2), new Point(p.X, origin.Y + 2));

                if (--i % 2 != 0)
                {
                    var text = Format(i, xFactorPowerOfTen);

                    plotContext.DrawText(
                        text,
                        textColor,
                        new Point(p.X, p.Y + PADDING),
                        TextAnchor.TopCenter,
                        condition: rect => rect.Left >= PADDING);
                }
            }

            // Y-Axis
            plotContext.DrawLine(axisColor, new Point(origin.X, 0), new Point(origin.X, plotContext.Height));
            plotContext.DrawLine(axisColor, new Point(origin.X - 3, 5), new Point(origin.X, 0));
            plotContext.DrawLine(axisColor, new Point(origin.X + 3, 5), new Point(origin.X, 0));

            var yFactorPowerOfTen = 10;
            var yStep = double.NaN;
            for (; yFactorPowerOfTen > -10; yFactorPowerOfTen--)
            {
                yStep = Math.Pow(10, yFactorPowerOfTen);
                var pxPerUnitY = plotContext.Height / (plotContext.AxisLimits.YMax - plotContext.AxisLimits.YMin);

                if (yStep * pxPerUnitY <= plotContext.Height / 4)
                    break;
            }

            i = 0;
            for (var y = yStep; y < plotContext.AxisLimits.YMax; y += yStep)
            {
                var p = plotContext.GetPixelFromLocation(x: 0, y);

                if (p.Y < PADDING)
                    break;

                plotContext.DrawLine(axisColor, new Point(origin.X - 2, p.Y), new Point(origin.X + 2, p.Y));

                if (++i % 2 != 0)
                {
                    var text = Format(i, yFactorPowerOfTen);
                    plotContext.DrawText(
                        text,
                        textColor,
                        new Point(p.X + PADDING, p.Y),
                        TextAnchor.CenterLeft,
                        condition: rect => rect.Top >= yAxisLabelRect.Bottom + PADDING);
                }
            }

            i = 0;
            for (var y = -yStep; y > plotContext.AxisLimits.YMin; y -= yStep)
            {
                var p = plotContext.GetPixelFromLocation(x: 0, y);

                plotContext.DrawLine(axisColor, new Point(origin.X - 2, p.Y), new Point(origin.X + 2, p.Y));

                if (--i % 2 != 0)
                {
                    var text = Format(i, yFactorPowerOfTen);
                    plotContext.DrawText(
                        text,
                        textColor,
                        new Point(p.X + PADDING, p.Y),
                        TextAnchor.CenterLeft,
                        condition: rect => rect.Bottom <= plotContext.Height + PADDING);
                }
            }
        }

        private static string Format(int value, int factorPowerOfTen)
        {
            if (value < 0)
            {
                return "-" + Format(-value, factorPowerOfTen);
            }

            var result = value.ToString(CultureInfo.InvariantCulture);

            if (factorPowerOfTen > 4)
            {
                result += "E+" + factorPowerOfTen.ToString(CultureInfo.InvariantCulture);
            }
            else if (-factorPowerOfTen - result.Length > 3)
            {
                result += "E-" + factorPowerOfTen.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                if (factorPowerOfTen > 0)
                {
                    result += new string('0', factorPowerOfTen);
                }
                else if (factorPowerOfTen < 0)
                {
                    result = new string('0', -factorPowerOfTen - result.Length + 1) + result;
                    var splitIndex = result.Length + factorPowerOfTen;
                    result = $"{result.Substring(0, splitIndex)}.{result[splitIndex..]}";
                }
            }

            return result;
        }
    }
}