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

namespace FFETech.Xpressr.Reporting
{
    public interface IRptRecord
    {
        #region Methods

        bool TryGetValue(string fieldName, out object value);

        #endregion
    }

    public static class RptRecordExtensions
    {
        #region Public Static Methods

        public static object GetValue(this IRptRecord record, string fieldName, object fallback = null)
        {
            object value;

            if (!record.TryGetValue(fieldName, out value))
            {
                if (fallback != null)
                    return fallback;

                throw new RptDataException($"Unknown field '{fieldName}'");

            }

            return value;
        }

        public static T CastValue<T>(this IRptRecord record, string fieldName)
        {
            object value = record.GetValue(fieldName);

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                throw new RptDataException($"Field' {fieldName}' not of expected type {typeof(T).Name}");
            }
        }

        #endregion
    }
}