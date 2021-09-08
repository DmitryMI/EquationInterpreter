using Microsoft.VisualStudio.TestTools.UnitTesting;
using EquationInterpreter.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EquationInterpreter.Arithmetics;

namespace EquationInterpreter.Equations.Tests
{
    [TestClass()]
    public class EquationTests
    {
        [TestMethod()]
        public void CalculateTest()
        {
            // (1 + 2) * x + y = 15
            // 1 2 + 4 * 3 +

            ArithmeticVariable xVariable = new ArithmeticVariable("x", 0);
            ArithmeticVariable yVariable = new ArithmeticVariable("y", 1);
            Equation<double> equation = new Equation<double>(new [] { xVariable, yVariable });            
            equation.Push(1);
            equation.Push(2);
            equation.Push(new ArithmeticOperation("+"));
            equation.Push(xVariable);
            equation.Push(new ArithmeticOperation("*"));
            equation.Push(yVariable);
            equation.Push(new ArithmeticOperation("+"));

            double resultWithParams = equation.Calculate(4, 3);
            Assert.AreEqual(15, resultWithParams);

            xVariable.SetValue(4);
            yVariable.SetValue(3);
            double resultWithValuedVariables = equation.Calculate();

            Assert.AreEqual(15, resultWithValuedVariables);            
        }
    }
}