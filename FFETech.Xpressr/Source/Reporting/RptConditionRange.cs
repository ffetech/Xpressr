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
using System.Linq;
using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public class RptConditionRange : RptRange
    {
        #region Fields

        private RptScriptProvider condition;

        #endregion

        #region Constructors

        public RptConditionRange(RptRange parent)
            : base(parent)
        {
        }

        #endregion

        #region Properties

        public string Condition
        {
            get
            {
                return condition != null ? condition.ToString() : string.Empty;
            }
            protected set
            {
                condition = new RptScriptProvider(value);
            }
        }

        #endregion

        #region Internal Methods

        internal override Type GetElementType(string name)
        {
            switch (name.ToLower())
            {
                case "if":
                    return typeof(RptConditionIfElement);

                case "else":
                    return typeof(RptConditionElseElement);

                default:
                    return Parent.GetElementType(name);
            }
        }

        #endregion

        #region Protected Methods

        protected override bool GetExpressionDefaultProperty(int index, out string propertyName)
        {
            switch (index)
            {
                case 2:
                    propertyName = "condition";
                    return true;
            }

            return base.GetExpressionDefaultProperty(index, out propertyName);
        }

        protected override void DoRender(IRptDataSet dataSet, StringBuilder output)
        {
            if (condition != null)
            {
                object result = condition.Execute(dataSet);

                // If
                if (Children.Any(child => child is RptConditionIfElement))
                {
                    RptConditionIfElement ifElement = Children.OfType<RptConditionIfElement>().FirstOrDefault(child => (child.Value ?? string.Empty) == (result ?? string.Empty).ToString());
                    if (ifElement != null)
                    {
                        RenderItems(dataSet, output, Children.SkipWhile(child => child != ifElement).TakeWhile(child => child == ifElement || (!(child is RptConditionIfElement) && !(child is RptConditionElseElement))));
                        return;
                    }
                }
                else
                {
                    if (result is bool && ((bool)result))
                    {
                        RenderItems(dataSet, output, Children.TakeWhile(child => !(child is RptConditionElseElement)));
                        return;
                    }
                }

                // Else
                RenderItems(dataSet, output, Children.SkipWhile(child => !(child is RptConditionElseElement)));
            }
        }

        #endregion
    }
}