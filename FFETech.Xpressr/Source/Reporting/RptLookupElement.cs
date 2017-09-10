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
    public class RptLookupElement : RptElement
    {
        #region Constructors

        public RptLookupElement(RptRange parent)
            : base(parent)
        {
        }

        #endregion

        #region Properties

        public string Value
        {
            get
            {
                return Values.SingleOrDefault();
            }
            protected set
            {
                Values = new[] { value };
            }
        }

        public string[] Values
        {
            get;
            protected set;
        }

        public string DataSet
        {
            get;
            protected set;
        }

        public string SearchField
        {
            get
            {
                return SearchFields.SingleOrDefault();
            }
            protected set
            {
                Values = new[] { value };
            }
        }

        public string[] SearchFields
        {
            get;
            protected set;
        }

        public string TargetField
        {
            get
            {
                return TargetFields.SingleOrDefault();
            }
            protected set
            {
                Values = new[] { value };
            }
        }

        public string[] TargetFields
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

        #endregion

        #region Protected Methods

        protected override bool GetExpressionDefaultProperty(int index, out string propertyName)
        {
            switch (index)
            {
                case 1:
                    propertyName = "values";
                    return true;

                case 2:
                    propertyName = "dataset";
                    return true;

                case 3:
                    propertyName = "searchfields";
                    return true;

                case 4:
                    propertyName = "targetfields";
                    return true;
            }

            return base.GetExpressionDefaultProperty(index, out propertyName);
        }

        protected override void DoRender(IRptDataSet dataSet, StringBuilder output)
        {
            if (!string.IsNullOrEmpty(DataSet))
            {
                if (!dataSet.TryGetDataSet(DataSet, out dataSet))
                    throw new RptDataException($"Unknown Dataset '{DataSet}'");

                dataSet.ReadRecord();
            }

            while (dataSet.CurrentRecord != null)
            {
                bool found = true;

                if (SearchFields.Length != Values.Length)
                    throw new RptDataException("Count of search fields and values do not match");

                for (int i = 0; i < SearchFields.Length; i++)
                {
                    if (!XtConvert.CompareValues(dataSet.CurrentRecord.GetValue(SearchFields[i]), Values[i]))
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    List<string> targetValues = new List<string>();

                    foreach (string targetField in TargetFields)
                        targetValues.Add(RptFieldElement.FormatValue(dataSet.CurrentRecord.GetValue(targetField), null, Nullable));

                    if (!string.IsNullOrEmpty(Format))
                    {
                        while (targetValues.Count < 10)
                            targetValues.Add(string.Empty);

                        output.Append(string.Format(Format, targetValues.ToArray()));
                        return;
                    }

                    output.Append(string.Join(", ", targetValues));
                    return;
                }

                dataSet.ReadRecord();
            }
        }

        #endregion
    }
}