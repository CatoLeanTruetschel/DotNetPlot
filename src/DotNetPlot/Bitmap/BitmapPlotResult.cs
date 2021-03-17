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

namespace DotNetPlot.Bitmap
{
    public sealed class BitmapPlotResult : PlotResult<System.Drawing.Bitmap>
    {
        private int _width;
        private int _height;
        private float _strokeWidth;

        internal BitmapPlotResult(PlotBase plot) : base(plot) { }

        public override System.Drawing.Bitmap Result => BuildResult();

        private System.Drawing.Bitmap BuildResult()
        {
            var contextFactory = new BitmapPlotContextFactory(new Size(_width, _height), _strokeWidth);
            var plotContext = Plotter.Execute(contextFactory);
            return plotContext.Result;
        }

        public int Width
        {
            get => _width;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _width = value;
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _height = value;
            }
        }

        public float StrokeWidth
        {
            get => _strokeWidth;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _strokeWidth = value;
            }
        }

        public BitmapPlotResult WithWidth(int width)
        {
            Width = width;
            return this;
        }

        public BitmapPlotResult WithHeight(int height)
        {
            Height = height;
            return this;
        }

        public BitmapPlotResult WithSize(int width, int height)
        {
            Width = width;
            Height = height;
            return this;
        }

        public BitmapPlotResult WithStrokeWidth(float strokeWidth)
        {
            StrokeWidth = strokeWidth;
            return this;
        }
    }
}
