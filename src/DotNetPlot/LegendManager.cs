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
using System.Runtime.CompilerServices;

namespace DotNetPlot
{
    public sealed class LegendManager
    {
        const int LEGEND_HORIZONTAL_LINE_SIZE = 26;
        const int LEGEND_HORIZONTAL_TEXT_SIZE = 150;
        const int PADDING = 5;
        const int WIDTH = PADDING + LEGEND_HORIZONTAL_LINE_SIZE + LEGEND_HORIZONTAL_TEXT_SIZE;

        private readonly IGraphicsPlotContext _plotContext;
        private readonly int _x;
        private Rectangle _lastLegendRect;

        private LegendManager(IGraphicsPlotContext plotContext)
        {
            Debug.Assert(plotContext is not null);

            _plotContext = plotContext;
            _x = _plotContext.Width - PADDING - WIDTH;
            _lastLegendRect = new Rectangle(_x, y: PADDING, WIDTH, height: 0);
        }

        public void DrawLegend(Color color, PlotValueMarker marker, string name, bool drawLine = true)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            // Align top. Go bottom two padding, the space the already drawn legends are rendered at.
            var y = _lastLegendRect.Bottom + PADDING;

            var textColor = _plotContext.Plotter.TextColor ?? _plotContext.Plotter.AxisColor ?? Color.DarkGray;
            var lineStartX = _x;
            var lineMidpointX = _x + (LEGEND_HORIZONTAL_LINE_SIZE / 2);
            var lineEndX = _x + LEGEND_HORIZONTAL_LINE_SIZE;
            var textRect = _plotContext.DrawText(
                name,
                textColor,
                new Point(lineEndX + PADDING, y),
                TextAnchor.TopLeft,
                maxWidth: LEGEND_HORIZONTAL_TEXT_SIZE);
            var height = textRect.Height;
            var lineY = y + height / 2;

            if (drawLine)
            {
                _plotContext.DrawLine(color, new Point(lineStartX, lineY), new Point(lineEndX, lineY));
            }

            if (marker == PlotValueMarker.Circle)
            {
                _plotContext.DrawCircle(color, new Point(lineMidpointX, lineY), 5);
            }
            else if (marker == PlotValueMarker.Cross)
            {
                _plotContext.DrawCross(color, new Point(lineMidpointX, lineY), 5);
            }

            _lastLegendRect = new Rectangle(_x, y, WIDTH, height);
        }

        private static readonly ConditionalWeakTable<IGraphicsPlotContext, LegendManager> _legendManagers = new();
        private static readonly ConditionalWeakTable<IGraphicsPlotContext, LegendManager>.CreateValueCallback _createLegendManager
            = CreateLegendManager;

        public static LegendManager GetLegendManager(IGraphicsPlotContext plotContext)
        {
            if (plotContext is null)
                throw new ArgumentNullException(nameof(plotContext));

            return _legendManagers.GetValue(plotContext, _createLegendManager);
        }

        private static LegendManager CreateLegendManager(IGraphicsPlotContext plotContext)
        {
            return new LegendManager(plotContext);
        }
    }
}
