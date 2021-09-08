using EquationInterpreter.Arithmetics;
using EquationInterpreter.Equations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationInterpreter.Calculator
{
    public class MathEquationParser
    {

        private enum SequenceType
        {
            None, 
            Literal,
            Identifier
        }

        private static string IdentifyCharacter(string str, int position)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for(int i = 0; i < str.Length; i++)
            {
                if(i != position)
                {
                    stringBuilder.Append(str[i]);
                }
                else
                {
                    stringBuilder.Append(">>");
                    stringBuilder.Append(str[i]);
                    stringBuilder.Append("<<");
                }
            }
            return stringBuilder.ToString();
        }

        public static Equation<double> ParseReversePolishNotation(string rvpEquation, Dictionary<string, ParsedVariable> variablesOut)
        {
            Equation<double> equation = new Equation<double>();

            char[] additionalChars = new char[] { '*' };
            
            SequenceType sequenceType = SequenceType.None;
            StringBuilder sequence = new StringBuilder();
            for (int i = 0; i < rvpEquation.Length; i++)
            {
                bool isLetter = Char.IsLetter(rvpEquation[i]);
                bool isSpecialSymbol = Char.IsSymbol(rvpEquation[i]) || additionalChars.Contains(rvpEquation[i]);
                bool isDigit = Char.IsDigit(rvpEquation[i]);
                bool isSpace = Char.IsWhiteSpace(rvpEquation[i]);
                bool isDoubleSeparator = rvpEquation[i].ToString() == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

                if(isSpace)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            break;
                        case SequenceType.Literal:
                            double literal = double.Parse(sequence.ToString());
                            equation.Push(literal);                            
                            break;
                        case SequenceType.Identifier:
                            string identifier = sequence.ToString();
                            if(ArithmeticOperation.IsValid(identifier))
                            {
                                equation.Push(new ArithmeticOperation(identifier));
                            }
                            else
                            {
                                bool hasKey = variablesOut.TryGetValue(identifier, out ParsedVariable variable);
                                if (!hasKey)
                                {
                                    variable = new ParsedVariable(identifier, variablesOut.Count);
                                    variablesOut.Add(identifier, variable);
                                }                                
                                equation.Push(variable);
                            }
                            break;
                    }
                    sequenceType = SequenceType.None;
                }
                else if(isDigit || isDoubleSeparator)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            sequence.Clear();
                            sequence.Append(rvpEquation[i]);
                            sequenceType = SequenceType.Literal;
                            break;
                        case SequenceType.Literal:
                            sequence.Append(rvpEquation[i]);
                            break;
                        case SequenceType.Identifier:
                            throw new ArgumentException($"Digit follows a letter on {i} ({IdentifyCharacter(rvpEquation, i)}). Separate variables, constants and operations with white space.");
                            //sequence.Append(rvpEquation[i]);                            
                            break;
                    }                    
                }
                else if(isLetter)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            sequence.Clear();
                            sequence.Append(rvpEquation[i]);
                            sequenceType = SequenceType.Identifier;
                            break;
                        case SequenceType.Literal:
                            //sequence.Append(rvpEquation[i]);
                            throw new ArgumentException($"Letter follows a digit on {i} ({IdentifyCharacter(rvpEquation, i)}). Separate variables, constants and operations with white space.");
                            break;
                        case SequenceType.Identifier:
                            sequence.Append(rvpEquation[i]);
                            break;
                    }                    
                } 
                else if(isSpecialSymbol)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            sequence.Clear();
                            sequence.Append(rvpEquation[i]);
                            break;
                        case SequenceType.Literal:
                            throw new ArgumentException($"Special symbol follows a digit on {i} ({IdentifyCharacter(rvpEquation, i)}). Separate variables, constants and operations with white space.");
                            break;
                        case SequenceType.Identifier:
                            sequence.Append(rvpEquation[i]);
                            break;
                    }
                    sequenceType = SequenceType.Identifier;
                }
                else
                {
                    throw new ArgumentException($"Unexpected character on {i}: {IdentifyCharacter(rvpEquation, i)}");
                }
            }

            switch (sequenceType)
            {
                case SequenceType.None:
                    break;
                case SequenceType.Literal:
                    double literal = double.Parse(sequence.ToString());
                    equation.Push(literal);
                    break;
                case SequenceType.Identifier:
                    string identifier = sequence.ToString();
                    if (ArithmeticOperation.IsValid(identifier))
                    {
                        equation.Push(new ArithmeticOperation(identifier));
                    }
                    else
                    {
                        bool hasKey = variablesOut.TryGetValue(identifier, out ParsedVariable variable);
                        if (!hasKey)
                        {
                            variable = new ParsedVariable(identifier, variablesOut.Count);
                            variablesOut.Add(identifier, variable);
                        }
                        equation.Push(variable);
                    }
                    break;
            }

            return equation;
        }
    }
}
