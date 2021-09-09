using EquationInterpreter.Calculator.Arithmetics;
using EquationInterpreter.Equations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EquationInterpreter.Calculator.InfixEquationContainer;

namespace EquationInterpreter.Calculator
{
    public class MathEquationParser
    {      

        private static char[] additionalChars = new char[] { '*' };

        public static string IdentifyCharacter(string str, int position)
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

        private static IEquationPushable<double> ProcessNewIdentifier(string identifier, Dictionary<string, ParsedVariable> variablesOut)
        {
            if (ArithmeticOperation.IsValid(identifier))
            {
                return new ArithmeticOperation(identifier);
            }
            else if (MathMethodWrapper.IsValid(identifier))
            {
               return new MathMethodWrapper(identifier);
            }
            else
            {
                bool hasKey = variablesOut.TryGetValue(identifier, out ParsedVariable variable);
                if (!hasKey)
                {
                    variable = new ParsedVariable(identifier, variablesOut.Count);
                    variablesOut.Add(identifier, variable);
                }
               return variable;
            }
        }

        public static Equation<double> ParseReversePolishNotation(string rvpEquation, Dictionary<string, ParsedVariable> variablesOut)
        {
            Equation<double> equation = new Equation<double>();            
            
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
                            equation.Push(ProcessNewIdentifier(identifier, variablesOut));
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
                            sequence.Append(rvpEquation[i]);                            
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
                case SequenceType.Identifier:
                    string identifier = sequence.ToString();
                    equation.Push(ProcessNewIdentifier(identifier, variablesOut));
                    break;
            }

            return equation;
        }       

        private static string PrintInfixEquation(InfixEquationContainer infixEquationElements)
        {

            StringBuilder builder = new StringBuilder();
            foreach (var element in infixEquationElements)
            {
                if (element == null)
                {
                    builder.Append(". ");
                    continue;
                }

                builder.Append(element.StringValue).Append(' ');
            }
            return builder.ToString();
        }

        private static string PrintInfixEquationWithPriorities(InfixEquationContainer infixEquationElements)
        {
            StringBuilder prioritizedOperationsMessage = new StringBuilder();
            StringBuilder prioritiesMessage = new StringBuilder();
            foreach (var element in infixEquationElements)
            {
                if(element == null)
                {
                    prioritizedOperationsMessage.Append(". ");
                    prioritiesMessage.Append(' ', 2);
                    continue;
                }
                if (element.Operation == null)
                {
                    prioritizedOperationsMessage.Append(element.StringValue).Append(' ');
                    prioritiesMessage.Append(' ', element.StringValue.Length + 1);
                }
                else
                {
                    prioritizedOperationsMessage.Append(element.StringValue).Append(' ');
                    string priorityLabel = $"({element.MajorPriority},{element.MinorPriority})";
                    prioritiesMessage.Append(priorityLabel);
                    int lengthDifference = element.StringValue.Length - priorityLabel.Length + 1;
                    if (lengthDifference > 0)
                    {
                        prioritiesMessage.Append(' ', lengthDifference);
                    }
                    else
                    {
                        prioritizedOperationsMessage.Append(' ', -lengthDifference);
                    }
                }
            }

            return prioritiesMessage.ToString() + "\n" + prioritizedOperationsMessage.ToString();
        }

        private static int CompareOperationsPriority(InfixEquationElement a, InfixEquationElement b)
        {
            if(a.Operation == null || a.Operation == null)
            {
                return 0;
            }

            if(a.MajorPriority > b.MajorPriority)
            {
                return 1;
            }
            else if(a.MajorPriority < b.MajorPriority)
            {
                return -1;
            }
            else
            {
                return a.MinorPriority.CompareTo(b.MinorPriority);
            }
        }

        private static V[] DictionaryToArray<K, V>(Dictionary<K, V> variablesOut)
        {
            V[] array = new V[variablesOut.Count];
            int index = 0;
            foreach(KeyValuePair<K, V> pair in variablesOut)
            {
                array[index] = pair.Value;
                index++;
            }
            return array;
        }

        private static int GetMaximumPriorityOperation(InfixEquationContainer infixEquationElements)
        {
            //InfixEquationElement max = null;
            int maxIndex = -1;
            for(int i = 0; i < infixEquationElements.Count; i++)
            {
                InfixEquationElement current = infixEquationElements[i];
                if(current == null)
                {
                    continue;
                }
                if(current.Operation == null)
                {
                    continue;
                }

                if(maxIndex == -1)
                {                    
                    maxIndex = i;
                }
                else
                {
                    int comparison = CompareOperationsPriority(infixEquationElements[maxIndex], current);
                    if(comparison < 0)
                    {                       
                        maxIndex = i;
                    }
                }
            }

            return maxIndex;
        }

