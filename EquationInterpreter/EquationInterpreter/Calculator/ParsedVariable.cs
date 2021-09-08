using EquationInterpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationInterpreter.Calculator
{
    public class ParsedVariable : IEquationVariable<double>
    {
        public string Name { get; }

        public bool HasValue { get; private set; }

        public int Index { get; private set; }

        public double Value { get; private set; }

        public bool IsReadOnly => false;

        public ParsedVariable(string name, int index)
        {
            Name = name;
            Index = index;
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
