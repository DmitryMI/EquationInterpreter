using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationInterpreter.Equations
{
    class EquationElement<T>
    {
        private EquationElementType equationElementType;
        public EquationElementType ElementType => equationElementType;

        public IEquationPushable<T> Pushable { 
            get
            {
                switch (ElementType)
                {
                    case EquationElementType.Operation:
                        return Operator;
                    case EquationElementType.Variable:
                        return Variable;
                    default:
                        return null;
                }
            }
        }
        public IEquationOperation<T> Operator { get; set; }
        public T Immediate { get; set; }
        public IEquationVariable<T> Variable { get; set; }

        public EquationElement(IEquationVariable<T> variable)
        {
            equationElementType = EquationElementType.Variable;
            Variable = variable;
        }

        public EquationElement(T literal)
        {
            equationElementType = EquationElementType.Literal;
            Immediate = literal;
        }

        public EquationElement(IEquationOperation<T> equationOperator)
        {
            equationElementType = EquationElementType.Operation;
            Operator = equationOperator;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            switch (ElementType)
            {
                case EquationElementType.Operation:
                    stringBuilder.Append(Operator.ToString());
                    break;
                case EquationElementType.Literal:
                    stringBuilder.Append(Immediate.ToString());
                    break;
                case EquationElementType.Variable:
                    stringBuilder.Append(Variable.ToString());
                    break;
            }
            return stringBuilder.ToString();
        }
    }
}