        private static InfixEquationElement[] GrapFunctionAndArguments(InfixEquationContainer infixEquationElements, string originalString, int functionIndex, ref int elementsLeft)
        {
            InfixEquationElement operationElement = infixEquationElements[functionIndex];
            infixEquationElements[functionIndex] = null;
            elementsLeft--;
            IPriorityOperation<double> operation = operationElement.Operation;

            InfixEquationElement nextElement = infixEquationElements[functionIndex + 1];
            if(nextElement == null)
            {
                // Arguments already grabbed
                return new InfixEquationElement[0];
            }
            if(nextElement.ElementType != InfixEquationElementType.OpeningBracket)
            {
                throw new ArgumentException($"Opening Bracket expected on {operationElement.OriginalStringPosition}: {IdentifyCharacter(originalString, operationElement.OriginalStringPosition)}");
            }

            infixEquationElements[functionIndex + 1] = null;
            elementsLeft--;

            List<InfixEquationElement> arguments = new List<InfixEquationElement>();
            bool pendingComma = false;
            int argumentsNumber = 1;
            for(int i = functionIndex + 2; i < infixEquationElements.Count; i++)
            {
                InfixEquationElement element = infixEquationElements[i];                

                if(element == null)
                {
                    pendingComma = false;
                    continue;
                }
                else if (element.ElementType == InfixEquationElementType.ArgumentSeparator)
                {
                    argumentsNumber++;
                    pendingComma = true;
                    infixEquationElements[i] = null;
                    elementsLeft--;
                    continue;
                }            
                else if (element.ElementType == InfixEquationElementType.ClosingBracket)
                {
                    if(pendingComma)
                    {
                        throw new ArgumentException($"No argument after a comma on {i}: {IdentifyCharacter(originalString, element.OriginalStringPosition)}");
                    }
                    infixEquationElements[i] = null;
                    elementsLeft--;
                    break;
                }
                else if(element.ElementType == InfixEquationElementType.Literal || element.Variable != null)
                {
                    pendingComma = false;
                    arguments.Add(element);
                    infixEquationElements[i] = null;
                    elementsLeft--;
                }
            }

            if(argumentsNumber != operation.ArgumentsNumber)
            {
                throw new ArgumentException($"Wrong number of arguments for function {operation}. Expected: {operation.ArgumentsNumber}, actual: {argumentsNumber}");
            }
            return arguments.ToArray();
        }
        
        private static InfixEquationElement[] GrabOperationSingleArgument(InfixEquationContainer infixEquationElements, int operationIndex, string originalStr, ref int elementsLeft)
        {
            InfixEquationElement operationElement = infixEquationElements[operationIndex];
            infixEquationElements[operationIndex] = null;
            elementsLeft--;
            IPriorityOperation<double> operation = operationElement.Operation;

            InfixEquationElement nextElement = infixEquationElements[operationIndex + 1];
            if (nextElement == null)
            {
                // Argument already grabbed
                return new InfixEquationElement[0];
            }

            if(nextElement.ElementType == InfixEquationElementType.OpeningBracket)
            {
                infixEquationElements[operationIndex + 1] = null;
                elementsLeft--;
                if (infixEquationElements[operationIndex + 2].ElementType != InfixEquationElementType.ClosingBracket)
                {
                    throw new ArgumentException("Something went wrong");
                }
                infixEquationElements[operationIndex + 2] = null;
                elementsLeft--;
                return new InfixEquationElement[0];
            }

            bool isNextLiteral = operationElement.ElementType == InfixEquationElementType.Literal;
            bool isNextVariable = operationElement.Variable != null;
            if (!isNextLiteral && isNextVariable)
            {
                throw new ArgumentException($"Literal or variable expected on {operationElement.OriginalStringPosition}: {IdentifyCharacter(originalStr, operationElement.OriginalStringPosition)}");
            }

            infixEquationElements[operationIndex + 1] = null;
            elementsLeft--;
            return new InfixEquationElement[] { nextElement };
        }

