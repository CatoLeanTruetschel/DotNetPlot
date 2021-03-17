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

using System.Diagnostics;
using System.Drawing;

namespace DotNetPlot
{
    public abstract class Plot : PlotBase
    {
        private protected Plot(Plotter plotter) : base(plotter) { }

        public abstract AxisLimits AxisLimits { get; }

        public Color? Color { get; set; }

        public string? Name { get; set; }

        internal abstract void Execute(IGraphicsPlotContext plotContext);
    }

    public abstract class Plot<TPlot> : Plot
       where TPlot : Plot<TPlot>
    {
        private protected Plot(Plotter plotter) : base(plotter)
        {
            Debug.Assert(this is TPlot);
        }

        public TPlot WithColor(Color? color)
        {
            Color = color;
            return (TPlot)this;
        }

        public TPlot ResetColor()
        {
            return WithColor(null);
        }

        public TPlot WithName(string? name)
        {
            Name = name;
            return (TPlot)this;
        }

        public TPlot ResetName()
        {
            return WithName(null);
        }
    }
}