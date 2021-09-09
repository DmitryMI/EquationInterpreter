using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationInterpreter.Equations
{
    public class Equation<T> : IEquationOperation<T>
    {
        private List<IEquationVariable<T>> variableSet;
        public int ArgumentsNumber => variableSet.Count;

        private List<EquationElement<T>> equationList = new List<EquationElement<T>>();

        public IEquationVariable<T>[] Variables => variableSet.ToArray();

        public T Calculate(params T[] operands)
        {
            Stack<EquationElement<T>> stack = new Stack<EquationElement<T>>();

            for(int i = 0; i < equationList.Count; i++)
            {
                EquationElement<T> equationElement = equationList[i];                
                switch (equationElement.ElementType)
                {
                    case EquationElementType.Operation:
                        ProcessOperation(stack, equationElement.Operator, operands);
                        break;
                    case EquationElementType.Literal:
                        stack.Push(equationElement);
                        break;
                    case EquationElementType.Variable:
                        stack.Push(equationElement);
                        break;
                }
            }

            T result = stack.Pop().Immediate;
            return result;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var element in equationList)
            {
                stringBuilder.Append(element.ToString()).Append(' ');
            }
            return stringBuilder.ToString();
        }

        private void ProcessOperation(Stack<EquationElement<T>> stack, IEquationOperation<T> operation, T[] operands)
        {
            T[] values = new T[operation.ArgumentsNumber];
            for (int i = 0; i < operation.ArgumentsNumber; i++)
            {
                T value;
                EquationElement<T> element = stack.Pop();
                if (element.ElementType == EquationElementType.Literal)
                {
                    value = element.Immediate;
                }
                else if (element.ElementType == EquationElementType.Variable)
                {
                    IEquationVariable<T> variable = element.Variable;
                    if (variable.HasValue)
                    {
                        value = variable.Value;
                    }
                    else if (operands.Length > variable.Index)
                    {
                        value = operands[variable.Index];
                    }            

                    else
                    {
                        throw new InvalidOperationException($"Variable {variable} has no value");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Stack is currupted");
                }
                values[operation.ArgumentsNumber - i - 1] = value;
            }

            T result = operation.Calculate(values);
            stack.Push(new EquationElement<T>(result));
        }

        public Equation(IEnumerable<IEquationVariable<T>> equationVariables)
        {
            if (equationVariables != null)
            {
                variableSet = new List<IEquationVariable<T>>(equationVariables);
            }
            else
            {
                variableSet = new List<IEquationVariable<T>>();
            }
        }

        public Equation()
        {
            variableSet = new List<IEquationVariable<T>>();
        }

        public void Push(IEquationPushable<T> pushable)
        {
            if(pushable is IEquationVariable<T> variable)
            {
                Push(variable);
            }
            else if(pushable is IEquationOperation<T> operation)
            {
                Push(operation);
            }
            else
            {
                throw new ArgumentException($"Pushable {pushable} is neither a variable nor an operation");
            }
        }

        public void Push(T literal)
        {
            EquationElement<T> equationElement = new EquationElement<T>(literal);
            equationList.Add(equationElement);
        }
        public void Push(IEquationOperation<T> operation)
        {
            EquationElement<T> equationElement = new EquationElement<T>(operation);
            equationList.Add(equationElement);
        }
        public void Push(IEquationVariable<T> variable)
        {
            if (!variableSet.Contains(variable))
            {
                //throw new ArgumentException($"Variable {variable} is not registered");
                variableSet.Add(variable);
            }

            EquationElement<T> equationElement = new EquationElement<T>(variable);
            equationList.Add(equationElement);
        }

        public void Push(string variableName)
        {
            IEquationVariable<T> equationVariable = variableSet.Find(v => v.Name == variableName);
            if (equationVariable == null)
            {
                throw new ArgumentException($"Variable {variableName} is not registered");
            }
            EquationElement<T> equationElement = new EquationElement<T>(equationVariable);
            equationList.Add(equationElement);
        }

        public void PushVariableByIndex(int variableIndex)
        {
            if (variableIndex > variableSet.Count)
            {
                IEquationVariable<T> equationVariable = variableSet[variableIndex];
                EquationElement<T> equationElement = new EquationElement<T>(equationVariable);
                equationList.Add(equationElement);
            }
            else
            {
                throw new ArgumentException($"Variable with index {variableIndex} is not registered");
            }
        }
    }
}
