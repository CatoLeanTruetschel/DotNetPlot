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
using System.Drawing;

namespace DotNetPlot.Sample
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Generate data
            var xsList = new List<double>();
            var ysList = new List<double>();
            for (var x = -6.0; x <= 4; x += .5)
            {
                xsList.Add(x);
                ysList.Add(Function3(x));
            }

            var function3XValues = xsList.ToArray();
            var function3YValues = ysList.ToArray();

            xsList.Clear();
            ysList.Clear();

            for (var x = -7.0; x <= 5; x += .1)
            {
                xsList.Add(x);
                ysList.Add(Function4(x));
            }

            var function4XValues = xsList.ToArray();
            var function4YValues = ysList.ToArray();

            var bitmap = new Plotter()
                .WithTitle("My custom fancy plot")
                .WithXLabel("x [sec]")
                .WithYLabel("y[m]")
                .Clear(clearColor: Color.White)
                .PlotLine(Function1, -3, 3, 0.25)
                    .WithName("Function 1")
                .PlotLine(Function2, -6, 4, 0.025)
                    .ResetName()
                    .WithType(LinePlotType.Polyline)
                .PlotLine(function3XValues, function3YValues)
                    .WithName("Function3")
                    .WithMarker(PlotValueMarker.Cross)
                    .WithType(LinePlotType.Polyline)
                .PlotScatter(function4XValues, function4YValues)
                    .WithName("Very very extra large function name that is to long to be displayed in the text rectangle.")
                    .WithMarker(PlotValueMarker.Circle)
                .AsBitmap()
                    .WithSize(1024, 768)
                    .WithStrokeWidth(1f)
                    .Result;

            bitmap.Save(@".\plot.png");
        }

        private static double Function1(double value)
        {
            return Math.Pow(value, 3) - 5 * value + 1.22;
        }

        private static double Function2(double value)
        {
            return 5 * value + Math.Sin(Math.PI * value - 0.22);
        }

        private static double Function3(double value)
        {
            return Math.Cos(2 * Math.PI * value) + 3.5;
        }

        private static readonly Random rnd = new Random();

        private static double Function4(double value)
        {
            return 7 * value * value + 3 * value - 3.5 + rnd.NextDouble() * 20;
        }
    }
}
