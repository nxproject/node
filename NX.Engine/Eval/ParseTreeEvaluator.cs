///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020 Jose E. Gonzalez (jegbhe@gmail.com) - All Rights Reserved
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// 
/// NB: Work derived from "a Tiny Parser Generator v1.2" by Herre Kuijpers
/// found at https://www.codeproject.com/Articles/28294/a-Tiny-Parser-Generator-v1-2
/// under the CPOL license found at https://www.codeproject.com/info/cpol10.aspx
/// 
///--------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;

namespace NX.Engine
{
    #region ParseTreeEvaluator
    // rootlevel of the node tree
    [Serializable]
    public class ParseTreeEvaluator : ParseTree, IDisposable
    {

        //public Functions Functions { get; private set; }
        public Context Context { get; private set; }

        public ParseTreeEvaluator() : base()
        {
        }

        public ParseTreeEvaluator(Context context)
            : base()
        {
            Context = context;
        }

        /// <summary>
        /// required to override this function from base otherwise the parsetree will consist of incorrect types of nodes
        /// </summary>
        /// <param name="token"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public override ParseNode CreateNode(Token token, string text)
        {
            ParseTreeEvaluator node = new ParseTreeEvaluator();
            node.Token = token;
            node.text = text;
            node.Parent = this;
            return node;
        }

        protected override object EvalStart(Context ctx, ParseTree tree, params object[] paramlist)
        {
            return this.GetValue(ctx, tree, TokenType.Expression, 0);
        }

        protected override object EvalUnaryExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            TokenType type = this.nodes[0].Token.Type;
            if (type == TokenType.PrimaryExpression)
                return this.GetValue(ctx, tree, TokenType.PrimaryExpression, 0);

            if (type == TokenType.MINUS)
            {
                object val = this.GetValue(ctx, tree, TokenType.UnaryExpression, 0);
                if (val is double)
                    return -((double)val);

                tree.Errors.Add(new ParseError("Illegal UnaryExpression format, cannot interpret minus " + val.ToString(), 1095, this));
                return null;
            }
            else if (type == TokenType.PLUS)
            {
                object val = this.GetValue(ctx, tree, TokenType.UnaryExpression, 0);
                return val;
            }
            else if (type == TokenType.NOT)
            {
                object val = XCVT.ToBoolean(this.GetValue(ctx, tree, TokenType.UnaryExpression, 0));
                if (val is bool)
                    return !((bool)val);

                tree.Errors.Add(new ParseError("Illegal UnaryExpression format, cannot interpret negate " + val.ToString(), 1098, this));
                return null;
            }

            Errors.Add(new ParseError("Illegal UnaryExpression format", 1099, this));
            return null;

        }

        protected override object EvalParams(Context ctx, ParseTree tree, params object[] paramlist)
        {
            List<object> parameters = new List<object>();
            for (int i = 0; i < nodes.Count; i += 2)
            {
                if (nodes[i].Token.Type == TokenType.Expression)
                {
                    object val = nodes[i].Evaluate(ctx, tree, paramlist);
                    parameters.Add(val);
                }
            }
            return parameters;
        }

        protected override object EvalFunction(Context ctx, ParseTree tree, params object[] paramlist)
        {
            ParseNode funcNode = this.nodes[0];
            ParseNode paramNode = this.nodes[2];

            ParseTreeEvaluator root = tree as ParseTreeEvaluator;
            if (root == null)
            {
                tree.Errors.Add(new ParseError("Invalid parser used", 1040, this));
                return null;
            }
            if (root.Context == null)
            {
                tree.Errors.Add(new ParseError("No context defined", 1041, this));
                return null;
            }
            string key = funcNode.Token.Text.ToLowerInvariant();

            if (!root.Context.Functions.ContainsKey(key))
            {
                // JGN - VIRTUAL FN COULD GO HERE
                tree.Errors.Add(new ParseError("Function not defined: " + funcNode.Token.Text + "()", 1042, this));
                return null;
            }

            // retrieves the function from declared functions
            Function func = root.Context.Functions[key];

            // evaluate the function parameters
            object[] parameters = new object[0];
            if (paramNode.Token.Type == TokenType.Params)
                parameters = (paramNode.Evaluate(ctx, tree, paramlist) as List<object>).ToArray();
            if (parameters.Length < func.MinParameters)
            {
                tree.Errors.Add(new ParseError("At least " + func.MinParameters.ToString() + " parameter(s) expected", 1043, this));
                return null; // illegal number of parameters
            }
            else if (parameters.Length > func.MaxParameters)
            {
                tree.Errors.Add(new ParseError("No more than " + func.MaxParameters.ToString() + " parameter(s) expected", 1044, this));
                return null; // illegal number of parameters
            }

            return func.Eval(ctx, parameters, root);
        }

