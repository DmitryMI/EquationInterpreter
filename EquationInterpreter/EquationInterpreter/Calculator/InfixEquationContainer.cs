using EquationInterpreter.Equations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EquationInterpreter.Calculator.InfixEquationContainer;

namespace EquationInterpreter.Calculator
{
    class InfixEquationContainer : IList<InfixEquationElement>
    {
        public enum InfixEquationElementType
        {
            Literal,
            Identifier,
            FunctionCall,
            ArgumentSeparator,
            OpeningBracket,
            ClosingBracket
        }

        public class InfixEquationElement
        {
            public string StringValue { get;}
            public int OriginalStringPosition { get;  }
            public InfixEquationElementType ElementType { get; }
            public double LiteralValue { get; set; }
            public IPriorityOperation<double> Operation { get; set; }
            public IEquationVariable<double> Variable { get; set; }
            public int MajorPriority { get; set; }
            public int MinorPriority { get; set; }
            public InfixEquationElement(string value, int originalPosition, InfixEquationElementType type)
            {
                StringValue = value;
                ElementType = type;
                OriginalStringPosition = originalPosition;
            }
        }


        private List<InfixEquationElement> elements = new List<InfixEquationElement>();

        public int Count => ((ICollection<InfixEquationElement>)elements).Count;

        public bool IsReadOnly => ((ICollection<InfixEquationElement>)elements).IsReadOnly;

        public InfixEquationElement this[int index] { get => ((IList<InfixEquationElement>)elements)[index]; set => ((IList<InfixEquationElement>)elements)[index] = value; }

