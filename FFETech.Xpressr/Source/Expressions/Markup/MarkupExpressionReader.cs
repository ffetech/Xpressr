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

namespace FFETech.Xpressr.Expressions.Markup
{
    public class MarkupExpressionReader : IExpressionReader
    {
        #region Fields

        private IMarkupExpression currentExpression;

        #endregion

        #region Constructors

        public MarkupExpressionReader(string name)
        {
            Name = name;
        }

        #endregion

        #region Properties

        public string Name
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public bool Read(IExpressionSource source, out IExpression expression)
        {
            expression = null;

            if (source.GetChar() != '{')
                return false;

            source.Read();

            currentExpression = CreateExpression();

            ReadProperties(source);
            ReadSpace(source);

            if (source.GetChar() != '}')
                throw new InvalidExpressionException("Unexpected expression end", source, source.Index - 1);

            source.Read();

            expression = currentExpression;
            currentExpression = null;

            return true;
        }

        public bool ReadList(IExpressionSource source, string name, out IExpressionList<IExpression> expression)
        {
            expression = null;

            if (source.GetChar() != '[')
                return false;

            source.Read();

            IExpressionList<IExpression> result = CreateExpressionList(name);

            while (source.Index < source.Length)
            {
                ReadSpace(source);

                long itemStartIndex = source.Index;
                char c = source.GetChar();

                IExpression item = null;

                switch (c)
                {
                    case '{': // Child-Expression
                        if (!CreateChildExpressionReader(name).Read(source, out item))
                            throw new InvalidExpressionException("Invalid child expression in array", source, source.Index);
                        break;

                    case '"': // String-Literal
                    case '\'':
                        if (!CreateStringLiteralExpressionReader(name, c).Read(source, out item))
                            throw new InvalidExpressionException("Invalid string literal", source, source.Index);
                        break;

                    case ',':
                        source.Read();
                        break;

                    case ']':
                        source.Read();
                        expression = result;
                        return true;

                    default: // Value
                        if (!CreateOperandExpressionReader(name).Read(source, out item))
                            throw new InvalidExpressionException("Invalid operand", source, source.Index);
                        break;
                }

                if (item != null)
                {
                    try
                    {
                        result.Add(item);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidExpressionException("Invalid list item", source, itemStartIndex, ex);
                    }
                }
            }

            return false;
        }

        #endregion

        #region Protected Methods

        protected virtual IMarkupExpression CreateExpression()
        {
            return new MarkupExpression(Name);
        }

        protected virtual IExpressionList<IExpression> CreateExpressionList(string name)
        {
            return currentExpression != null ? currentExpression.CreateExpressionList(name) : new ExpressionList<IExpression>(name);
        }

        protected virtual IExpressionReader CreateSpaceExpressionReader()
        {
            return new SpaceExpressionReader("Space");
        }

        protected virtual IExpressionReader CreateOperandExpressionReader(string name)
        {
            return new OperandExpressionReader(name);
        }

        protected virtual IExpressionReader CreateStringLiteralExpressionReader(string name, char embrace)
        {
            return new EmbraceExpressionReader(name, embrace.ToString(), embrace.ToString(), '\\');
        }

        protected virtual IExpressionReader CreateChildExpressionReader(string name)
        {
            return new MarkupExpressionReader(name);
        }

        #endregion

        #region Private Methods

        private void VerifyOBound(IExpressionSource source)
        {
            if (source.Index >= source.Length)
                throw new InvalidExpressionException("Unexpected expression end", source, source.Index);
        }

        private bool ReadSpace(IExpressionSource source, bool verifyOBound = true)
        {
            try
            {
                IExpression expression;
                return CreateSpaceExpressionReader().Read(source, out expression);
            }
            finally
            {
                if (verifyOBound)
                    VerifyOBound(source);
            }
        }

        private bool ReadPropertyOperand(IExpressionSource source, string name, out IExpression result)
        {
            result = null;

            VerifyOBound(source);

            char c = source.GetChar();

            switch (c)
            {
                case '{': // Child Expression
                    if (!CreateChildExpressionReader(name).Read(source, out result))
                        throw new InvalidExpressionException("Invalid child expression", source, source.Index);
                    return true;

                case '[': // Array Expression
                    IExpressionList<IExpression> resultList;
                    if (!ReadList(source, name, out resultList))
                        throw new InvalidExpressionException("Invalid array expression", source, source.Index);
                    VerifyOBound(source);
                    result = resultList;
                    return true;

                case '"': // String-Literal
                case '\'':
                    if (!CreateStringLiteralExpressionReader(name, c).Read(source, out result))
                        throw new InvalidExpressionException("Invalid string literal", source, source.Index);
                    return true;

                default: // Value
                    return CreateOperandExpressionReader(name).Read(source, out result);
            }
        }

        private void ReadProperties(IExpressionSource source)
        {
            long startIndex = source.Index;

            bool allowDefaultProperty = true;
            int propertyIndex = 0;

            while (source.Index < source.Length)
            {
                string propertyName = null;
                IExpression propertyExpression;

                //Standardeigenschaften
                allowDefaultProperty = allowDefaultProperty && currentExpression.GetDefaultProperty(propertyIndex, out propertyName);

                //Linker Operand
                IExpression leftOperand;
                ReadSpace(source);
                if (!ReadPropertyOperand(source, propertyName, out leftOperand))
                    throw new InvalidExpressionException("Left operand missing", source, startIndex);

                bool defaultSpace = ReadSpace(source);

                if (source.GetChar() == '=')
                {
                    allowDefaultProperty = false;

                    // Name der Eigenschaft aus Linken Operand
                    if (!(leftOperand is IValueExpression))
                        throw new InvalidExpressionException("Left operand is of too complicated type", source, startIndex);

                    propertyName = ((IValueExpression)leftOperand).Value;

                    // Rechter Operand
                    source.Read();
                    IExpression rightOperand;
                    ReadSpace(source);
                    if (!ReadPropertyOperand(source, propertyName, out rightOperand))
                        throw new InvalidExpressionException("Right operand missing", source, startIndex);

                    propertyExpression = rightOperand;
                }
                else
                {
                    if (!allowDefaultProperty)
                        throw new InvalidExpressionException("Wrong ordered default properties", source, startIndex);

                    if (string.IsNullOrEmpty(propertyName))
                        throw new InvalidExpressionException(string.Format("Default property {0} not available", propertyIndex + 1), source, startIndex);

                    propertyExpression = leftOperand;
                }

                if (string.IsNullOrEmpty(propertyName))
                    throw new InvalidExpressionException("Property name missing", source, startIndex);

                try
                {
                    currentExpression.Properties.Add(propertyName, propertyExpression);
                }
                catch (Exception ex)
                {
                    throw new InvalidExpressionException("Invalid property", source, startIndex, ex);
                }

                ReadSpace(source);

                char c = source.GetChar();

                if (c == '}')
                    return;

                if (c != ',' && !(defaultSpace && allowDefaultProperty))
                    return;

                if (c == ',')
                    source.Read();

                propertyIndex++;
            }
        }

        #endregion
    }
}