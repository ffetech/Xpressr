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
    public class RptChartCategoryAxis : RptChartAxis
    {
        #region Constructors

        public RptChartCategoryAxis(RptChartElement chart)
            : base(chart)
        {
            Size = 100;
        }

        #endregion

        #region Public Methods

        public void Append(StringBuilder output, string axis, string indent)
        {
            output.AppendLine(string.Format("{0}var categoryAxis = chart.addCategoryAxis(\"{1}\", [\"{2}\"]);", indent, axis, Chart.SeriesDef.Length > 1 ? "CategoryTitle\", \"SeriesTitle" : "CategoryTitle"));

            AppendFormat(output, "categoryAxis", indent);

            output.AppendLine(string.Format("{0}categoryAxis.addOrderRule(function(left, right) {1}", indent, "{"));
            output.AppendLine(string.Format("{0}  if (left.CategoryTitle === right.CategoryTitle) return {1}", indent, 0));
            output.AppendLine(string.Format("{0}  if (left.CategoryTitle > right.CategoryTitle) return {1}", indent, Chart.Orientation == RptChartOrientation.Vertical ? -1 : 1));
            output.AppendLine(string.Format("{0}  return {1}", indent, Chart.Orientation == RptChartOrientation.Vertical ? 1 : -1));
            output.AppendLine(string.Format("{0}{1});", indent, "}"));
        }

        #endregion
    }
}