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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotNetPlot.Bitmap
{
    public sealed class BitmapPlotContext : IGraphicsPlotContext, IDisposable
    {
        private readonly Size _size;
        private readonly float _strokeWidth;
#pragma warning disable IDE0079, CA2213 
        private Graphics? _graphics;
        private Font? _font;
#pragma warning restore IDE0079, CA2213

        internal BitmapPlotContext(Size size, float strokeWidth, Plotter plotter)
        {
            Debug.Assert(plotter is not null);

            _size = size;
            _strokeWidth = strokeWidth;

            Plotter = plotter;
            AxisLimits = plotter.AxisLimits ?? AxisLimits.Fit(plotter.Plots.Select(p => p.AxisLimits));
            Result = new System.Drawing.Bitmap(Width, Height);
            _graphics = Graphics.FromImage(Result);
            _graphics.Clear(plotter.ClearColor);
            _graphics.SmoothingMode = SmoothingMode.AntiAlias;
            _font = new Font("Arial", emSize: 12, GraphicsUnit.Pixel);
        }

        private Color GetAxisColor() => Plotter.AxisColor ?? Color.DarkGray;

        private Color GetTextColor() => Plotter.TextColor ?? GetAxisColor();

        public Plotter Plotter { get; }

        public System.Drawing.Bitmap Result { get; }

        public int Width => _size.Width;

        public int Height => _size.Height;

        public AxisLimits AxisLimits { get; }

        public Point GetPixelFromLocation(double x, double y)
        {
            var pxPerUnitX = Width / (AxisLimits.XMax - AxisLimits.XMin);
            var pxPerUnitY = Height / (AxisLimits.YMax - AxisLimits.YMin);
            var xPx = (int)((x - AxisLimits.XMin) * pxPerUnitX);
            var yPx = Height - (int)((y - AxisLimits.YMin) * pxPerUnitY);
            return new Point(xPx, yPx);
        }

        public Rectangle DrawLine(Color color, Point pt1, Point pt2)
        {
            ThrowIfDisposed();

            using var pen = CreatePen(color);
            _graphics.DrawLine(pen, pt1, pt2);

            return Rectangle.FromLTRB(
                Math.Min(pt1.X, pt2.X),
                Math.Min(pt1.Y, pt2.Y),
                Math.Max(pt1.X, pt2.X),
                Math.Max(pt1.Y, pt2.Y));
        }

        public Rectangle DrawPolyline(Color color, ReadOnlyMemory<Point> points)
        {
            ThrowIfDisposed();

            using var pen = CreatePen(color);
            var top = points.Span[0].Y;
            var bottom = points.Span[0].Y;
            var left = points.Span[0].X;
            var right = points.Span[0].X;

            if (MemoryMarshal.TryGetArray(points, out var arraySegment) && arraySegment.Array is not null)
            {
                if (arraySegment.Offset == 0 && arraySegment.Count == arraySegment.Array.Length)
                {
                    _graphics.DrawLines(pen, arraySegment.Array);

                    for (var i = 1; i < points.Length; i++)
                    {
                        top = Math.Min(top, points.Span[i].Y);
                        bottom = Math.Max(bottom, points.Span[i].Y);
                        left = Math.Min(left, points.Span[i].X);
                        right = Math.Min(right, points.Span[i].X);
                    }

                    return Rectangle.FromLTRB(left, top, right, bottom);
                }
            }

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

        private Pen CreatePen(Color color)
        {
            return new Pen(color)
            {
                LineJoin = LineJoin.Round,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                Width = _strokeWidth
            };
        }

        public Rectangle DrawCardinalSpline(Color color, ReadOnlyMemory<Point> points, float tension)
        {
            ThrowIfDisposed();

            using var pen = CreatePen(color);

            if (MemoryMarshal.TryGetArray(points, out var arraySegment) && arraySegment.Array is not null)
            {
                if (arraySegment.Offset == 0 && arraySegment.Count == arraySegment.Array.Length)
                {
                    _graphics.DrawCurve(pen, arraySegment.Array, tension);
                }
                else
                {
                    // The void Graphics.DrawCurve(Pen, Point[], int, int, float)
                    // silently ignores offset and numberOfSegments, so work around this to prevent the large array copy
                    // and allocation.
                    DrawCardinalSpline(pen, arraySegment, tension);
                    //_graphics.DrawCurve(pen, arraySegment.Array, arraySegment.Offset, arraySegment.Count, tension);
                }
            }
            else
            {
                _graphics.DrawCurve(pen, points.ToArray(), tension);
            }

            // TODO: this is not correct actually, as the curve may get out of this bounding box.

            var top = points.Span[0].Y;
            var bottom = points.Span[0].Y;
            var left = points.Span[0].X;
            var right = points.Span[0].X;

            for (var i = 1; i < points.Length; i++)
            {
                top = Math.Min(top, points.Span[i].Y);
                bottom = Math.Max(bottom, points.Span[i].Y);
                left = Math.Min(left, points.Span[i].X);
                right = Math.Min(right, points.Span[i].X);
            }

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

#if ALLOW_UNSAFE
        [SkipLocalsInit]
#endif
        private void DrawCardinalSpline(Pen pen, ArraySegment<Point> points, float tension)
        {
            Debug.Assert(_graphics is not null);

            if (points.Count == 0)
                return;

            // 4kB
            const int MaxStackAllocationBytes = 1024 * 4;
#if ALLOW_UNSAFE
            int sizeOfPoint;
            unsafe { sizeOfPoint = sizeof(Point); }
#else
            var sizeOfPoint =   Unsafe.SizeOf<Point>();
#endif
            Debug.Assert(points.Array is not null);

            var rentedBuffer = default(Point[]);
            var bufferSize = points.Array.Length - points.Count;
            var buffer =
                checked(bufferSize * sizeOfPoint) > MaxStackAllocationBytes
                ? (rentedBuffer = ArrayPool<Point>.Shared.Rent(bufferSize))
                : stackalloc Point[bufferSize];

            try
            {
                var pointsLeading = points.Array.AsSpan(0, points.Offset);
                var pointsTrailing = points.Array.AsSpan(points.Offset + points.Count);

                var bufferLeading = buffer[0..points.Offset];
                var bufferTrailing = buffer[points.Offset..];

                pointsLeading.CopyTo(bufferLeading);
                pointsTrailing.CopyTo(bufferTrailing);

                pointsLeading.Fill(points.Array[points.Offset]);
                pointsTrailing.Fill(points.Array[points.Offset + points.Count - 1]);

                _graphics.DrawCurve(pen, points.Array, tension);

                bufferLeading.CopyTo(pointsLeading);
                bufferTrailing.CopyTo(pointsTrailing);
            }
            finally
            {
                if (rentedBuffer is not null)
                {
                    ArrayPool<Point>.Shared.Return(rentedBuffer);
                }
            }
        }

        public Rectangle DrawEllipse(Color color, Rectangle rect)
        {
            ThrowIfDisposed();

            using var pen = CreatePen(color);

            _graphics.DrawEllipse(pen, rect);
            return rect;
        }

#if !SUPPORTS_DEFAULT_INTERFACE_METHODS
        Rectangle IGraphicsPlotContext.DrawCircle(Color color, Point midpoint, int size)
        {
            return DrawEllipse(color, ComputeRectangle(midpoint, size));
        }

        Rectangle IGraphicsPlotContext.DrawCross(Color color, Point midpoint, int size)
        {
            return ((IGraphicsPlotContext)this).DrawCross(color, ComputeRectangle(midpoint, size));
        }

        Rectangle IGraphicsPlotContext.DrawCross(Color color, Rectangle rect)

        {
            var topLeft = new Point(rect.Left, rect.Top);
            var topRight = new Point(rect.Right, rect.Top);
            var bottomLeft = new Point(rect.Left, rect.Bottom);
            var bottomRight = new Point(rect.Right, rect.Bottom);

            DrawLine(color, topLeft, bottomRight);
            DrawLine(color, bottomLeft, topRight);

            return rect;
        }

        private static Rectangle ComputeRectangle(Point midpoint, int size)
        {
            return new Rectangle(midpoint.X - size / 2, midpoint.Y - size / 2, size, size);
        }
#endif

        public Rectangle DrawText(
            string text,
            Color color,
            Point point,
            TextAnchor anchor,
            float textSize,
            TextStyles style,
            int? maxWidth,
            Func<Rectangle, bool>? condition)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            ThrowIfDisposed();

            var allocatedFont = default(Font);

            try
            {
                var font = _font;
                // TODO: use proper FP comparison
                if (textSize != 1 || style != TextStyles.None)
                {
                    var fontStyle = (FontStyle)style;
                    allocatedFont = font = new Font(_font.FontFamily, _font.Size * textSize, fontStyle, _font.Unit);
                }

                if (maxWidth is not null)
                {
                    text = ShortenToMaxSize(text, font, maxWidth.Value);
                }

                var nativeTextAnchor = GetNativeTextAnchor(text, font, point, anchor);
                var rect = Rectangle.Ceiling(new RectangleF(nativeTextAnchor, _graphics.MeasureString(text, font)));

                if (condition is not null && !condition(rect))
                {
                    return default;
                }

                using var brush = new SolidBrush(color);
                _graphics.DrawString(text, font, brush, nativeTextAnchor);

                return rect;
            }
            finally
            {
                allocatedFont?.Dispose();
            }
        }

        private string ShortenToMaxSize(string str, Font font, int maxWidth)
        {
            Debug.Assert(_graphics is not null);

            if (Math.Ceiling(_graphics.MeasureString(str, font).Width) <= maxWidth)
            {
                return str;
            }

            var threeDots = "...";
            var current = str[0..^1];

            while (Math.Ceiling(_graphics.MeasureString(current + threeDots, font).Width) > maxWidth)
            {
                current = current[0..^1];
            }

            return current + threeDots;
        }

        private PointF GetNativeTextAnchor(string text, Font font, Point point, TextAnchor anchor)
        {
            Debug.Assert(_graphics is not null);

            if (anchor == TextAnchor.TopLeft)
            {
                return point;
            }

            var textSize = _graphics.MeasureString(text, font);
            var x = (float)point.X;
            var y = (float)point.Y;

            // Horizontally center aligned.
            if (anchor is TextAnchor.TopCenter or TextAnchor.Center or TextAnchor.BottomCenter)
            {
                x -= (textSize.Width / 2);
            }

            // Horizontally right aligned.
            else if (anchor is TextAnchor.TopRight or TextAnchor.CenterRight or TextAnchor.BottomRight)
            {
                x -= textSize.Width;
            }

            // Vertically center aligned.
            if (anchor is TextAnchor.CenterLeft or TextAnchor.Center or TextAnchor.CenterRight)
            {
                y -= (textSize.Height / 2);
            }
            // Vertically bottom aligned.
            else if (anchor is TextAnchor.BottomLeft or TextAnchor.BottomCenter or TextAnchor.BottomRight)
            {
                y -= textSize.Height;
            }

            return new(x, y);
        }

        [MemberNotNull(nameof(_font))]
        [MemberNotNull(nameof(_graphics))]
        private void ThrowIfDisposed()
        {
            if (_font is null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (_graphics is null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            var font = Interlocked.Exchange(ref _font, null);
            var graphics = Interlocked.Exchange(ref _graphics, null);

            font?.Dispose();
            graphics?.Dispose();
        }
    }
}
