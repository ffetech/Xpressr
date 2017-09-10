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
    public class RptAverageAggregateElement : RptAggregateElement
    {
        #region Fields

        private double sum;
        private int count;
        private RptDataException exception;

        #endregion

        #region Constructors

        public RptAverageAggregateElement(RptRange parent)
            : base(parent)
        {
        }

        #endregion

        #region Properties

        public string Field
        {
            get;
            protected set;
        }

        public string Format
        {
            get;
            protected set;
        }

        #endregion

        #region Public Methods

        public override void Reset()
        {
            sum = 0;
            count = 0;
            exception = null;
        }

        public override void Join(IRptDataSet dataSet)
        {
            if (exception != null)
                return;

            try
            {
                sum += dataSet.CurrentRecord.CastValue<double>(Field);
                count++;
            }
            catch (RptDataException ex)
            {
                exception = ex;
            }
        }

        #endregion

        #region Protected Methods

        protected override bool GetExpressionDefaultProperty(int index, out string propertyName)
        {
            switch (index)
            {
                case 1:
                    propertyName = "field";
                    return true;
            }

            return base.GetExpressionDefaultProperty(index, out propertyName);
        }

        protected override void DoRender(IRptDataSet dataSet, StringBuilder output)
        {
            if (exception != null)
                throw new RptDataException(exception.Message);

            output.Append((count > 0 ? (sum / count) : 0).ToString(Format));
        }

        #endregion
    }
}