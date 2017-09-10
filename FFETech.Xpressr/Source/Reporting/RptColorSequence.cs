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

namespace FFETech.Xpressr.Reporting
{
    public class RptColorSequence
    {
        #region Fields

        Random random = new Random();
        int index = -1;
        List<int> colors = new List<int>();

        #endregion

        #region Constructors

        public RptColorSequence()
        {
            colors.Add(RptColors.Navy);
            colors.Add(RptColors.Maroon);
            colors.Add(RptColors.Teal);
            colors.Add(RptColors.Purple);
            colors.Add(RptColors.Green);
            colors.Add(RptColors.SaddleBrown);
            colors.Add(RptColors.Blue);
            colors.Add(RptColors.Olive);
            colors.Add(RptColors.Gray);
        }

        public RptColorSequence(int [] startColors)
        {
            colors.AddRange(startColors);
        }

        #endregion

        #region Properties

        public int CurrentColor
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public int Next()
        {
            index++;

            while (colors.Count < index + 1)
                colors.Add(RptColors.RGBToColor((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255)));

            CurrentColor = colors[index];
            return CurrentColor;
        }

        #endregion
    }
}