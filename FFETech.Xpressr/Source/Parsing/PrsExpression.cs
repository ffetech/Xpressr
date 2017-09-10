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

using FFETech.Xpressr.Expressions;
using FFETech.Xpressr.Expressions.Markup;

namespace FFETech.Xpressr.Parsing
{
    internal interface IPrsExpressionVisitor
    {
        #region Methods

        IPrsExpressionTarget CreateElementEndTarget(Type elementType);

        IPrsExpressionTarget CreateElementTarget(Type elementType);

        Type GetElementType(string name);

        #endregion
    }

    internal class CprMarkupExpression : MarkupExpression
    {
        #region Nested Types

        private class PropertiesExpression : ExpressionDictionary<IExpression>
        {
            #region Fields

            private CprMarkupExpression expression;

            #endregion

            #region Constructors

            public PropertiesExpression(string name, CprMarkupExpression expression)
                : base(name)
            {
                this.expression = expression;
            }

            #endregion

            #region Public Methods

            public override void Add(string key, IExpression value)
            {
                base.Add(key, value);
                expression.AssignProperty(key, value);
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly IPrsExpressionVisitor visitor;

        private Type elementType;

        #endregion

        #region Constructors

        public CprMarkupExpression(string name, IPrsExpressionVisitor visitor)
            : base(name, null)
        {
            this.visitor = visitor;
            Properties = new PropertiesExpression("Properties", this);
        }

        #endregion

        #region Properties

        public IPrsExpressionTarget Target
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override bool GetDefaultProperty(int index, out string name)
        {
            if (index == 0)
            {
                name = "element";
                return true;
            }
            else if (index == 1 && elementType != null && typeof(PrsRange).IsAssignableFrom(elementType))
            {
                name = "step";
                return true;
            }

            if (Target == null)
                throw new Exception("Target element not established");

            return Target.GetDefaultProperty(index, out name);
        }

        public void AssignProperty(string name, IExpression expression)
        {
            IValueExpression valueExpression = expression as IValueExpression;

            switch (name.ToLower())
            {
                case "element":
                    if (valueExpression == null)
                        throw new Exception("Operand is of too complicated type");

                    elementType = visitor.GetElementType(valueExpression.Value);

                    if (!typeof(PrsRange).IsAssignableFrom(elementType))
                        Target = visitor.CreateElementTarget(elementType);

                    return;

                case "step":
                    if (valueExpression == null)
                        throw new Exception("Operand is of too complicated type");

                    if (Target != null)
                        throw new Exception("Non range target with step property");

                    switch (valueExpression.Value.ToLower())
                    {
                        case "begin":
                            Target = visitor.CreateElementTarget(elementType);
                            return;

                        case "end":
                            Target = visitor.CreateElementEndTarget(elementType);
                            return;

                        default:
                            throw new Exception("Invalid value for property step. Value 'begin' or 'end' expected.");
                    }

                default:
                    if (Target == null)
                        throw new Exception("Target element not established");

                    Target.AssignProperty(name, expression);
                    break;
            }
        }

        #endregion
    }

    internal class CprDetailMarkupExpression : MarkupExpression
    {
        #region Nested Types

        private class PropertiesExpression : ExpressionDictionary<IExpression>
        {
            #region Fields

            private CprDetailMarkupExpression expression;

            #endregion

            #region Constructors

            public PropertiesExpression(string name, CprDetailMarkupExpression expression)
                : base(name)
            {
                this.expression = expression;
            }

            #endregion

            #region Public Methods

            public override void Add(string key, IExpression value)
            {
                base.Add(key, value);
                expression.AssignProperty(key, value);
            }

            #endregion
        }

        #endregion

        #region Constructors

        public CprDetailMarkupExpression(string name, IPrsExpressionTarget target)
            : base(name, null)
        {
            Target = target;
            Properties = new PropertiesExpression("Properties", this);
        }

        #endregion

        #region Properties

        public IPrsExpressionTarget Target
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override bool GetDefaultProperty(int index, out string name)
        {
            return Target.GetDefaultProperty(index, out name);
        }

        public void AssignProperty(string name, IExpression expression)
        {
            Target.AssignProperty(name, expression);
        }

        #endregion
    }

    internal class CprMarkupExpressionReader : MarkupExpressionReader
    {
        #region Nested Types

        private class DetailReader : MarkupExpressionReader
        {
            #region Fields

            private readonly IPrsExpressionTarget target;

            private CprDetailMarkupExpression currentExpression;

            #endregion

            #region Constructors

            public DetailReader(string name, IPrsExpressionTarget target)
                : base(name)
            {
                this.target = target;
            }

            #endregion

            #region Protected Methods

            protected override IMarkupExpression CreateExpression()
            {
                currentExpression = new CprDetailMarkupExpression(Name, target);
                return currentExpression;
            }

            protected override IExpressionReader CreateChildExpressionReader(string name)
            {
                if (currentExpression == null)
                    throw new Exception("Current expression not established");

                if (currentExpression.Target == null)
                    throw new Exception("Target element not established");

                return new DetailReader(name, currentExpression.Target.CreateChildTarget(name));
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly IPrsExpressionVisitor visitor;

        private CprMarkupExpression currentExpression;

        #endregion

        #region Constructors

        public CprMarkupExpressionReader(string name, IPrsExpressionVisitor visitor)
            : base(name)
        {
            this.visitor = visitor;
        }

        #endregion

        #region Protected Methods

        protected override IMarkupExpression CreateExpression()
        {
            currentExpression = new CprMarkupExpression(Name, visitor);
            return currentExpression;
        }

        protected override IExpressionReader CreateChildExpressionReader(string name)
        {
            if (currentExpression == null)
                throw new Exception("Current expression not established");

            if (currentExpression.Target == null)
                throw new Exception("Target element not established");

            return new DetailReader(name, currentExpression.Target.CreateChildTarget(name));
        }

        #endregion
    }
}