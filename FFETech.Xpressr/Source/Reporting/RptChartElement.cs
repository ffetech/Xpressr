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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public class RptChartElement : RptElement
    {
        #region Constructors

        public RptChartElement(RptRange parent)
            : base(parent)
        {
            Width = 800;
            Height = 600;
            Margin = 50;
            Orientation = RptChartOrientation.Horizontal;

            CategoryAxis = new RptChartCategoryAxis(this);
            MeasureAxis = new RptChartMeasureAxis(this);
        }

        #endregion

        #region Properties

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

        public int Margin
        {
            get;
            protected set;
        }

        public RptChartOrientation Orientation
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

        public RptChartSeriesDef[] SeriesDef
        {
            get;
            protected set;
        }

        public string[] SeriesColors
        {
            get;
            protected set;
        }

        public RptChartCategoryAxis CategoryAxis
        {
            get;
            protected set;
        }

        public RptChartMeasureAxis MeasureAxis
        {
            get;
            protected set;
        }

        public RptChartLegend Legend
        {
            get;
            protected set;
        }

        internal RptColorSequence ColorSequence
        {
            get;
            private set;
        }

        #endregion

        #region Protected Methods

        protected override void DoRender(IRptDataSet dataSet, StringBuilder output)
        {
            ColorSequence = SeriesColors != null ? new RptColorSequence(SeriesColors.Select(color => RptColors.HEXToColor(color)).ToArray()) : new RptColorSequence();

            bool isPieChart = SeriesDef.All(series => series.Type == RptChartSeriesDef.SeriesType.Pie);

            string categoryAxis = isPieChart ? string.Empty : Orientation == RptChartOrientation.Horizontal ? "x" : "y";
            string measureAxis = isPieChart ? "p" : Orientation == RptChartOrientation.Horizontal ? "y" : "x";

            Dictionary<RptChartSeriesDef, IEnumerable<RptChartSeriesDef.DataValue>> dataValues = new Dictionary<RptChartSeriesDef, IEnumerable<RptChartSeriesDef.DataValue>>();

            foreach (RptChartSeriesDef seriesDef in SeriesDef)
                dataValues.Add(seriesDef, seriesDef.GenerateDataValues(dataSet));

            Func<int, string> indent = (n) => new string('\t', n);

            output.AppendLine(string.Format("<div>", indent(0)));
            output.AppendLine(string.Format("{0}<script src=\"{1}\"></script>", indent(1), "http://d3js.org/d3.v4.min.js"));
            output.AppendLine(string.Format("{0}<script src=\"{1}\"></script>", indent(1), "http://dimplejs.org/dist/dimple.v2.3.0.min.js"));
            output.AppendLine(string.Format("{0}<script type= \"text/javascript\">", indent(1)));

            output.AppendLine(string.Format("{0}var svg = dimple.newSvg(\"{1}\", \"{2}\", \"{3}\");", indent(2), "body", Width, Height));

            output.AppendLine(string.Format("{0}var data = [", indent(2)));

            foreach (RptChartSeriesDef seriesDef in SeriesDef)
                seriesDef.AppendJsonDataArray(output, dataValues[seriesDef], indent(3));

            output.AppendLine(string.Format("{0}]; ", indent(2)));

            output.AppendLine(string.Format("{0}var chart = new dimple.chart(svg, data);", indent(2)));
            //output.AppendLine(string.Format("{0}chart.setBounds({1}, {1}, {1}, {1});", indent(2), Margin));

            if (!isPieChart)
                CategoryAxis.Append(output, categoryAxis, indent(2));

            MeasureAxis.Append(output, measureAxis, indent(2));

            foreach (RptChartSeriesDef seriesDef in SeriesDef)
                seriesDef.AppendSeries(output, dataValues[seriesDef], indent(2));

            foreach (RptChartSeriesDef seriesDef in SeriesDef)
                seriesDef.AppendShowLabelsMethod(output, indent(2));

            if (Legend != null)
                Legend.Append(output, indent(2));

            int marginLeft = Margin;
            int marginTop = Margin;
            int marginRight = Margin;
            int marginBottom = Margin;

            if (Orientation == RptChartOrientation.Horizontal)
                marginBottom += CategoryAxis.Size;
            else
                marginLeft += CategoryAxis.Size;

            if (Orientation == RptChartOrientation.Horizontal)
                marginLeft += MeasureAxis.Size;
            else
                marginBottom += MeasureAxis.Size;

            output.AppendLine(string.Format("{0}chart.setMargins({1}, {2}, {3}, {4});", indent(2), marginLeft, marginTop, marginRight, marginBottom));

            output.AppendLine(string.Format("{0}chart.draw();", indent(2)));

            output.AppendLine(string.Format("{0}</script>", indent(1)));
            output.AppendLine(string.Format("{0}</div>", indent(0)));
        }

        #endregion
    }
}