using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationInterpreter.Equations
{
    public interface IEquationOperation<T> : IEquationPushable<T>
    {
        int ArgumentsNumber { get; }
        T Calculate(params T[] operands);
    }
}
