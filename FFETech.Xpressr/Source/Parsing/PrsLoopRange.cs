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

using FFETech.Xpressr.Expressions;

namespace FFETech.Xpressr.Parsing
{
    public class PrsLoopRange : PrsRange
    {
        #region Constructors

        public PrsLoopRange(PrsRange parent)
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

        #endregion

        #region Internal Methods

        internal override bool Parse(IExpressionSource source, IPrsOutput output, PrsElement next)
        {
            bool result = false;

            output.Debug("Loop begin");

            IPrsOutput childOutput = output.CreateChild();
            while (base.Parse(source, childOutput, next))
            {
                output.AddChild(Name, childOutput);
                childOutput = output.CreateChild();
                result = true;
            }

            output.Debug("Loop end");

            return result;
        }

        #endregion

        #region Protected Methods

        protected override bool GetExpressionDefaultProperty(int index, out string propertyName)
        {
            switch (index)
            {
                case 2:
                    propertyName = "name";
                    return true;
            }

            return base.GetExpressionDefaultProperty(index, out propertyName);
        }

        #endregion
    }
}