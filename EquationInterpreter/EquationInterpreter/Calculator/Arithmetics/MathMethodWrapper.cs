using EquationInterpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;



namespace EquationInterpreter.Calculator.Arithmetics
{
    public class MathMethodWrapper : IPriorityOperation<double>
    {
        private static List<MethodInfo> GetMathMethods(string name)
        {
            Type mathType = typeof(Math);
            MethodInfo[] allMethods = mathType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            List<MethodInfo> selectedMethods = new List<MethodInfo>();
            foreach (MethodInfo methodInfo in allMethods)
            {
                if (methodInfo.ReturnType != typeof(double))
                {
                    continue;
                }
                ParameterInfo[] parameters = methodInfo.GetParameters();

                bool goodParams = true;
                foreach (ParameterInfo parameterInfo in parameters)
                {
                    if (parameterInfo.ParameterType != typeof(double))
                    {
                        goodParams = false;
                        break;
                    }
                }
                if (!goodParams)
                {
                    continue;
                }
                if (methodInfo.Name != name)
                {
                    continue;
                }
                selectedMethods.Add(methodInfo);
            }
            return selectedMethods;
        }

        private static MethodInfo GetSpecificMethod(string strOperation)
        {
            List<MethodInfo> methods = GetMathMethods(strOperation);
            if (methods.Count == 1)
            {
                return methods[0];
            }

            if (char.IsDigit(strOperation[strOperation.Length - 1]))
            {
                int parametersNumber = int.Parse(strOperation[strOperation.Length - 1].ToString());
                string methodName = strOperation.Remove(strOperation.Length - 1);
                methods = GetMathMethods(methodName);

                MethodInfo specificMethod = methods.Find(m => m.GetParameters().Length == parametersNumber);
                return specificMethod;
            }
            return null;
        }

        public static bool IsValid(string strOperation)
        {
            var method = GetSpecificMethod(strOperation);
            return method != null;
        }

        private MethodInfo method;
        private int argumentsNumber;
        private string operation;
        public int ArgumentsNumber => argumentsNumber;

        public int Priority => 2;

        public MathMethodWrapper(string operation)
        {
            this.operation = operation;
            method = GetSpecificMethod(operation);
            if (method == null)
            {
                throw new ArgumentException($"Operation {operation} is not implemented in Math or parameters number does not match");
            }
            argumentsNumber = method.GetParameters().Length;
        }

        public double Calculate(params double[] operands)
        {
            if (operands.Length != ArgumentsNumber)
            {
                throw new ArgumentException($"Not enough arguments for operation {operation}");
            }
            object[] operandsObj = new object[ArgumentsNumber];
            Array.Copy(operands, operandsObj, operands.Length);
            object result = method.Invoke(null, operandsObj);
            return (double)result;
        }

        public override string ToString()
        {
            return operation;
        }
    }
}
