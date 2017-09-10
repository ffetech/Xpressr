﻿/*

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
    public abstract class RptChartAxis : RptExpressionTarget
    {
        #region Constructors

        public RptChartAxis(RptChartElement chart)
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

        public string Title
        {
            get;
            protected set;
        }

        public int Size
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

        #region Protected Methods

        protected void AppendFormat(StringBuilder output, string axisName, string indent)
        {
            output.AppendLine(string.Format("{0}{1}.title = \"{2}\"", indent, axisName, Title));
            output.AppendLine(string.Format("{0}{1}.fontFamily = \"{2}\"", indent, axisName, FontFamily));
            output.AppendLine(string.Format("{0}{1}.fontSize = \"{2}pt\"", indent, axisName, FontSize));
        }

        #endregion
    }
}