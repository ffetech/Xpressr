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
using System.Reflection;

using FFETech.Xpressr.Expressions;

namespace FFETech.Xpressr.Parsing
{
    public abstract class PrsExpressionTarget : IPrsExpressionTarget
    {
        #region Explicit Members

        bool IPrsExpressionTarget.GetDefaultProperty(int index, out string propertyName)
        {
            return GetExpressionDefaultProperty(index, out propertyName);
        }

        void IPrsExpressionTarget.AssignProperty(string propertyName, IExpression expression)
        {
            AssignExpressionProperty(propertyName, expression);
        }

        IPrsExpressionTarget IPrsExpressionTarget.CreateChildTarget(string propertyName)
        {
            return CreateExpressionChildTarget(propertyName);
        }

        #endregion

        #region Protected Methods

        protected virtual bool GetExpressionDefaultProperty(int index, out string propertyName)
        {
            propertyName = string.Empty;
            return false;
        }

        protected virtual object ReadExpressionContent(string propertyName, IExpression expression)
        {
            ValueExpression valueExpression = expression as ValueExpression;
            CprDetailMarkupExpression markupExpression = expression as CprDetailMarkupExpression;
            ExpressionList<IExpression> expressionList = expression as ExpressionList<IExpression>;

            if (valueExpression != null)
                return valueExpression.Value;
            else if (markupExpression != null)
                return markupExpression.Target;
            else if (expressionList != null)
            {
                int i = 0;
                List<object> result = new List<object>();
                foreach (IExpression item in expressionList)
                    result.Add(ReadExpressionContent(string.Format("{0}[1]", propertyName, i++), item));
                return result;
            }
            else
                throw new Exception("Unexpected expression");
        }

        protected virtual object CastExpressionValue(string propertyName, Type propertyType, object value)
        {
            if (propertyType.IsArray)
            {
                IEnumerable<object> source = value as IEnumerable<object>;

                if (source == null)
                    source = new object[] { value };

                int i = 0;
                propertyType = propertyType.GetElementType();
                Array result = Array.CreateInstance(propertyType, source.Count());
                foreach (object item in source)
                    result.SetValue(CastExpressionValue(string.Format("{0}[{1}]", propertyName, i), propertyType, item), i++);

                return result;
            }
            else
            {
                return XtConvert.CastValue(propertyType, value);
            }
        }

        protected virtual void AssignExpressionProperty(string propertyName, IExpression expression)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(propertyName);

            if (propertyInfo == null)
                throw new Exception(string.Format("Property {0} not found.", propertyName));

            object targetValue = CastExpressionValue(propertyName, propertyInfo.PropertyType, ReadExpressionContent(propertyName, expression));
            propertyInfo.SetValue(this, targetValue, null);
        }

        protected virtual IPrsExpressionTarget CreateExpressionChildTarget(string propertyName)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(propertyName);

            if (propertyInfo == null)
                throw new Exception(string.Format("Property {0} not found.", propertyName));

            Type propertyType = propertyInfo.PropertyType;

            if (propertyType.IsArray)
                propertyType = propertyType.GetElementType();

            if (propertyType.IsAbstract)
                throw new Exception(string.Format("Abstract type of Property {0} cannot be instantiated.", propertyName));

            if (!typeof(IPrsExpressionTarget).IsAssignableFrom(propertyType))
                throw new Exception(string.Format("Type of Property {0} must implement interface {1}.", propertyName, typeof(IPrsExpressionTarget).Name));

            return (IPrsExpressionTarget)Activator.CreateInstance(propertyType, this);
        }

        #endregion

        #region Private Methods

        private PropertyInfo GetPropertyInfo(string propertyName)
        {
            foreach (PropertyInfo result in GetType().GetProperties())
                if (result.Name.ToLower() == propertyName.ToLower())
                    return result;

            throw new Exception(string.Format("Property {0} not found.", propertyName));
        }

        #endregion
    }
}