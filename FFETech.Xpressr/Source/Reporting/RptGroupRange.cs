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

using System.Linq;
using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public class RptGroupRange : RptRange
    {
        #region Fields

        private object[] currentFieldValues;

        #endregion

        #region Constructors

        public RptGroupRange(RptRange parent)
            : base(parent)
        {
        }

        #endregion

        #region Properties

        public string[] Fields
        {
            get;
            protected set;
        }

        #endregion

        #region Public Methods

        public override bool Join(IRptDataSet dataSet)
        {
            object[] newFieldValues = Fields.Select(field => dataSet.CurrentRecord.GetValue(field)).ToArray();

            if (!Enumerable.SequenceEqual(currentFieldValues, newFieldValues))
                return false;

            return base.Join(dataSet);
        }

        #endregion

        #region Protected Methods

        protected override void DoRender(IRptDataSet dataSet, StringBuilder output)
        {
            ResetLimit();

            while (dataSet.CurrentRecord != null && base.Join(dataSet))
            {
                currentFieldValues = Fields.Select(field => dataSet.CurrentRecord.GetValue(field)).ToArray();
                base.DoRender(dataSet, output);
            }
        }

        #endregion
    }
}