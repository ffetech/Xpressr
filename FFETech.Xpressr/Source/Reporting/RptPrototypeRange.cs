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

using System.Collections.Generic;
using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public class RptPrototypeRange : RptRange
    {
        #region Fields

        private Dictionary<string, RptDocument> prototypes = new Dictionary<string, RptDocument>();

        #endregion

        #region Constructors

        public RptPrototypeRange(RptRange parent)
            : base(parent)
        {
        }

        #endregion

        #region Protected Methods

        protected override void DoRender(IRptDataSet dataSet, StringBuilder output)
        {
            StringBuilder templateOutput = new StringBuilder();
            RenderItems(dataSet, templateOutput, Children);

            string template = templateOutput.ToString();

            RptDocument prototype;

            if (!prototypes.TryGetValue(template, out prototype))
            {
                prototype = new RptDocument();
                prototypes.Add(template, prototype);
                prototype.ReadFromTemplate(template);
            }

            prototype.Render(dataSet, output);
        }

        #endregion
    }
}