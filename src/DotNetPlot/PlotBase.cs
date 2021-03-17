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
using System.Diagnostics;
using System.Drawing;

namespace DotNetPlot
{
    public abstract class PlotBase : IDisposable
    {
        private protected PlotBase()
        {
            Debug.Assert(this is Plotter);
            Plotter = (Plotter)this;
        }

        private protected PlotBase(Plotter plotter)
        {
            Debug.Assert(plotter is not null);
            Plotter = plotter;
        }

        public Plotter Plotter { get; }

        public Plotter WithAxisLimits(AxisLimits? axisLimits)
        {
            Plotter.AxisLimits = axisLimits;
            return Plotter;
        }

        public Plotter ResetAxisLimits()
        {
            return WithAxisLimits(null);
        }

        public Plotter WithAxisColor(Color? axisColor)
        {
            Plotter.AxisColor = axisColor;
            return Plotter;
        }

        public Plotter ResetAxisColor()
        {
            return WithAxisColor(null);
        }

        public Plotter WithTextColor(Color? textColor)
        {
            Plotter.TextColor = textColor;
            return Plotter;
        }

        public Plotter ResetTextColor()
        {
            return WithTextColor(null);
        }

        public Plotter WithTitle(string? title)
        {
            Plotter.Title = title;
            return Plotter;
        }

        public Plotter ResetTitle()
        {
            return WithTitle(null);
        }

        public Plotter WithXLabel(string? label)
        {
            Plotter.XLabel = label;
            return Plotter;
        }

        public Plotter ResetXLabel()
        {
            return WithXLabel(null);
        }

        public Plotter WithYLabel(string? label)
        {
            Plotter.YLabel = label;
            return Plotter;
        }

        public Plotter ResetYLabel()
        {
            return WithYLabel(null);
        }

        public virtual Plotter Clear()
        {
            return Plotter.Clear();
        }

        public virtual Plotter Clear(Color clearColor)
        {
            return Plotter.Clear(clearColor);
        }

        public LinePlot PlotLine(in ReadOnlySpan<double> xValues, in ReadOnlySpan<double> yValues)
        {
            return Plotter.RegisterPlot(new LinePlot(Plotter, xValues, yValues));
        }

        public LinePlot PlotLine(in ReadOnlySpan<float> xValues, in ReadOnlySpan<float> yValues)
        {
            return Plotter.RegisterPlot(new LinePlot(Plotter, xValues, yValues));
        }

        public FunctionLinePlot PlotLine(Func<double, double> function, double start, double end, double? step = null)
        {
            if (function is null)
                throw new ArgumentNullException(nameof(function));

            return Plotter.RegisterPlot(new FunctionLinePlot(Plotter, function, start, end, step));
        }

        public FunctionLinePlot PlotLine(Func<float, float> function, float start, float end, float? step = null)
        {
            if (function is null)
                throw new ArgumentNullException(nameof(function));

            return Plotter.RegisterPlot(new FunctionLinePlot(Plotter, function, start, end, step));
        }

        public ScatterPlot PlotScatter(in ReadOnlySpan<double> xValues, in ReadOnlySpan<double> yValues)
        {
            return Plotter.RegisterPlot(new ScatterPlot(Plotter, xValues, yValues));
        }

        public ScatterPlot PlotScatter(in ReadOnlySpan<float> xValues, in ReadOnlySpan<float> yValues)
        {
            return Plotter.RegisterPlot(new ScatterPlot(Plotter, xValues, yValues));
        }

        protected virtual void Dispose(bool disposing) { }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}