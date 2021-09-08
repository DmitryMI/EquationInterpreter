using Microsoft.VisualStudio.TestTools.UnitTesting;
using EquationInterpreter.Calculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EquationInterpreter.Equations;

namespace EquationInterpreter.Calculator.Tests
{
    [TestClass()]
    public class MathEquationParserTests
    {
        [TestMethod()]
        public void ParseReversePolishNotationTest()
        {
            double correct = 215.13;
            string equationString = "12,5 y + x * 0,3 +";
            double x = 4.95;
            double y = 30.9;

            Dictionary<string, ParsedVariable> variables = new Dictionary<string, ParsedVariable>();
            Equation<double> equation = MathEquationParser.ParseReversePolishNotation(equationString, variables);

            variables["x"].SetValue(x);
            variables["y"].SetValue(y);

            double result = equation.Calculate();
            Assert.IsTrue(Math.Abs(correct - result) < 0.01f, $"Value is {result}, but expected {correct}");
        }
    }
}