        protected override object EvalVariable(Context ctx, ParseTree tree, params object[] paramlist)
        {
            ParseTreeEvaluator root = tree as ParseTreeEvaluator;
            if (root == null)
            {
                tree.Errors.Add(new ParseError("Invalid parser used", 1040, this));
                return null;
            }
            if (root.Context == null)
            {
                tree.Errors.Add(new ParseError("No context defined", 1041, this));
                return null;
            }

            string key = this.nodes[0].Token.Text;
            // next check if the variable was declared as a global variable
            if (root.Context.Globals != null)
                return root.Context.Globals[key];

            //variable not found
            tree.Errors.Add(new ParseError("Variable not defined: " + key, 1039, this));
            return null;
        }

        protected override object EvalPrimaryExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            TokenType type = this.nodes[0].Token.Type;
            if (type == TokenType.Function)
                return this.GetValue(ctx, tree, TokenType.Function, 0);
            else if (type == TokenType.Literal)
                return this.GetValue(ctx, tree, TokenType.Literal, 0);
            else if (type == TokenType.ParenthesizedExpression)
                return this.GetValue(ctx, tree, TokenType.ParenthesizedExpression, 0);
            else if (type == TokenType.Variable)
                return this.GetValue(ctx, tree, TokenType.Variable, 0);

