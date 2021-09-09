using EquationInterpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationInterpreter.Calculator
{
    public interface IPriorityOperation<T> : IEquationOperation<T>
    {
        /// <summary>
        /// Priority of operation. Higher values mean higher priority (e.g. + can have Priority=1 and * can have Priority=2)
        /// </summary>
        int Priority { get; }
    }
}
