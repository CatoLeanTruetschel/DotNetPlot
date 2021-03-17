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
using System.Drawing;

namespace DotNetPlot
{
    public interface IGraphicsPlotContext
    {
        Plotter Plotter { get; }

        AxisLimits AxisLimits { get; }

        int Height { get; }

        int Width { get; }

        Point GetPixelFromLocation(double x, double y);

        Rectangle DrawCardinalSpline(Color color, ReadOnlyMemory<Point> points, float tension = 0.5F);

        Rectangle DrawEllipse(Color color, Rectangle rect);

        Rectangle DrawLine(Color color, Point pt1, Point pt2);

        Rectangle DrawText(
            string text,
            Color color,
            Point point,
            TextAnchor anchor = TextAnchor.TopLeft,
            float textSize = 1,
            TextStyles style = TextStyles.None,
            int? maxWidth = null,
            Func<Rectangle, bool>? condition = null);

        public Rectangle DrawCircle(Color color, Point midpoint, int size)
#if SUPPORTS_DEFAULT_INTERFACE_METHODS
        {
            return DrawEllipse(color, ComputeRectangle(midpoint, size));
        }
#else
        ;
#endif

        public Rectangle DrawCross(Color color, Point midpoint, int size)
#if SUPPORTS_DEFAULT_INTERFACE_METHODS
        {
            return DrawCross(color, ComputeRectangle(midpoint, size));
        }
#else 
        ;
#endif
        public Rectangle DrawCross(Color color, Rectangle rect)
#if SUPPORTS_DEFAULT_INTERFACE_METHODS
       {
            var topLeft = new Point(rect.Left, rect.Top);
            var topRight = new Point(rect.Right, rect.Top);
            var bottomLeft = new Point(rect.Left, rect.Bottom);
            var bottomRight = new Point(rect.Right, rect.Bottom);

            DrawLine(color, topLeft, bottomRight);
            DrawLine(color, bottomLeft, topRight);

            return rect;
        }
#else
        ;
#endif
        public Rectangle DrawPolyline(Color color, ReadOnlyMemory<Point> points)
#if SUPPORTS_DEFAULT_INTERFACE_METHODS
        {
            var top = points.Span[0].Y;
            var bottom = points.Span[0].Y;
            var left = points.Span[0].X;
            var right = points.Span[0].X;

            for (var i = 1; i < points.Length; i++)
            {
                DrawLine(color, points.Span[i - 1], points.Span[i]);

                top = Math.Min(top, points.Span[i].Y);
                bottom = Math.Max(bottom, points.Span[i].Y);
                left = Math.Min(left, points.Span[i].X);
                right = Math.Min(right, points.Span[i].X);
            }

            return Rectangle.FromLTRB(left, top, right, bottom);
        }
#else
        ;
#endif

#if SUPPORTS_DEFAULT_INTERFACE_METHODS
        private static Rectangle ComputeRectangle(Point midpoint, int size)
        {
            return new Rectangle(midpoint.X - size / 2, midpoint.Y - size / 2, size, size);
        }
#endif
    }
}