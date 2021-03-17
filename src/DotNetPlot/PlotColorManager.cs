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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotNetPlot
{
    public sealed class PlotColorManager
    {
        private readonly IGraphicsPlotContext _plotContext;
        private readonly HashSet<Color> _allocatedColors;

        private PlotColorManager(IGraphicsPlotContext plotContext)
        {
            Debug.Assert(plotContext is not null);

            _plotContext = plotContext;
            _allocatedColors = GetAllocatedColors();
        }

        private HashSet<Color> GetAllocatedColors()
        {
            var plotter = _plotContext.Plotter;
            var axisColor = plotter.AxisColor ?? Color.DarkGray;
            var textColor = plotter.TextColor ?? axisColor;
            var allocatedColors = new HashSet<Color>() { plotter.ClearColor, axisColor, textColor };

            foreach (var plot in plotter.Plots)
            {
                if (plot.Color is not null)
                {
                    allocatedColors.Add(plot.Color.Value);
                }
            }

            return allocatedColors;
        }

        public Color GetPlotColor()
        {
            var result = Color.Transparent;
            var contrastRatio = 1f;

            foreach (var knownColor in KnownColors)
            {
                var knownColorLuminance = knownColor.GetBrightness();
                var minContrastRatio = 21f;

                foreach (var allocatedColor in _allocatedColors)
                {
                    var allocatedColorLuminance = allocatedColor.GetBrightness();

                    var lighterColor = Math.Max(knownColorLuminance, allocatedColorLuminance);
                    var darkerColor = Math.Min(knownColorLuminance, allocatedColorLuminance);
                    var currentContrastRation = (lighterColor + 0.05f) / (darkerColor + 0.05f);

                    minContrastRatio = Math.Min(minContrastRatio, currentContrastRation);
                }

                if (minContrastRatio > contrastRatio)
                {
                    result = knownColor;
                    contrastRatio = minContrastRatio;
                }
            }

            _allocatedColors.Add(result);
            return result;
        }

        private static readonly ConditionalWeakTable<IGraphicsPlotContext, PlotColorManager> _colorManagers = new();
        private static readonly ConditionalWeakTable<IGraphicsPlotContext, PlotColorManager>.CreateValueCallback _createColorManager 
            = CreateColorManager;

        private static ImmutableArray<Color> KnownColors { get; } = BuildKnownColors();

        private static ImmutableArray<Color> BuildKnownColors()
        {
            var properties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
            var resultBuilder = ImmutableArray.CreateBuilder<Color>(properties.Length);

            foreach (var property in properties)
            {
                if (property.PropertyType != typeof(Color))
                {
                    continue;
                }

                resultBuilder.Add((Color)property.GetValue(null)!);
            }

            return resultBuilder.ToImmutable();
        }

        public static PlotColorManager GetColorManager(IGraphicsPlotContext plotContext)
        {
            if (plotContext is null)
                throw new ArgumentNullException(nameof(plotContext));

            return _colorManagers.GetValue(plotContext, _createColorManager);
        }

        private static PlotColorManager CreateColorManager(IGraphicsPlotContext plotContext)
        {
            return new PlotColorManager(plotContext);
        }
    }
}
