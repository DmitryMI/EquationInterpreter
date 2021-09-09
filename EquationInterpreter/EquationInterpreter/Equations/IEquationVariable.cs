using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationInterpreter.Equations
{
    public interface IEquationVariable<T> : IEquationPushable<T>
    {
        string Name { get; }
        bool HasValue { get; }
        int Index { get; }
        T Value { get; }

        bool IsReadOnly { get; }
        void SetValue(T value);
        void ClearValue();
    }
}
