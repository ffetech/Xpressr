/*

This file is part of Xpressr.

Copyright 2017 FFE-Tech e. U. Inh. Florian Feilmeier
website: www.ffe-tech.com
contact: info@ffe-tech.com

Xpressr is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Xpressr is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with Xpressr.  If not, see <http://www.gnu.org/licenses/>.

*/

using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public class RptChartLegend : RptExpressionTarget
    {
        #region Constructors

        public RptChartLegend(RptChartElement chart)
        {
            Chart = chart;
            FontFamily = chart.FontFamily;
            FontSize = chart.FontSize;
        }

        #endregion

        #region Properties

        public RptChartElement Chart
        {
            get;
            private set;
        }

        public int Left
        {
            get;
            protected set;
        }

        public int Top
        {
            get;
            protected set;
        }

        public int Width
        {
            get;
            protected set;
        }

        public int Height
        {
            get;
            protected set;
        }

        public string FontFamily
        {
            get;
            protected set;
        }

        public int FontSize
        {
            get;
            protected set;
        }

        #endregion

        #region Public Methods

        public void Append(StringBuilder output, string indent)
        {
            output.AppendLine(string.Format("{0}var legend = chart.addLegend({1}, {2}, {3}, {4});", indent, Left, Top, Width, Height));
            output.AppendLine(string.Format("{0}legend.fontFamily = \"{1}\"", indent, FontFamily));
            output.AppendLine(string.Format("{0}legend.fontSize = \"{1}pt\"", indent, FontSize));
        }

        #endregion

        #region Protected Methods

        protected override bool GetExpressionDefaultProperty(int index, out string propertyName)
        {
            switch (index)
            {
                case 0:
                    propertyName = "left";
                    return true;

                case 1:
                    propertyName = "top";
                    return true;

                case 2:
                    propertyName = "width";
                    return true;

                case 3:
                    propertyName = "height";
                    return true;
            }

            return base.GetExpressionDefaultProperty(index, out propertyName);
        }

        #endregion
    }
}