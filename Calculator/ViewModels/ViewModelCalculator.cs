using Calculator.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Calculator.ViewModels
{
    class ViewModelCalculator : ViewModel
    {
        public ICommand CalculateCommand { get; set; }
        public ICommand NumberCommand { get; set; }
        public ICommand OperatorCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        private int LeftParenthesisCount { get; set; }
        private int RightParenthesisCount { get;  set; }

        private string expression;

        private bool Calculated { get; set; }
        private List<string> ExpressionElements { get; set; }

        public ViewModelCalculator()
        {
            this.Expression = string.Empty;
            this.LeftParenthesisCount = 0;
            this.Calculated = false;
            this.ExpressionElements = new List<string>();
            this.NumberCommand = new RelayCommandWithParameter(WriteNumberToExpression);
            this.OperatorCommand = new RelayCommandWithParameter(WriteOperatorToExpression);
            this.CalculateCommand = new RelayCommand(ConvertToReversePolishNotation);
            this.ClearCommand = new RelayCommand(ClearExpression);
        }

        
        public string Expression
        {
            get
            {
                return this.expression;
            }
            private set
            {
                this.expression = value;
                base.OnPropertyChanged("Expression");
            }
        }

        #region Command Handlers

        private void WriteNumberToExpression(string parameter)
        {
            if (this.Calculated)
	        {
		        this.ExpressionElements = new List<string>();
                this.Calculated = false;
	        }
            int elementsCount = this.ExpressionElements.Count;
            if (elementsCount > 0)
            {
                int lastElementIndex = this.ExpressionElements.Count - 1;
                string lastElement = this.ExpressionElements[lastElementIndex];
                if (this.IsDigit(parameter) && this.IsNumber(lastElement))
                {
                    this.ExpressionElements[lastElementIndex] += parameter;
                }
                else if (this.IsDecimalPoint(parameter) && this.IsNumber(lastElement) && !lastElement.Contains(','))
                {
                    this.ExpressionElements[lastElementIndex] += parameter;
                }
                else if (!this.IsNumber(lastElement) && !this.IsDecimalPoint(parameter))
                {
                    this.ExpressionElements.Add(parameter);
                }
            }
            else if (!this.IsDecimalPoint(parameter))
            {
                this.ExpressionElements.Add(parameter);
            }
            this.Expression = this.GetExpression();
        }

        private string GetExpression()
        {
            string result = string.Empty;
            foreach (string item in this.ExpressionElements)
            {
                if (this.ValidateOperator(item))
                {
                    result += " " + item + " ";
                }
                else
                {
                    result += item;
                }
            }
            return result;
        }

        private bool IsDigit(string token)
        {
            return token[0] - '0' >= 0 && token[0] - '0' <= 9;
        }

        private bool IsNumber(string str)
        {
            for (int index = 0; index < str.Length; index++)
            {
                if (str.Length > 1 && index == 0 && str[index] == '-')
                {
                    index++;
                }
                if (!(this.IsDigit(str[index].ToString()) || this.IsDecimalPoint(str[index].ToString())))
                {
                    return false;
                }
            }
            return true;
        }

        private void ClearExpression()
        {
            this.Expression = string.Empty;
            this.ExpressionElements = new List<string>();
        }

        private bool IsParenthesis(string token)
        {
            return this.ValidateLeftParenthesis(token) || this.ValidateRightParenthesis(token);
        }

        private void WriteOperatorToExpression(string parameter)
        {
            this.Calculated = false;
            int elementsCount = this.ExpressionElements.Count;
            if (elementsCount > 0 )
            {
                int lastElementIndex = elementsCount - 1;
                string lastElement = this.ExpressionElements[lastElementIndex];
                if (lastElement.Length >= 2 && this.IsNumber(lastElement) && lastElement.EndsWith(","))
                {
                    this.ExpressionElements[lastElementIndex] = lastElement.Remove(lastElement.Length - 1, 1);
                }
                if (this.ValidateLeftParenthesis(lastElement) && !this.IsParenthesis(parameter))
                {
                    this.ExpressionElements.Add("0");
                    this.ExpressionElements.Add(parameter);
                }
                if ((this.IsNumber(lastElement) || this.ValidateRightParenthesis(lastElement)) && !this.IsParenthesis(parameter))
                {
                    this.ExpressionElements.Add(parameter);
                }
                else if ((this.ValidateOperator(lastElement) || this.ValidateLeftParenthesis(lastElement)) && this.ValidateLeftParenthesis(parameter))
                {
                    this.ExpressionElements.Add(parameter);
                    this.LeftParenthesisCount++;
                }
                else if (((this.IsNumber(lastElement) || this.ValidateRightParenthesis(lastElement)) && this.ValidateRightParenthesis(parameter)) 
                    && this.RightParenthesisCount < this.LeftParenthesisCount)
                {
                    this.ExpressionElements.Add(parameter);
                    this.RightParenthesisCount++;
                }
            }
            else if (this.ValidateLeftParenthesis(parameter))
            {
                this.ExpressionElements.Add(parameter);
                this.LeftParenthesisCount++;
            }
            this.Expression = this.GetExpression();
        }

        private bool IsDecimalPoint(string token)
        {
            return token == ",";
        }

        #region Convertion To Reverse Polish Notation

        private int GerPrecedence(string mathOperator)
        {
            switch (mathOperator)
            {
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                    return 2;
                case "^":
                    return 3;
                default:
                    return 0;
            }
        }

        private bool IsOperatorLeftAssociative(string mathOperator)
        {
            string[] signs = { "+", "-", "*", "/" };
            if (signs.Contains(mathOperator))
            {
                return true;
            }
            return false;
        }

        private bool ValideteAssociativityAndPrecedence(Stack<string> operators, string operatorFromExpression)
        {
            if (operators.Count > 0 &&
                ((IsOperatorLeftAssociative(operatorFromExpression) &&
                  GerPrecedence(operatorFromExpression) == GerPrecedence(operators.Peek())) ||
                 GerPrecedence(operatorFromExpression) < GerPrecedence(operators.Peek())))
            {
                return true;
            }
            return false;
        }

        private bool ValidateOperator(string token)
        {
            string[] operatorTokens = { "+", "-", "*", "/", "^" };
            return operatorTokens.Contains(token);
        }

        private bool ValidateLeftParenthesis(string token)
        {
            return token == "(";
        }

        private bool ValidateRightParenthesis(string token)
        {
            return token == ")";
        }

        public void ConvertToReversePolishNotation()
        {
            List<string> rpn = new List<string>();
            Stack<string> operators = new Stack<string>();

            List<string> expressionMembers = this.ExpressionElements;
            int tokensInExpressionCount = expressionMembers.Count;
            for (int index = 0; index < tokensInExpressionCount; index++)
            {
                string token = expressionMembers[index].ToString();
                if (ValidateOperator(token))
                {
                    while (ValideteAssociativityAndPrecedence(operators, token))
                    {
                        rpn.Add(operators.Pop());
                    }
                    operators.Push(token);
                }
                else if (ValidateLeftParenthesis(token))
                {
                    operators.Push(token);
                }
                else if (ValidateRightParenthesis(token))
                {
                    while (!ValidateLeftParenthesis(operators.Peek()))
                    {
                        rpn.Add(operators.Pop());
                    }
                    if (ValidateLeftParenthesis(operators.Peek()))
                    {
                        operators.Pop();
                    }
                }
                else if (token != " " && token != string.Empty)
                {
                    rpn.Add(token);
                }
            }
            while (operators.Count > 0)
            {
                rpn.Add(operators.Pop());
            }

            CalculateRpn(rpn);
        }

        #endregion Convertion To Reverse Polish Notation

        #region RPN Calculation Methods

        private void CalculateRpn(List<string> rpn)
        {            
            if (rpn.Count > 0)
            {
                Stack<decimal> stack = new Stack<decimal>();
                decimal number = 0M;
                foreach (string token in rpn)
                {
                    if (decimal.TryParse(token, out number))
                    {
                        stack.Push(number);
                    }
                    else if (stack.Count > 1)
                    {
                        switch (token)
                        {
                            case "^":
                                number = stack.Pop();
                                stack.Push((decimal)Math.Pow((double)stack.Pop(), (double)number));
                                break;
                            case "*":
                                stack.Push(stack.Pop() * stack.Pop());
                                break;
                            case "/":
                                number = stack.Pop();
                                if (number == 0)
                                {
                                    this.Expression = "Cannot divide by 0";
                                    this.LeftParenthesisCount = 0;
                                    this.RightParenthesisCount = 0;
                                    this.ExpressionElements = new List<string>();
                                    return;
                                }
                                stack.Push(stack.Pop() / number);
                                break;
                            case "+":
                                stack.Push(stack.Pop() + stack.Pop());
                                break;
                            case "-":
                                number = stack.Pop();
                                stack.Push(stack.Pop() - number);
                                break;
                            default:
                                break;
                        }
                    }
                }
                this.LeftParenthesisCount = 0;
                this.RightParenthesisCount = 0;
                this.ExpressionElements = new List<string>();
                decimal result = stack.Pop();
                this.ExpressionElements.Add(string.Format("{0:G29}", result));
                this.Expression = this.GetExpression();
                this.Calculated = true;
            }
        }

        #endregion RPN Calculation Methods

        #endregion Command Handlers

    }
}