        private static void RemoveBrackets(InfixEquationContainer infixEquationElements, int bracketIndex, string originalString, ref int elementsLeft)
        {
            InfixEquationElement bracket = infixEquationElements[bracketIndex];
            infixEquationElements[bracketIndex] = null;
            elementsLeft--;
            InfixEquationElementType expectedType;
            int direction = 0;
            if(bracket.ElementType == InfixEquationElementType.OpeningBracket)
            {
                expectedType = InfixEquationElementType.ClosingBracket;
                direction = 1;
            }
            else if(bracket.ElementType == InfixEquationElementType.ClosingBracket)
            {
                expectedType = InfixEquationElementType.OpeningBracket;
                direction = -1;
            }
            else
            {
                throw new ArgumentException("Argument is not a bracket");
            }
            bool bracketRemoved = false;
            for (int i = bracketIndex + direction; i >= 0 && i < infixEquationElements.Count; i += direction)
            {         
                if(infixEquationElements[i] == null)
                {
                    continue;
                }
                else if (infixEquationElements[i].ElementType != expectedType)
                {
                    throw new ArgumentException($"Unexpected token on {infixEquationElements[i].OriginalStringPosition}: {IdentifyCharacter(originalString, infixEquationElements[i].OriginalStringPosition)}");
                }
                else
                {
                    bracketRemoved = true;
                    infixEquationElements[i] = null;
                    elementsLeft--;
                    break;
                }
            }

            if(!bracketRemoved)
            {
                throw new ArgumentException($"Matching bracked not found for {bracket.OriginalStringPosition}: {IdentifyCharacter(originalString, bracket.OriginalStringPosition)}");
            }
        }

        private static InfixEquationElement[] GrabOperationTwoArguments(InfixEquationContainer infixEquationElements, int operationIndex, string originalStr, ref int elementsLeft)
        {
            InfixEquationElement operationElement = infixEquationElements[operationIndex];
            infixEquationElements[operationIndex] = null;
            elementsLeft--;
            IPriorityOperation<double> operation = operationElement.Operation;
            List<InfixEquationElement> resultingList = new List<InfixEquationElement>(2);

            InfixEquationElement previousElement = infixEquationElements[operationIndex - 1];
            if(previousElement == null)
            {
                // Previous argument already grabbed
            }
            else
            {
                if(previousElement.ElementType == InfixEquationElementType.OpeningBracket)
                {
                    infixEquationElements[operationIndex - 1] = null;
                    elementsLeft--;
                }
                else if(previousElement.ElementType == InfixEquationElementType.ClosingBracket)
                {
                    //infixEquationElements[operationIndex - 1] = null;
                    //elementsLeft--;
                    RemoveBrackets(infixEquationElements, operationIndex - 1, originalStr, ref elementsLeft);
                }
                else if(previousElement.ElementType == InfixEquationElementType.Literal || previousElement.Variable != null)
                {
                    resultingList.Add(previousElement);
                    infixEquationElements[operationIndex - 1] = null;
                    elementsLeft--;
                }
                else
                {
                    throw new ArgumentException($"Unexpected token on {previousElement.OriginalStringPosition}: {IdentifyCharacter(originalStr, previousElement.OriginalStringPosition)}");
                }
            }

            InfixEquationElement nextElement = infixEquationElements[operationIndex + 1];
            if (nextElement == null)
            {
                // Next argument already grabbed               
            }
            else
            {
                if (nextElement.ElementType == InfixEquationElementType.ClosingBracket)
                {
                    infixEquationElements[operationIndex + 1] = null;
                    elementsLeft--;
                }
                else if(nextElement.ElementType == InfixEquationElementType.OpeningBracket)
                {
                    RemoveBrackets(infixEquationElements, operationIndex + 1, originalStr, ref elementsLeft);
                }
                else if (nextElement.ElementType == InfixEquationElementType.Literal || previousElement.Variable != null)
                {
                    resultingList.Add(nextElement);
                    infixEquationElements[operationIndex + 1] = null;
                    elementsLeft--;
                }
                else
                {
                    throw new ArgumentException($"Unexpected token on {nextElement.OriginalStringPosition}: {IdentifyCharacter(originalStr, nextElement.OriginalStringPosition)}");
                }
            }

            return resultingList.ToArray();
        }

        private static InfixEquationElement[] GrabOperationAndArguments(InfixEquationContainer infixEquationElements, string originalString, int operationIndex, ref int elementsLeft)
        {
            InfixEquationElement operationElement = infixEquationElements[operationIndex];
            IPriorityOperation<double> operation = operationElement.Operation;
            int arguments = operation.ArgumentsNumber;
            if(operationElement.ElementType == InfixEquationElementType.FunctionCall)
            {
                return GrapFunctionAndArguments(infixEquationElements, originalString, operationIndex, ref elementsLeft);
            }

            if(arguments == 1)
            {
                return GrabOperationSingleArgument(infixEquationElements, operationIndex, originalString, ref elementsLeft);
            }
            if(arguments == 2)
            {
                return GrabOperationTwoArguments(infixEquationElements, operationIndex, originalString, ref elementsLeft);
            }
            else
            {
                throw new InvalidOperationException($"Non-function operation must use only 1 or 2 arguments, but operation {operation} requires {arguments} arguments");
            }
        }

