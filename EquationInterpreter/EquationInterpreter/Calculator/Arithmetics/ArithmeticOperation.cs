using EquationInterpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OperatorFunc = System.Func<double, double, double>;

namespace EquationInterpreter.Calculator.Arithmetics
{
    public class ArithmeticOperation : IPriorityOperation<double>
    {
        private struct OperationDescriptor
        {
            public OperatorFunc Function { get; set; }
            public int Priority { get; set; }
            public OperationDescriptor(OperatorFunc func, int priority)
            {
                Function = func;
                Priority = priority;
            }
        }

        private static Dictionary<string, OperationDescriptor> operations = new Dictionary<string, OperationDescriptor>()
        {
            {"+", new OperationDescriptor(Sum, 0) },
            {"-", new OperationDescriptor(Sub, 0) },
            {"*", new OperationDescriptor(Mul, 1) },
            {"/", new OperationDescriptor(Div, 1) },
        };

        public static bool IsValid(string operation)
        {
            return operations.ContainsKey(operation);
        }

        public int ArgumentsNumber => 2;
        public string Operator { get; set; }

        public int Priority => operationDescriptor.Priority;

        private OperationDescriptor operationDescriptor;

        public ArithmeticOperation(string strOperator)
        {
            if (!IsValid(strOperator))
            {
                throw new ArgumentException($"{strOperator} operator not recognized");
            }
            Operator = strOperator;
            operationDescriptor = operations[strOperator];
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
            if (operands.Length != 2)
            {
                throw new ArgumentException($"Not enough arguments to operation <{Operator}>");
            }
            return operationDescriptor.Function(operands[0], operands[1]);
        }

        public override string ToString()
        {
            return Operator;
        }
    }
}
