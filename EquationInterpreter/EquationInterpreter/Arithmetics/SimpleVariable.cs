using EquationInterpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationInterpreter.Arithmetics
{
    public class ArithmeticVariable : IEquationVariable<double>
    {
        public string Name { get; }
        public bool HasValue { get; private set; }
        public int Index { get; }
        public double Value { get; private set; }

        public ArithmeticVariable(string name, int index)
        {
            Name = name;
            Index = index;
            HasValue = false;
        }

        public void SetValue(double value)
        {
            Value = value;
            HasValue = true;
        }

        public void ClearValue()
        {
            HasValue = false;
        }

        public override string ToString()
        {
            return "(<" + Name + ">: " + (HasValue ? Value.ToString() : "N/A") + ")";
        }
    }
}