        private static void PushToEquation(InfixEquationElement infixEquationElement, Equation<double> equation)
        {            
            switch (infixEquationElement.ElementType)
            {
                case InfixEquationElementType.Literal:
                    equation.Push(infixEquationElement.LiteralValue);
                    break;
                case InfixEquationElementType.Identifier:
                    if (infixEquationElement.Variable != null)
                    {
                        equation.Push(infixEquationElement.Variable);
                    }
                    else if(infixEquationElement.Operation != null)
                    {
                        equation.Push(infixEquationElement.Operation);
                    }
                    else
                    {
                        throw new ArgumentException("Element's Variable and Operation values are both null!");
                    }
                    break;
                case InfixEquationElementType.FunctionCall:
                    if (infixEquationElement.Operation != null)
                    {
                        equation.Push(infixEquationElement.Operation);
                    }
                    else
                    {
                        throw new ArgumentException("FunctionCall's Operation value is null!");
                    }
                    break;
                default:
                    throw new ArgumentException($"{infixEquationElement.ElementType} cannot be pushed to Equation!");
                    break;
            }
        }

        public static Equation<double> Parse(string equationString, Dictionary<string, ParsedVariable> variablesOut)
        {
            InfixEquationContainer infixEquationElements = new InfixEquationContainer();
            infixEquationElements.Parse(equationString, additionalChars, ',');

#if DEBUG            
            Debug.WriteLine(PrintInfixEquation(infixEquationElements));
#endif

            int currentMajorPriority = 0;
            for(int i = 0; i < infixEquationElements.Count; i++)
            {
                InfixEquationElement element = infixEquationElements[i];
                switch (element.ElementType)
                {
                    case InfixEquationElementType.Literal:
                        element.LiteralValue = double.Parse(element.StringValue);
                        break;
                    case InfixEquationElementType.Identifier:
                        IEquationPushable<double> pushable = ProcessNewIdentifier(element.StringValue, variablesOut);
                        if (pushable is IPriorityOperation<double> operation)
                        {
                            element.Operation = operation;
                            element.MinorPriority = operation.Priority;
                            element.MajorPriority = currentMajorPriority;
                        }
                        else if (pushable is IEquationVariable<double> variable)
                        {
                            element.Variable = variable;
                        }
                        break;
                    case InfixEquationElementType.OpeningBracket:
                        currentMajorPriority++;
                        break;
                    case InfixEquationElementType.ClosingBracket:
                        currentMajorPriority--;
                        break;
                    case InfixEquationElementType.FunctionCall:
                        IPriorityOperation<double> functionCall = (IPriorityOperation<double>)ProcessNewIdentifier(element.StringValue, variablesOut);
                        element.Operation = functionCall;
                        element.MinorPriority = functionCall.Priority;
                        element.MajorPriority = currentMajorPriority;
                        break;
                    case InfixEquationElementType.ArgumentSeparator:
                        break;
                }
            }

#if DEBUG
            Debug.WriteLine(PrintInfixEquationWithPriorities(infixEquationElements));
#endif

            ParsedVariable[] variablesArray = DictionaryToArray(variablesOut);
            Equation<double> equation = new Equation<double>(variablesArray);
            int elementsLeft = infixEquationElements.Count;

            Debug.Write($"({elementsLeft:000}): ");
            Debug.WriteLine(PrintInfixEquation(infixEquationElements));
            while(elementsLeft > 0)
            {
                int maxPriorityIndex = GetMaximumPriorityOperation(infixEquationElements);
                if(maxPriorityIndex == -1)
                {
                    break;
                }
                InfixEquationElement maxPriorityElement= infixEquationElements[maxPriorityIndex];
                IPriorityOperation<double> operation = maxPriorityElement.Operation;
                InfixEquationElement[] arguments = GrabOperationAndArguments(infixEquationElements, equationString, maxPriorityIndex, ref elementsLeft);
                Debug.Write($"({elementsLeft:000}): ");
                Debug.WriteLine(PrintInfixEquation(infixEquationElements));
                for (int i = 0; i < arguments.Length; i++)
                {
                    PushToEquation(arguments[i], equation);
                }
                equation.Push(operation);
            }

            for(int i = 0; i < infixEquationElements.Count; i++)
            {
                InfixEquationElement element = infixEquationElements[i];
                if(element == null)
                {
                    continue;
                }
                infixEquationElements[i] = null;
                elementsLeft--;
                PushToEquation(element, equation);
                Debug.Write($"({elementsLeft}): ");
                Debug.WriteLine(PrintInfixEquation(infixEquationElements));
            }

            return equation;
        }
    }
}
