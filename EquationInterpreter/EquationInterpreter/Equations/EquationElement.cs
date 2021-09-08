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
        
    }
}
