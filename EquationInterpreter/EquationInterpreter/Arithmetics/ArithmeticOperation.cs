using EquationInterpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OperatorFunc = System.Func<double, double, double>;

namespace EquationInterpreter.Arithmetics
{
    public class ArithmeticOperation : IEquationOperation<double>
    {
        private static Dictionary<string, OperatorFunc> operations = new Dictionary<string, OperatorFunc>()
        {
            {"+", Sum },
            {"-", Sub },
            {"*", Mul },
            {"/", Div },
        };

        public static bool IsValid(string operation)
        {
            return operations.ContainsKey(operation);
        }

        public int ArgumentsNumber => 2;
        public string Operator { get; set; }

        private OperatorFunc operatorFunc;

        public ArithmeticOperation(string strOperator)
        {
            if (!IsValid(strOperator))
            {
                throw new ArgumentException($"{strOperator} operator not recognized");
            }
            Operator = strOperator;
            operatorFunc = operations[strOperator];            
        }

        private static double Sum(double a, double b)
        {
            return a + b;
        }

        private static double Sub(double a, double b)
        {
            return a - b;
        }

        private static double Mul(double a, double b)
        {
            return a * b;
        }

        private static double Div(double a, double b)
        {
            return a / b;
        }

        public double Calculate(params double[] operands)
        {
            if(operands.Length != 2)
            {
                throw new ArgumentException($"Not enough arguments to operation <{Operator}>");
            }
            return operatorFunc(operands[0], operands[1]);
        }
    }
}