        public void Parse(string equationString, char[] identifierAdditionalCharset, char numberDecimalSeparator)
        {
            SequenceType sequenceType = SequenceType.None;
            StringBuilder sequence = new StringBuilder();
            for (int i = 0; i < equationString.Length; i++)
            {
                bool isLetter = Char.IsLetter(equationString[i]);
                bool isSpecialSymbol = Char.IsSymbol(equationString[i]) || identifierAdditionalCharset.Contains(equationString[i]);
                bool isDigit = Char.IsDigit(equationString[i]);
                bool isSpace = Char.IsWhiteSpace(equationString[i]);
                bool isDoubleSeparator = equationString[i] == numberDecimalSeparator;
                bool isOpeningBracket = equationString[i] == '(';
                bool isClosingBracket = equationString[i] == ')';
                bool isMinus = equationString[i] == '-';
                bool isComma = equationString[i] == ',';

                if (isSpace)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            break;
                        case SequenceType.Literal:
                            elements.Add(new InfixEquationElement(sequence.ToString(), i, InfixEquationElementType.Literal));
                            break;
                        case SequenceType.Identifier:
                        case SequenceType.LiteralOrIdentifier:
                            elements.Add(new InfixEquationElement(sequence.ToString(), i, InfixEquationElementType.Identifier));
                            break;
                    }
                    sequenceType = SequenceType.None;
                }
                else if(isMinus)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            sequence.Clear();
                            sequence.Append(equationString[i]);
                            sequenceType = SequenceType.LiteralOrIdentifier;
                            break;
                        case SequenceType.Literal:
                            throw new ArgumentException($"Minus symbol appeared on an unexpected place {i} ({MathEquationParser.IdentifyCharacter(equationString, i)})");
                            break;
                        case SequenceType.Identifier:
                            //elements.Add(new InfixEquationElement(sequence.ToString(), InfixEquationElementType.Identifier));
                            throw new ArgumentException($"Minus symbol appeared on an unexpected place {i} ({MathEquationParser.IdentifyCharacter(equationString, i)})");
                            break;
                    }
                }
                else if(isComma)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            elements.Add(new InfixEquationElement(",", i, InfixEquationElementType.ArgumentSeparator));
                            break;
                        case SequenceType.Literal:
                            elements.Add(new InfixEquationElement(sequence.ToString(), i, InfixEquationElementType.Literal));
                            elements.Add(new InfixEquationElement(",", i, InfixEquationElementType.ArgumentSeparator));
                            sequenceType = SequenceType.None;
                            break;
                        case SequenceType.Identifier:
                            elements.Add(new InfixEquationElement(",", i, InfixEquationElementType.ArgumentSeparator));
                            break;
                        case SequenceType.LiteralOrIdentifier:
                            throw new ArgumentException($"Argument separator symbol appeared on an unexpected place {i} ({MathEquationParser.IdentifyCharacter(equationString, i)})");
                            break;
                    }
                }
                else if (isDigit || isDoubleSeparator)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            sequence.Clear();
                            sequence.Append(equationString[i]);
                            sequenceType = SequenceType.Literal;
                            break;
                        case SequenceType.Literal:
                            sequence.Append(equationString[i]);
                            break;
                        case SequenceType.Identifier:
                            sequence.Append(equationString[i]);
                            break;
                        case SequenceType.LiteralOrIdentifier:
                            sequence.Append(equationString[i]);
                            sequenceType = SequenceType.Literal;
                            break;
                    }
                }
                else if (isLetter)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            sequence.Clear();
                            sequence.Append(equationString[i]);
                            sequenceType = SequenceType.Identifier;
                            break;
                        case SequenceType.Literal:
                            //sequence.Append(rvpEquation[i]);
                            throw new ArgumentException($"Letter follows a digit on {i} ({MathEquationParser.IdentifyCharacter(equationString, i)}). Separate variables, constants and operations with white space.");
                            break;
                        case SequenceType.Identifier:
                            sequence.Append(equationString[i]);
                            break;
                    }
                }
                else if (isOpeningBracket)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            elements.Add(new InfixEquationElement("(", i, InfixEquationElementType.OpeningBracket));
                            break;
                        case SequenceType.Literal:
                            //sequence.Append(rvpEquation[i]);
                            throw new ArgumentException($"Opening bracket follows a digit on {i} ({MathEquationParser.IdentifyCharacter(equationString, i)}).");
                            break;
                        case SequenceType.Identifier:
                            elements.Add(new InfixEquationElement(sequence.ToString(), i, InfixEquationElementType.FunctionCall));
                            elements.Add(new InfixEquationElement("(", i, InfixEquationElementType.OpeningBracket));
                            sequenceType = SequenceType.None;
                            break;
                    }
                }
                else if (isClosingBracket)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            elements.Add(new InfixEquationElement(")", i, InfixEquationElementType.ClosingBracket));
                            break;
                        case SequenceType.Literal:                            
                            elements.Add(new InfixEquationElement(sequence.ToString(), i, InfixEquationElementType.Literal));
                            elements.Add(new InfixEquationElement(")", i, InfixEquationElementType.ClosingBracket));
                            sequenceType = SequenceType.None;
                            break;
                        case SequenceType.Identifier:
                            sequence.Append(equationString[i]);
                            elements.Add(new InfixEquationElement(sequence.ToString(), i, InfixEquationElementType.Identifier));
                            elements.Add(new InfixEquationElement(")", i, InfixEquationElementType.ClosingBracket));
                            sequenceType = SequenceType.None;
                            break;
                    }
                }
                else if (isSpecialSymbol)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            sequence.Clear();
                            sequence.Append(equationString[i]);
                            break;
                        case SequenceType.Literal:
                            throw new ArgumentException($"Special symbol follows a digit on {i} ({MathEquationParser.IdentifyCharacter(equationString, i)}). Separate variables, constants and operations with white space.");
                            break;
                        case SequenceType.Identifier:
                            sequence.Append(equationString[i]);
                            break;
                    }
                    sequenceType = SequenceType.Identifier;
                }
                else
                {
                    throw new ArgumentException($"Unexpected character on {i}: {MathEquationParser.IdentifyCharacter(equationString, i)}");
                }
            }
        }

        public int IndexOf(InfixEquationElement item)
        {
            return ((IList<InfixEquationElement>)elements).IndexOf(item);
        }

        public void Insert(int index, InfixEquationElement item)
        {
            ((IList<InfixEquationElement>)elements).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<InfixEquationElement>)elements).RemoveAt(index);
        }

        public void Add(InfixEquationElement item)
        {
            ((ICollection<InfixEquationElement>)elements).Add(item);
        }

        public void Clear()
        {
            ((ICollection<InfixEquationElement>)elements).Clear();
        }

        public bool Contains(InfixEquationElement item)
        {
            return ((ICollection<InfixEquationElement>)elements).Contains(item);
        }

        public void CopyTo(InfixEquationElement[] array, int arrayIndex)
        {
            ((ICollection<InfixEquationElement>)elements).CopyTo(array, arrayIndex);
        }

        public bool Remove(InfixEquationElement item)
        {
            return ((ICollection<InfixEquationElement>)elements).Remove(item);
        }

        public IEnumerator<InfixEquationElement> GetEnumerator()
        {
            return ((IEnumerable<InfixEquationElement>)elements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)elements).GetEnumerator();
        }
    }
}
