using Microsoft.VisualStudio.TestTools.UnitTesting;
using EquationInterpreter.Calculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EquationInterpreter.Equations;
using System.Diagnostics;

namespace EquationInterpreter.Calculator.Tests
{
    [TestClass()]
    public class MathEquationParserTests
    {
        [TestMethod()]
        public void ParseReversePolishNotationTest()
        {
            double correct = 0.633;
            string equationString = "12,5 y + x * y + Sin 100 Min2";
            double x = 4.95;
            double y = 30.9;

            Dictionary<string, ParsedVariable> variables = new Dictionary<string, ParsedVariable>();
            Equation<double> equation = MathEquationParser.ParseReversePolishNotation(equationString, variables);

            variables["x"].SetValue(x);
            variables["y"].SetValue(y);

            double result = equation.Calculate();
            Assert.IsTrue(Math.Abs(correct - result) < 0.01f, $"Value is {result}, but expected {correct}");
        }

        [TestMethod()]
        public void ParseTest()
        {
            double expected = (5 + 6) * ((-1 + 2) * Math.Sin(Math.Min(2 - 3, 10)));
            string equationString = "(5 + 6) * ((-1 + 2) * Sin(Min2(2 - 3, 10)))";            
            Dictionary<string, ParsedVariable> variables = new Dictionary<string, ParsedVariable>();
            Equation<double> equation = MathEquationParser.Parse(equationString, variables);
            Debug.WriteLine(equation);
            double result = equation.Calculate();
            double delta = Math.Abs(expected - result);
            Assert.IsTrue(delta < 0.01f, $"Expected resul is {expected}, but actual result is {result}");
        }
    }
}