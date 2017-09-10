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
using System.Text;

using FFETech.Xpressr.Expressions;

namespace FFETech.Xpressr.Reporting
{
    public abstract class RptExpressionTarget : IRptExpressionTarget
    {
        #region Fields

        private readonly Dictionary<string, object> bindings = new Dictionary<string, object>();

        #endregion

        #region Explicit Members

        bool IRptExpressionTarget.GetDefaultProperty(int index, out string propertyName)
        {
            return GetExpressionDefaultProperty(index, out propertyName);
        }

        void IRptExpressionTarget.AssignProperty(string propertyName, IExpression expression)
        {
            AssignExpressionProperty(propertyName, expression);
        }

        IRptExpressionTarget IRptExpressionTarget.CreateChildTarget(string propertyName)
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
            RptDetailMarkupExpression markupExpression = expression as RptDetailMarkupExpression;
            RptMarkupExpression bindingExpression = expression as RptMarkupExpression;
            ExpressionList<IExpression> expressionList = expression as ExpressionList<IExpression>;

            if (valueExpression != null)
                return valueExpression.Value;

            else if (markupExpression != null)
                return markupExpression.Target;

            else if (bindingExpression != null)
                return bindingExpression.Target;

            else if (expressionList != null)
            {
                int i = 0;
                List<object> result = new List<object>();
                foreach (IExpression item in expressionList)
                    result.Add(ReadExpressionContent(string.Format("{0}[1]", propertyName, i++), item));
                return result;
            }
            else
                throw new RptTemplateException("Unexpected expression");
        }

        protected virtual object CastExpressionValue(Type propertyType, object value, out object binding)
        {
            if (propertyType.IsArray)
            {
                object[] source = (value as IEnumerable<object>)?.ToArray();

                if (source == null)
                    source = new object[] { value };

                propertyType = propertyType.GetElementType();

                Array result = Array.CreateInstance(propertyType, source.Count());

                Array bindingList = null;
                object bindingItem;

                for (int i = 0; i < source.Length; i++)
                {
                    result.SetValue(CastExpressionValue(propertyType, source[i], out bindingItem), i);

                    if (bindingItem != null)
                    {
                        if (bindingList == null)
                            bindingList = new object[source.Count()];

                        bindingList.SetValue(bindingItem, i);
                    }
                }

                binding = bindingList;
                return result;
            }
            else
            {
                if (value is RptElement)
                {
                    binding = value;
                    return XtConvert.GetDefaultValue(propertyType);
                }

                binding = null;
                return XtConvert.CastValue(propertyType, value);
            }
        }

        protected virtual void AssignExpressionProperty(string propertyName, IExpression expression)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(propertyName);

            if (propertyInfo == null)
                throw new RptTemplateException(string.Format("Property {0} not found.", propertyName));

            object targetValue = ReadExpressionContent(propertyName, expression);

            object targetBinding;
            targetValue = CastExpressionValue(propertyInfo.PropertyType, targetValue, out targetBinding);

            bindings[propertyName] = targetBinding;
            propertyInfo.SetValue(this, targetValue, null);
        }

        protected virtual IRptExpressionTarget CreateExpressionChildTarget(string propertyName)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(propertyName);

            if (propertyInfo == null)
                throw new RptTemplateException(string.Format("Property {0} not found.", propertyName));

            Type propertyType = propertyInfo.PropertyType;

            if (propertyType.IsArray)
                propertyType = propertyType.GetElementType();

            if (typeof(IRptExpressionTarget).IsAssignableFrom(propertyType))
            {
                if (propertyType.IsAbstract)
                    throw new RptTemplateException(string.Format("Abstract type of Property {0} cannot be instantiated.", propertyName));

                return (IRptExpressionTarget)Activator.CreateInstance(propertyType, this);
            }

            return null;
        }

        protected void ExecuteBindings(IRptDataSet dataSet)
        {
            foreach (string name in bindings.Keys)
            {
                object binding = bindings[name];

                if (binding == null)
                    continue;

                PropertyInfo propertyInfo = GetPropertyInfo(name);
                Type propertyType = propertyInfo.PropertyType;

                if (binding.GetType().IsArray)
                {
                    Array target = (Array)propertyInfo.GetValue(this, null);
                    propertyType = propertyType.GetElementType();

                    object[] bindingList = binding as object[];

                    for (int i = 0; i < bindingList.Length; i++)
                    {
                        if (bindingList[i] == null)
                            continue;

                        RptElement element = (RptElement)bindingList[i];

                        StringBuilder output = new StringBuilder();
                        element.Render(dataSet, output);

                        target.SetValue(XtConvert.CastValue(propertyType, output.ToString()), i);
                    }
                }
                else
                {
                    RptElement element = (RptElement)binding;

                    StringBuilder output = new StringBuilder();
                    element.Render(dataSet, output);

                    propertyInfo.SetValue(this, XtConvert.CastValue(propertyType, output.ToString()), null);
                }
            }
        }

        #endregion

        #region Private Methods

        private PropertyInfo GetPropertyInfo(string propertyName)
        {
            foreach (PropertyInfo result in GetType().GetProperties())
                if (result.Name.ToLower() == propertyName.ToLower())
                    return result;

            throw new RptTemplateException(string.Format("Property {0} not found.", propertyName));
        }

        #endregion
    }
}