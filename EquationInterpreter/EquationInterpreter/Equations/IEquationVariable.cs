using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationInterpreter.Equations
{
    public interface IEquationVariable<T>
    {
        string Name { get; }
        bool HasValue { get; }
        int Index { get; }
        T Value { get; }
    }
}