            tree.Errors.Add(new ParseError("Illegal EvalPrimaryExpression format", 1097, this));
            return null;
        }

        protected override object EvalParenthesizedExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            return this.GetValue(ctx, tree, TokenType.Expression, 0);
        }

        protected override object EvalPowerExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            object result = this.GetValue(ctx, tree, TokenType.UnaryExpression, 0);

            // IMPORTANT: scanning and calculating the power is done from Left to Right.
            // this is conform the Excel evaluation of power, but not conform strict mathematical guidelines
            // this means that a^b^c evaluates to (a^b)^c  (Excel uses the same kind of evaluation)
            // stricly mathematical speaking a^b^c should evaluate to a^(b^c) (therefore calculating the powers from Right to Left)
            for (int i = 1; i < nodes.Count; i += 2)
            {
                Token token = nodes[i].Token;
                object val = nodes[i + 1].Evaluate(ctx, tree, paramlist);
                if (token.Type == TokenType.POWER)
                    result = Math.Pow(XCVT.ToDouble(result), XCVT.ToDouble(val));
            }

            return result;
        }

        protected override object EvalMultiplicativeExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            object result = this.GetValue(ctx, tree, TokenType.PowerExpression, 0);
            for (int i = 1; i < nodes.Count; i += 2)
            {
                Token token = nodes[i].Token;
                object val = nodes[i + 1].Evaluate(ctx, tree, paramlist);
                if (token.Type == TokenType.ASTERIKS)
                    result = XCVT.ToDouble(result) * XCVT.ToDouble(val);
                else if (token.Type == TokenType.SLASH)
                    result = XCVT.ToDouble(result) / XCVT.ToDouble(val);
                else if (token.Type == TokenType.PERCENT)
                    result = XCVT.ToDouble(result) % XCVT.ToDouble(val);
            }

            return result;
        }

        protected override object EvalAdditiveExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            object result = this.GetValue(ctx, tree, TokenType.MultiplicativeExpression, 0);
            for (int i = 1; i < nodes.Count; i += 2)
            {
                Token token = nodes[i].Token;
                object val = nodes[i + 1].Evaluate(ctx, tree, paramlist);
                if (token.Type == TokenType.PLUS)
                    result = XCVT.ToDouble(result) + XCVT.ToDouble(val);
                else if (token.Type == TokenType.MINUS)
                    result = XCVT.ToDouble(result) - XCVT.ToDouble(val);
            }

            return result;
        }

        protected override object EvalConcatEpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            object result = this.GetValue(ctx, tree, TokenType.AdditiveExpression, 0);
            for (int i = 1; i < nodes.Count; i += 2)
            {
                Token token = nodes[i].Token;
                object val = nodes[i + 1].Evaluate(ctx, tree, paramlist);
                if (token.Type == TokenType.AMP)
                    result = XCVT.ToString(result) + XCVT.ToString(val);
            }
            return result;
        }

        protected override object EvalRelationalExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            object result = this.GetValue(ctx, tree, TokenType.ConcatEpression, 0);
            for (int i = 1; i < nodes.Count; i += 2)
            {
                Token token = nodes[i].Token;
                object val = nodes[i + 1].Evaluate(ctx, tree, paramlist);

                // compare as numbers
                if (result is double || val is double)
                {
                    if (token.Type == TokenType.LESSTHAN)
                        result = XCVT.ToDouble(result) < XCVT.ToDouble(val);
                    else if (token.Type == TokenType.LESSEQUAL)
                        result = XCVT.ToDouble(result) <= XCVT.ToDouble(val);
                    else if (token.Type == TokenType.GREATERTHAN)
                        result = XCVT.ToDouble(result) > XCVT.ToDouble(val);
                    else if (token.Type == TokenType.GREATEREQUAL)
                        result = XCVT.ToDouble(result) >= XCVT.ToDouble(val);
                }
                else // compare as strings
                {
                    int comp = string.Compare(XCVT.ToString(result), XCVT.ToString(val));
                    if (token.Type == TokenType.LESSTHAN)
                        result = comp < 0;
                    else if (token.Type == TokenType.LESSEQUAL)
                        result = comp <= 0;
                    else if (token.Type == TokenType.GREATERTHAN)
                        result = comp > 0;
                    else if (token.Type == TokenType.GREATEREQUAL)
                        result = comp >= 0;
                }

            }
            return result;
        }

        protected override object EvalEqualityExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            object result = this.GetValue(ctx, tree, TokenType.RelationalExpression, 0);
            if (result != null) result = result.ToString();

            for (int i = 1; i < nodes.Count; i += 2)
            {
                Token token = nodes[i].Token;
                object val = nodes[i + 1].Evaluate(ctx, tree, paramlist);
                if (val != null) val = val.ToString();

                if (token.Type == TokenType.EQUAL)
                    result = object.Equals(result, val);
                else if (token.Type == TokenType.NOTEQUAL)
                    result = !object.Equals(result, val);
            }
            return result;
        }

        protected override object EvalConditionalAndExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            object result = this.GetValue(ctx, tree, TokenType.EqualityExpression, 0);
            for (int i = 1; i < nodes.Count; i += 2)
            {
                Token token = nodes[i].Token;
                object val = nodes[i + 1].Evaluate(ctx, tree, paramlist);
                if (token.Type == TokenType.AMPAMP)
                    result = XCVT.ToBoolean(result) && XCVT.ToBoolean(val);
            }
            return result;
        }

        protected override object EvalConditionalOrExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            object result = this.GetValue(ctx, tree, TokenType.ConditionalAndExpression, 0);
            for (int i = 1; i < nodes.Count; i += 2)
            {
                Token token = nodes[i].Token;
                object val = nodes[i + 1].Evaluate(ctx, tree, paramlist);
                if (token.Type == TokenType.PIPEPIPE)
                    result = XCVT.ToBoolean(result) || XCVT.ToBoolean(val);
            }
            return result;
        }

        protected override object EvalAssignmentExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            object result = this.GetValue(ctx, tree, TokenType.ConditionalOrExpression, 0);
            if (nodes.Count >= 5 && result is bool
                && nodes[1].Token.Type == TokenType.QUESTIONMARK
                && nodes[3].Token.Type == TokenType.COLON)
            {
                if (XCVT.ToBoolean(result))
                    result = nodes[2].Evaluate(ctx, tree, paramlist); // return 1st argument
                else
                    result = nodes[4].Evaluate(ctx, tree, paramlist); // return 2nd argumen
            }
            return result;
        }

        protected override object EvalExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            // if only left hand side available, this is not an assignment, simple evaluate expression
            if (nodes.Count == 1)
                return this.GetValue(ctx, tree, TokenType.AssignmentExpression, 0); // return the result

            if (nodes.Count != 3)
            {
                tree.Errors.Add(new ParseError("Illegal EvalExpression format", 1092, this));
                return null;
            }

            // ok, this is an assignment so declare the function or variable
            // assignment only allowed to function or to a variable
            ParseNode v = GetFunctionOrVariable(nodes[0]);
            if (v == null)
            {
                tree.Errors.Add(new ParseError("Can only assign to function or variable", 1020, this));
                return null;
            }

            ParseTreeEvaluator root = tree as ParseTreeEvaluator;
            if (root == null)
            {
                tree.Errors.Add(new ParseError("Invalid parser used", 1040, this));
                return null;
            }

            if (root.Context == null)
            {
                tree.Errors.Add(new ParseError("No context defined", 1041, this));
                return null;
            }

            if (v.Token.Type == TokenType.VARIABLE)
            {

                // simply overwrite any previous defnition
                string key = v.Token.Text;
                root.Context.Globals[key] = this.GetValue(ctx, tree, TokenType.AssignmentExpression, 1);
                return null;
            }
            else if (v.Token.Type == TokenType.Function)
            {
                tree.Errors.Add(new ParseError("Built in functions cannot be overwritten", 1050, this));
                return null;
            }



            // in an assignment, dont return any result (basically void)
            return null;
        }

        // helper function to find access the function or variable
        private ParseNode GetFunctionOrVariable(ParseNode n)
        {
            // found the right node, exit
            if (n.Token.Type == TokenType.Function || n.Token.Type == TokenType.VARIABLE)
                return n;

            if (n.Nodes.Count == 1) // search lower branch (left side only, may not contain other node branches)
                return GetFunctionOrVariable(n.Nodes[0]);

            // function or variable not found in branch
            return null;
        }

        protected override object EvalLiteral(Context ctx, ParseTree tree, params object[] paramlist)
        {
            TokenType type = this.nodes[0].Token.Type;
            if (type == TokenType.StringLiteral)
                return this.GetValue(ctx, tree, TokenType.StringLiteral, 0);
            else if (type == TokenType.IntegerLiteral)
                return this.GetValue(ctx, tree, TokenType.IntegerLiteral, 0);
            else if (type == TokenType.RealLiteral)
                return this.GetValue(ctx, tree, TokenType.RealLiteral, 0);
            else if (type == TokenType.BOOLEANLITERAL)
            {
                string val = this.GetValue(ctx, tree, TokenType.BOOLEANLITERAL, 0).ToString();
                return XCVT.ToBoolean(val);
            }

            tree.Errors.Add(new ParseError("illegal Literal format", 1003, this));
            return null;
        }

        protected override object EvalIntegerLiteral(Context ctx, ParseTree tree, params object[] paramlist)
        {
            if (this.GetValue(ctx, tree, TokenType.DECIMALINTEGERLITERAL, 0) != null)
                return XCVT.ToDouble(this.GetValue(ctx, tree, TokenType.DECIMALINTEGERLITERAL, 0));
            if (this.GetValue(ctx, tree, TokenType.HEXINTEGERLITERAL, 0) != null)
            {
                string hex = this.GetValue(ctx, tree, TokenType.HEXINTEGERLITERAL, 0).ToString();
                return XCVT.ToDouble(XCVT.ToInt64(hex.Substring(2, hex.Length - 2), 16));
            }

            tree.Errors.Add(new ParseError("illegal IntegerLiteral format", 1002, this));
            return null;
        }

        protected override object EvalRealLiteral(Context ctx, ParseTree tree, params object[] paramlist)
        {
            if (this.GetValue(ctx, tree, TokenType.REALLITERAL, 0) != null)
            {
                string val = string.Format(CultureInfo.InvariantCulture, "{0}", this.GetValue(ctx, tree, TokenType.REALLITERAL, 0));
                return double.Parse(val, CultureInfo.InvariantCulture);
            }
            tree.Errors.Add(new ParseError("illegal RealLiteral format", 1001, this));
            return null;
        }

        protected override object EvalStringLiteral(Context ctx, ParseTree tree, params object[] paramlist)
        {
            if (this.GetValue(ctx, tree, TokenType.STRINGLITERAL, 0) != null)
            {
                string r = (string)this.GetValue(ctx, tree, TokenType.STRINGLITERAL, 0);
                r = r.Substring(1, r.Length - 2); // strip quotes
                return r;
            }

            tree.Errors.Add(new ParseError("illegal StringLiteral format", 1000, this));
            return string.Empty;
        }


        public void Dispose()
        {
        }

    }

    #endregion ParseTree
}