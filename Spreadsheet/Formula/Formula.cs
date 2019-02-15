// Alexander Bartee, PS4a

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static Formulas.TokenType;
using System.Linq;

namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public class Formula
    {
        private List<Token> theFormula;
        private int size;
        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        public Formula(String formula)
        {
            if (formula == null)
            {
                throw new ArgumentNullException();
            }
            // Attempt to read formula
            IEnumerable<Token> tokens;
            try
            {
                tokens = GetTokens(formula);
            }
            catch (InvalidOperationException e)
            {
                throw e;
            }

            //Constants for the next boolean, this will help to track whether we should see a number
            //or a operator
            bool NUM = true;
            bool OPER = false;

            //counts the number of tokens to ensure no empty formula
            size = 0;
            //counts how deep into parentheses the formula is to detect errors
            int parenCount = 0;
            //tracks what the next token type should be
            bool next = NUM;

            //iterates the tokens, incrementing count and validating each one
            //compares the current token to next to see if there is a contradiction
            //flips next from oper to num and num to oper if one pops up
            foreach (Token token in tokens)
            {
                size++;
                switch (token.type)
                {
                    case Invalid:
                        throw new FormulaFormatException("Invalid item in formula: " + token.text + " in index " + size);
                    case LParen:
                        if (next != NUM)
                        {
                            throw new FormulaFormatException("Opening Parentheses is misplaced");
                        }
                        parenCount++;
                        break;
                    case RParen:
                        if (next != OPER)
                        {
                            throw new FormulaFormatException("Closing Parentheses is misplaced");
                        }
                        parenCount--;
                        break;
                    case Number:
                    case Var:
                        if (next != NUM)
                        {
                            throw new FormulaFormatException("Number or Variable is misplaced");
                        }
                        next = OPER;
                        break;
                    case Oper:
                        if (next != OPER)
                        {
                            throw new FormulaFormatException("Operator is misplaced");
                        }
                        next = NUM;
                        break;
                }
                if (parenCount < 0)
                {
                    throw new FormulaFormatException("Parentheses overclosed");
                }

            }
            if (parenCount != 0)
            {
                throw new FormulaFormatException("Parentheses are left open");
            }
            if (size <= 0)
            {
                throw new FormulaFormatException("Not enough operations to create a formula");
            }
            if (next == NUM)
            {
                throw new FormulaFormatException("Invalid closing operator");
            }
            theFormula = tokens.ToList<Token>();
        }

        /// <summary>
        /// Additional constructor for PS4
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="normal"></param>
        /// <param name="valid"></param>
        public Formula(string formula, Normalizer normalizer, Validator validator)
            //run the original constructor to scan for initial errors
            : this(formula)
        {
            if (formula == null || normalizer == null || validator == null)
            {
                throw new ArgumentNullException();
            }
            // scan for the normalizer and validator
            try
            {
                int index = 0;

                while (index < theFormula.Count)
                {
                    Token token = theFormula[index];
                    if (token.type == Var)
                    {
                        theFormula[index] = new Token(normalizer(token.text),token.type);
                        if (!validator(theFormula[index].text))
                        {
                            throw new FormulaFormatException("Variable is not valid");
                        }
                    }
                    index++;
                }

            }
            catch (UndefinedVariableException)
            {
                throw new FormulaFormatException("Normalized variable does not exist");
            }
        }
        /// <summary>
        /// PS4 method, retrieves all variables from the formula
        /// </summary>
        /// <returns></returns>
        public ISet<string> GetVariables(){
            
            HashSet<string> variables = new HashSet<string>();
            foreach (Token token in theFormula)
            {
                if (token.type == Var)
                {
                    variables.Add(token.text);
                }
            }
            return variables;
        }
        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)
        {
            if (lookup == null)
            {
                throw new ArgumentNullException();
            }
            Stack<string> oper = new Stack<string>();
            Stack<double> value = new Stack<double>();

            foreach (Token token in theFormula)
            {
                switch (token.type)
                {
                    case Var:
                    case Number:

                        double num;
                        if (token.type == Var)
                        {
                            try {
                                num = Convert.ToDouble(lookup(token.text));
                            }
                            catch (UndefinedVariableException)
                            {
                                throw new FormulaEvaluationException("Variable Undefined: " + token.text);
                            }
                            
                        }
                        else
                        {
                            num = Convert.ToDouble(token.text);
                        }

                        if (oper.Count >= 1 && (oper.Peek() == "*" || oper.Peek() == "/"))
                        {
                            value.Push(Calculate(num, oper.Pop(), value.Pop()));
                        }
                        else
                        {
                            value.Push(num);
                        }
                        break;

                    case Oper:
                        switch (token.text)
                        {
                            case "+":
                            case "-":
                                if (oper.Count >= 1 && (oper.Peek() == "+" || oper.Peek() == "-"))
                                {
                                    value.Push(Calculate(value.Pop(), oper.Pop(), value.Pop()));
                                }
                                oper.Push(token.text);
                                break;
                            case "*":
                            case "/":
                                oper.Push(token.text);
                                break;
                        }
                        break;
                    case LParen:
                        oper.Push(token.text);
                        break;
                    case RParen:
                        if (oper.Count >= 1 && (oper.Peek() == "+" || oper.Peek() == "-"))
                        {
                            value.Push(Calculate(value.Pop(), oper.Pop(), value.Pop()));
                        }
                        oper.Pop();
                        if (oper.Count >= 1 && (oper.Peek() == "*" || oper.Peek() == "/"))
                        {
                            value.Push(Calculate(value.Pop(), oper.Pop(), value.Pop()));
                        }
                        break;
                }
            }
            if (oper.Count == 0)
            {
                return value.Pop();
            }
            return Calculate(value.Pop(),oper.Pop(),value.Pop());
        }
        /// <summary>
        /// toString default method override
        /// </summary>
        /// <returns></returns>
        override public string ToString()
        {
            string final = "";

            foreach (Token token in theFormula)
            {
                final += token.text;
            }

            return final;
        }
        /// <summary>
        /// Basic calculator function that takes in 2 numbers and an operation and performs them on each other.
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="op"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        private double Calculate(string num1, string op, double num2)
        {
            return Calculate(Convert.ToDouble(num1), op, num2);
        }
        /// <summary>
        /// Basic calculator function that takes in 2 numbers and an operation and performs them on each other.
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="op"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        private double Calculate(double num1, string op, double num2)
        {
            switch (op)
            {
                case "+":
                    return num1 + num2;
                case "-":
                    return num2 - num1;
                case "*":
                    return num1 * num2;
                case "/":
                    if (num1 == 0)
                    {
                        throw new FormulaEvaluationException("Divide by 0");
                    }
                    return num2 / num1;
            }
            throw new FormulaEvaluationException("Operator not recognized");
        }

        /// <summary>
        /// Simple debugging code to output what got saved into the formula's tokens
        /// </summary>
        /// <returns></returns>
        public string printFormula()
        {
            string output = "";
            foreach (Token token in theFormula)
            {
                output += token.text;
            }
            return output;
        }
        /// <summary>
        /// Token struct to steamline the process for PS4
        /// </summary>
        public struct Token
        {
            public string text;
            public TokenType type;
            public Token(string text, TokenType type)
            {
                this.text = text;
                this.type = type;
            }
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Each token is described by a
        /// Tuple containing the token's text and TokenType.  There are no empty tokens, and no
        /// token contains white space.
        /// </summary>
        private static IEnumerable<Token> GetTokens(String formula)
        {
            if (formula == null)
            {
                throw new ArgumentNullException();
            }
            // Patterns for individual tokens.
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";

            // NOTE:  I have added white space to this regex to make it more readable.
            // When the regex is used, it is necessary to include a parameter that says
            // embedded white space should be ignored.  See below for an example of this.
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall token pattern.  It contains embedded white space that must be ignored when
            // it is used.  See below for an example of this.
            String tokenPattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5}) | (.)",
                                            spacePattern, lpPattern, rpPattern, opPattern, varPattern, doublePattern);

            // Create a Regex for matching tokens.  Notice the second parameter to Split says 
            // to ignore embedded white space in the pattern.
            Regex r = new Regex(tokenPattern, RegexOptions.IgnorePatternWhitespace);

            // Look for the first match
            Match match = r.Match(formula);

            // Start enumerating tokens
            while (match.Success)
            {
                // Ignore spaces
                if (!match.Groups[1].Success)
                {
                    // Holds the token's type
                    TokenType type;

                    if (match.Groups[2].Success)
                    {
                        type = LParen;
                    }
                    else if (match.Groups[3].Success)
                    {
                        type = RParen;
                    }
                    else if (match.Groups[4].Success)
                    {
                        type = Oper;
                    }
                    else if (match.Groups[5].Success)
                    {
                        type = Var;
                    }
                    else if (match.Groups[6].Success)
                    {
                        type = Number;
                    }
                    else if (match.Groups[7].Success)
                    {
                        type = Invalid;
                    }
                    else
                    {
                        // We shouldn't get here
                        throw new InvalidOperationException("Regular exception failed in GetTokens");
                    }

                    // Yield the token
                    yield return new Token(match.Value, type);
                }

                // Look for the next match
                match = match.NextMatch();
            }
        }
    }

    /// <summary>
    /// Identifies the type of a token.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Left parenthesis
        /// </summary>
        LParen,

        /// <summary>
        /// Right parenthesis
        /// </summary>
        RParen,

        /// <summary>
        /// Operator symbol
        /// </summary>
        Oper,

        /// <summary>
        /// Variable
        /// </summary>
        Var,

        /// <summary>
        /// Double literal
        /// </summary>
        Number,

        /// <summary>
        /// Invalid token
        /// </summary>
        Invalid
    };

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string var);
    public delegate string Normalizer(string s);
    public delegate bool Validator(string s);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    [Serializable]
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    [Serializable]
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    [Serializable]
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }
}
