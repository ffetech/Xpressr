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
using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public class RptFieldElement : RptElement
    {
        #region Constructors

        public RptFieldElement(RptRange parent)
            : base(parent)
        {
        }

        #endregion

        #region Properties

        public string Name
        {
            get;
            protected set;
        }

        public string Format
        {
            get;
            protected set;
        }

        public bool Nullable
        {
            get;
            protected set;
        }

        public string Fallback
        {
            get;
            protected set;
        }

        #endregion

        #region Public Static Methods

        public static string FormatValue(object value, string format = null, bool nullable = false)
        {
            if (value != null && nullable)
                value = XtConvert.CastValue(value.GetType(), value, nullable);

            if (value == null)
                value = string.Empty;

            if (value is DateTime)
            {
                DateTime dvalue = (DateTime)value;

                if (dvalue == DateTime.MinValue || dvalue < new DateTime(1900, 1, 1))
                    value = string.Empty;
                else if (dvalue.TimeOfDay.TotalSeconds == 0)
                    value = dvalue.ToString("dd/MM/yyyy");
                else
                    value = dvalue.ToString("dd/MM/yyyy HH:mm");
            }

            if (!string.IsNullOrEmpty(format))
                value = string.Format(format, value);

            return value.ToString();
        }

        #endregion

        #region Protected Methods

        protected override bool GetExpressionDefaultProperty(int index, out string propertyName)
        {
            switch (index)
            {
                case 1:
                    propertyName = "name";
                    return true;
            }

            return base.GetExpressionDefaultProperty(index, out propertyName);
        }

        protected override void DoRender(IRptDataSet dataSet, StringBuilder output)
        {
            output.Append(FormatValue(dataSet.CurrentRecord.GetValue(Name, Fallback), Format, Nullable));
        }

        #endregion
    }
}