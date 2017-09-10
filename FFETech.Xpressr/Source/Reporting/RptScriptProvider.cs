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
    internal class RptScriptProvider
    {
        #region Nested Types

        private class Fields
        {
            #region Fields

            private IRptRecord record;

            #endregion

            #region Constructors

            public Fields(IRptRecord record)
            {
                this.record = record;
            }

            #endregion

            #region Indexers

            public object this[string name]
            {
                get
                {
                    return record.GetValue(name);
                }
            }

            #endregion

            #region Public Methods

            public bool IsNull(string name)
            {
                return XtConvert.ValueIsNull(this[name]);
            }

            public string AsString(string name)
            {
                return (RptFieldElement.FormatValue(this[name]));
            }

            // ToDo: linebreak provisorisch, da Expression nicht funktionierte
            public string AsString(string name, string linebreak)
            {
                string result = AsString(name);

                if (!string.IsNullOrEmpty(result))
                    result = result.Replace("\r", "").Replace("\n", linebreak);

                return result;
            }

            public bool Contains(string name)
            {
                object value;
                return record.TryGetValue(name, out value);
            }

            #endregion
        }

        #endregion

        #region Fields

        private string script;
        private Delegate lambda;

        #endregion

        #region Constructors

        public RptScriptProvider(string script)
        {
            this.script = script;
            lambda = System.Linq.Dynamic.DynamicExpression.ParseAndCompileLambda(typeof(Fields), "Fields => " + script);
        }

        #endregion

        #region Public Methods

        public object Execute(IRptDataSet dataSet)
        {
            return lambda.DynamicInvoke(new Fields(dataSet.CurrentRecord));
        }

        public override string ToString()
        {
            return script;
        }

        #endregion
    }
}