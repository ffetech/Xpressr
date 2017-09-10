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

namespace FFETech.Xpressr.Expressions
{
    public class OperationExpressionReader : IExpressionReader
    {
        #region Constructors

        public OperationExpressionReader(string name, int index)
        {
            Name = name;
            Index = index;
        }

        #endregion

        #region Properties

        public string Name
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public bool Read(IExpressionSource source, out IExpression expression)
        {
            expression = null;

            object bookmark = new object();
            source.SetBookmark(bookmark);

            try
            {
                ReadSpace(source);

                IExpression binaryOperator = null;
                IExpression unaryOperator = null;
                IExpression operand = null;

                if (Index > 0 && !CreateBinaryOperatorExpressionReader().Read(source, out binaryOperator))
                {
                    source.GotoBookmark(bookmark);
                    return false;
                }

                ReadSpace(source);

                CreateUnaryOperatorExpressionReader().Read(source, out unaryOperator);

                ReadSpace(source);

                if (source.GetChar() == '(')
                {
                    IExpressionList<IOperationExpression> children = CreateChildExpressionList();
                    IExpression child;

                    int i = 0;
                    while (CreateChildExpressionReader(i++).Read(source, out child))
                        children.Add((IOperationExpression)child);

                    if (!children.Any())
                        throw new InvalidExpressionException("Invalid operand", source, source.Index);

                    operand = children;

                    ReadSpace(source);

                    if (source.GetChar() != ')')
                        throw new InvalidExpressionException("Unexpected expression end", source, source.Index);
                }
                else
                {
                    if (!CreateOperandExpressionReader().Read(source, out operand))
                    {
                        if (Index == 0 && unaryOperator == null)
                        {
                            source.GotoBookmark(bookmark);
                            return false;
                        }

                        throw new InvalidExpressionException("Invalid operand", source, source.Index);
                    }
                }

                expression = CreateExpression(binaryOperator, unaryOperator, operand);
                return true;
            }
            finally
            {
                source.ClearBookmark(bookmark);
            }
        }

        #endregion

        #region Protected Methods

        protected virtual IOperationExpression CreateExpression(IExpression binaryOperator, IExpression unaryOperator, IExpression operand)
        {
            return new OperationExpression(Name, binaryOperator, unaryOperator, operand);
        }

        protected virtual IExpressionList<IOperationExpression> CreateChildExpressionList()
        {
            return new ExpressionList<IOperationExpression>("Children");
        }

        protected virtual IExpressionReader CreateSpaceExpressionReader()
        {
            return new SpaceExpressionReader("Space");
        }

        protected virtual IExpressionReader CreateBinaryOperatorExpressionReader()
        {
            return new OperatorExpressionReader("BinaryOperator", "&&", "||", "=", "!=", ">=", "<=");
        }

        protected virtual IExpressionReader CreateUnaryOperatorExpressionReader()
        {
            return new OperatorExpressionReader("UnaryOperator", "!");
        }

        protected virtual IExpressionReader CreateOperandExpressionReader()
        {
            return new OperandExpressionReader("Operand");
        }

        protected virtual IExpressionReader CreateChildExpressionReader(int index)
        {
            return new OperationExpressionReader($"{Name}-Child-{index}", index);
        }

        #endregion

        #region Private Methods

        private bool ReadSpace(IExpressionSource source, bool verifyOBound = true)
        {
            try
            {
                IExpression expression;
                return CreateSpaceExpressionReader().Read(source, out expression);
            }
            finally
            {
                if (verifyOBound && source.Index >= source.Length)
                    throw new InvalidExpressionException("Unexpected expression end", source, source.Index);
            }
        }

        #endregion
    }
}