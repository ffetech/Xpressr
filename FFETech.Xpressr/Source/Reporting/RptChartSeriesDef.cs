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
using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public class RptChartSeriesDef : RptExpressionTarget
    {
        #region Nested Types

        public enum SeriesType
        {
            Bar, StackBar, Line, Surface, Pie
        }

        public struct DataValue
        {
            #region Properties

            public string CategoryId
            {
                get;
                set;
            }

            public string CategoryTitle
            {
                get;
                set;
            }

            public string SeriesId
            {
                get;
                set;
            }

            public string SeriesTitle
            {
                get;
                set;
            }

            public int? SeriesColor
            {
                get;
                set;
            }

            public double Value
            {
                get;
                set;
            }

            #endregion
        }

        #endregion

        #region Fields

        private RptScriptProvider selection;

        #endregion

        #region Constructors

        public RptChartSeriesDef(RptChartElement chart)
        {
            Chart = chart;
        }

        #endregion

        #region Properties

        public RptChartElement Chart
        {
            get;
            private set;
        }

        public SeriesType Type
        {
            get;
            protected set;
        }

        public string DataSet
        {
            get;
            protected set;
        }

        public string Selection
        {
            get
            {
                return selection != null ? selection.ToString() : string.Empty;
            }
            protected set
            {
                selection = new RptScriptProvider(value);
            }
        }

        public string CategoryField
        {
            get;
            protected set;
        }

        public string CategoryTitleField
        {
            get;
            protected set;
        }

        public string SeriesField
        {
            get;
            protected set;
        }

        public string SeriesTitleField
        {
            get;
            protected set;
        }

        public string SeriesValue
        {
            get;
            protected set;
        }

        public string ValueField
        {
            get;
            protected set;
        }

        public bool ShowValues
        {
            get;
            protected set;
        }

        #endregion

        #region Public Methods

        public IEnumerable<DataValue> GenerateDataValues(IRptDataSet dataSet)
        {
            List<DataValue> result = new List<DataValue>();
            HashSet<string> seriesSet = new HashSet<string>();

            if (!dataSet.TryGetDataSet(DataSet, out dataSet))
                throw new RptDataException($"Unknown Dataset '{DataSet}'");

            while (dataSet.ReadRecord())
            {
                if (selection != null)
                {
                    object selected = selection.Execute(dataSet);
                    if (!(selected is bool) || !(bool)selected)
                        continue;
                }

                DataValue dataValue = new DataValue();

                dataValue.CategoryId = ((!string.IsNullOrEmpty(CategoryField) ? dataSet.CurrentRecord.GetValue(CategoryField) : null) ?? string.Empty).ToString();
                dataValue.CategoryTitle = ((!string.IsNullOrEmpty(CategoryTitleField) ? dataSet.CurrentRecord.GetValue(CategoryTitleField) : null) ?? dataValue.CategoryId).ToString();

                dataValue.SeriesId = ((!string.IsNullOrEmpty(SeriesField) ? dataSet.CurrentRecord.GetValue(SeriesField) : null) ?? string.Empty).ToString();
                dataValue.SeriesTitle = SeriesValue ?? (((!string.IsNullOrEmpty(SeriesTitleField) ? dataSet.CurrentRecord.GetValue(SeriesTitleField) : null) ?? dataValue.SeriesId).ToString());

                if (dataValue.SeriesTitle != null)
                    dataValue.SeriesTitle = dataValue.SeriesTitle.Replace("<br>", "");

                if (!seriesSet.Contains(dataValue.SeriesId))
                {
                    seriesSet.Add(dataValue.SeriesId);
                    dataValue.SeriesColor = Chart.ColorSequence.Next();
                }

                dataValue.Value = !string.IsNullOrEmpty(ValueField) ? dataSet.CurrentRecord.CastValue<double>(ValueField) : 0.0;

                result.Add(dataValue);
            }

            return result;
        }

        public void AppendJsonDataArray(StringBuilder output, IEnumerable<DataValue> dataValues, string indent)
        {
            Func<string, string> escape = s => s.Replace("\"", "\\\\\"");

            foreach (DataValue dataValue in dataValues)
            {
                output.Append(indent + "{ " + string.Format("\"CategoryId\":\"{0}\", \"CategoryTitle\":\"{1}\", \"SeriesId\":\"{2}\", \"SeriesTitle\":\"{3}\", \"Value\":{4}", escape(dataValue.CategoryId), escape(dataValue.CategoryTitle), escape(dataValue.SeriesId), escape(dataValue.SeriesTitle), dataValue.Value.ToString().Replace(',', '.')) + " }");
                output.Append(",\r\n");
            }
        }

        public void AppendShowLabelsMethod(StringBuilder output, string indent)
        {
            if (!ShowValues)
                return;

            switch (Type)
            {
                case SeriesType.Bar:
                case SeriesType.StackBar:
                    AppendShowLabelsMethodOnBarChart(output, indent);
                    break;

                case SeriesType.Pie:
                    AppendShowLabelsMethodOnPieChart(output, indent);
                    break;

            }
        }

        public void AppendSeries(StringBuilder output, IEnumerable<DataValue> dataValues, string indent)
        {
            output.AppendLine(string.Format("{0}var series{1} = chart.addSeries(\"{2}\", {3});", indent, SeriesField, "SeriesTitle", MapSeriesTypeToDimpleType()));
        }

        #endregion

        #region Internal Methods

        internal string MapSeriesTypeToDimpleType()
        {
            switch (Type)
            {
                case SeriesType.Bar:
                case SeriesType.StackBar:
                    return "dimple.plot.bar";

                case SeriesType.Line:
                    return "dimple.plot.line";

                case SeriesType.Surface:
                    return "dimple.plot.area";

                case SeriesType.Pie:
                    return "dimple.plot.pie";

                default:
                    throw new InvalidOperationException(string.Format("Unknown series type {0}", Type));
            }
        }

        #endregion

        #region Private Methods

        private void AppendShowLabelsMethodOnPieChart(StringBuilder output, string indent)
        {
            output.AppendLine(string.Format("{0}series{1}.afterDraw = function(shape, data) {2}", indent, SeriesField, "{"));
            output.AppendLine(string.Format("{0}  var g = svg.select(\"g\");", indent));
            output.AppendLine(string.Format("{0}  var grect = g.node().getBBox();", indent));
            output.AppendLine(string.Format("{0}  var gmidx = grect.x + (grect.width - 7)/2; ", indent));
            output.AppendLine(string.Format("{0}  var gmidy = grect.y + (grect.height - 7)/2;", indent));
            output.AppendLine(string.Format("{0}  var radius = (grect.height - 7) / 2; ", indent));
            output.AppendLine(string.Format("{0}  var srect = d3.select(shape).node().getBBox();", indent));
            output.AppendLine(string.Format("{0}  var smidx = srect.x + srect.width/2; ", indent));
            output.AppendLine(string.Format("{0}  var smidy = srect.y + srect.height/2;", indent));
            output.AppendLine(string.Format("{0}  var dirx = smidx;", indent));
            output.AppendLine(string.Format("{0}  var diry = smidy;", indent));
            output.AppendLine(string.Format("{0}  var norm = Math.sqrt(dirx * dirx + diry * diry);", indent));
            output.AppendLine(string.Format("{0}  dirx /= norm;", indent));
            output.AppendLine(string.Format("{0}  diry /= norm;", indent));
            output.AppendLine(string.Format("{0}  var x = Math.round(gmidx + (radius + 25) * dirx);", indent));
            output.AppendLine(string.Format("{0}  var y = Math.round(gmidy + (radius + 15) * diry);", indent));
            output.AppendLine(string.Format("{0}  var xOnPie = Math.round(gmidx + (radius+4) * dirx);", indent));
            output.AppendLine(string.Format("{0}  var yOnPie = Math.round(gmidy + (radius+4) * diry);", indent));
            output.AppendLine(string.Format("{0}  var node = svg.append(\"text\")", indent));
            output.AppendLine(string.Format("{0}    .attr(\"x\", x + ((dirx > 0) ? 5 : -5))", indent));
            output.AppendLine(string.Format("{0}    .attr(\"y\", y + 3)", indent));
            output.AppendLine(string.Format("{0}    .style(\"font-size\", \"12px\")", indent));
            output.AppendLine(string.Format("{0}    .style(\"font-family\", \"sans-serif\")", indent));
            output.AppendLine(string.Format("{0}    .style(\"text-anchor\", (dirx > 0) ? \"start\" : \"end\")", indent));
            output.AppendLine(string.Format("{0}    .style(\"fill\", \"black\")", indent));
            output.AppendLine(string.Format("{0}    .text(data.aggField[0]);", indent));
            output.AppendLine(string.Format("{0}{1}", indent, "}"));
        }

        private void AppendShowLabelsMethodOnBarChart(StringBuilder output, string indent)
        {
            if (Chart.Orientation == RptChartOrientation.Horizontal)
            {
                output.AppendLine(string.Format("{0}series{1}.afterDraw = function (s, data) {2}", indent, SeriesField, "{"));
                output.AppendLine(string.Format("{0}  var shape = d3.select(s);", indent));
                output.AppendLine(string.Format("{0}  svg.append(\"text\")", indent));
                output.AppendLine(string.Format("{0}    .attr(\"x\", parseFloat(shape.attr(\"x\")) + shape.attr(\"width\") / 2)", indent));
                output.AppendLine(string.Format("{0}    .attr(\"y\", parseFloat(shape.attr(\"y\")) + (shape.attr(\"height\") > 30 ? (shape.attr(\"height\") / 2 + 8) : - 10))", indent));
                output.AppendLine(string.Format("{0}    .style(\"font-family\", \"courier new\")", indent));
                output.AppendLine(string.Format("{0}    .style(\"text-anchor\", \"middle\")", indent));
                output.AppendLine(string.Format("{0}    .style(\"font-size\", \"16px\")", indent));
                output.AppendLine(string.Format("{0}    .style(\"fill\", \"black\")", indent));
                output.AppendLine(string.Format("{0}    .style(\"pointer-events\", \"none\")", indent));
                output.AppendLine(string.Format("{0}    .text(measureAxis._getFormat()(data.yValue));", indent));
                output.AppendLine(string.Format("{0}{1}", indent, "}"));
            }
            else
            {
                // ToDo
            }
        }

        #endregion
    }
}