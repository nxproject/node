///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2024 Jose E. Gonzalez (nx.jegbhe@gmail.com) - All Rights Reserved
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
using System.Text;
using System.Xml.Serialization;

using NX.Shared;

namespace NX.Engine
{
    #region ParseTree
    [Serializable]
    public class ParseErrors : List<ParseError>
    {
    }

    [Serializable]
    public class ParseError
    {
        private string message;
        private int code;
        private int line;
        private int col;
        private int pos;
        private int length;

        public int Code { get { return code; } }
        public int Line { get { return line; } }
        public int Column { get { return col; } }
        public int Position { get { return pos; } }
        public int Length { get { return length; } }
        public string Message { get { return message; } }

        // just for the sake of serialization
        public ParseError()
        {
        }

        public ParseError(string message, int code, ParseNode node) : this(message, code,  0, node.Token.StartPos, node.Token.StartPos, node.Token.Length)
        {
        }

        public ParseError(string message, int code, int line, int col, int pos, int length)
        {
            this.message = message;
            this.code = code;
            this.line = line;
            this.col = col;
            this.pos = pos;
            this.length = length;
        }

        public override string ToString()
        {
            return "[{0}] {1}".FormatString(this.Column, this.Message);
        }
    }

    // rootlevel of the node tree
    [Serializable]
    public partial class ParseTree : ParseNode
    {
        public ParseErrors Errors;

        public List<Token> Skipped;

        public ParseTree() : base(new Token(), "ParseTree")
        {
            Token.Type = TokenType.Start;
            Token.Text = "Root";
            Errors = new ParseErrors();
        }

        public string PrintTree()
        {
            StringBuilder sb = new StringBuilder();
            int indent = 0;
            PrintNode(sb, this, indent);
            return sb.ToString();
        }

        private void PrintNode(StringBuilder sb, ParseNode node, int indent)
        {
            
            string space = "".PadLeft(indent, ' ');

            sb.Append(space);
            sb.AppendLine(node.Text);

            foreach (ParseNode n in node.Nodes)
                PrintNode(sb, n, indent + 2);
        }
        
        /// <summary>
        /// this is the entry point for executing and evaluating the parse tree.
        /// </summary>
        /// <param name="paramlist">additional optional input parameters</param>
        /// <returns>the output of the evaluation function</returns>
        public object Evaluate(Context ctx, params object[] paramlist)
        {
            return Nodes[0].Evaluate(ctx, this, paramlist);
        }
    }

    [Serializable]
    [XmlInclude(typeof(ParseTree))]
    public partial class ParseNode
    {
        protected string text;
        protected List<ParseNode> nodes;
        
        public List<ParseNode> Nodes { get {return nodes;} }
        
        [XmlIgnore] // avoid circular references when serializing
        public ParseNode Parent;
        public Token Token; // the token/rule

        [XmlIgnore] // skip redundant text (is part of Token)
        public string Text { // text to display in parse tree 
            get { return text;} 
            set { text = value; }
        } 

        public virtual ParseNode CreateNode(Token token, string text)
        {
            ParseNode node = new ParseNode(token, text);
            node.Parent = this;
            return node;
        }

        protected ParseNode(Token token, string text)
        {
            this.Token = token;
            this.text = text;
            this.nodes = new List<ParseNode>();
        }

        protected object GetValue(Context ctx, ParseTree tree, TokenType type, int index)
        {
            return GetValue(ctx, tree, type, ref index);
        }

        protected object GetValue(Context ctx, ParseTree tree, TokenType type, ref int index)
        {
            object o = null;
            if (index < 0) return o;

            // left to right
            foreach (ParseNode node in nodes)
            {
                if (node.Token.Type == type)
                {
                    index--;
                    if (index < 0)
                    {
                        o = node.Evaluate(ctx, tree);
                        break;
                    }
                }
            }
            return o;
        }

        /// <summary>
        /// this implements the evaluation functionality, cannot be used directly
        /// </summary>
        /// <param name="tree">the parsetree itself</param>
        /// <param name="paramlist">optional input parameters</param>
        /// <returns>a partial result of the evaluation</returns>
        internal object Evaluate(Context ctx, ParseTree tree, params object[] paramlist)
        {
            object Value = null;

            switch (Token.Type)
            {
                case TokenType.Start:
                    Value = EvalStart(ctx, tree, paramlist);
                    break;
                case TokenType.Function:
                    Value = EvalFunction(ctx, tree, paramlist);
                    break;
                case TokenType.PrimaryExpression:
                    Value = EvalPrimaryExpression(ctx, tree, paramlist);
                    break;
                case TokenType.ParenthesizedExpression:
                    Value = EvalParenthesizedExpression(ctx, tree, paramlist);
                    break;
                case TokenType.UnaryExpression:
                    Value = EvalUnaryExpression(ctx, tree, paramlist);
                    break;
                case TokenType.PowerExpression:
                    Value = EvalPowerExpression(ctx, tree, paramlist);
                    break;
                case TokenType.MultiplicativeExpression:
                    Value = EvalMultiplicativeExpression(ctx, tree, paramlist);
                    break;
                case TokenType.AdditiveExpression:
                    Value = EvalAdditiveExpression(ctx, tree, paramlist);
                    break;
                case TokenType.ConcatEpression:
                    Value = EvalConcatEpression(ctx, tree, paramlist);
                    break;
                case TokenType.RelationalExpression:
                    Value = EvalRelationalExpression(ctx, tree, paramlist);
                    break;
                case TokenType.EqualityExpression:
                    Value = EvalEqualityExpression(ctx, tree, paramlist);
                    break;
                case TokenType.ConditionalAndExpression:
                    Value = EvalConditionalAndExpression(ctx, tree, paramlist);
                    break;
                case TokenType.ConditionalOrExpression:
                    Value = EvalConditionalOrExpression(ctx, tree, paramlist);
                    break;
                case TokenType.AssignmentExpression:
                    Value = EvalAssignmentExpression(ctx, tree, paramlist);
                    break;
                case TokenType.Expression:
                    Value = EvalExpression(ctx, tree, paramlist);
                    break;
                case TokenType.Params:
                    Value = EvalParams(ctx, tree, paramlist);
                    break;
                case TokenType.Literal:
                    Value = EvalLiteral(ctx, tree, paramlist);
                    break;
                case TokenType.IntegerLiteral:
                    Value = EvalIntegerLiteral(ctx, tree, paramlist);
                    break;
                case TokenType.RealLiteral:
                    Value = EvalRealLiteral(ctx, tree, paramlist);
                    break;
                case TokenType.StringLiteral:
                    Value = EvalStringLiteral(ctx, tree, paramlist);
                    break;
                case TokenType.Variable:
                    Value = EvalVariable(ctx, tree, paramlist);
                    break;

                default:
                    Value = Token.Text;
                    break;
            }
            return Value;
        }

        protected virtual object EvalStart(Context ctx, ParseTree tree, params object[] paramlist)
        {
            return "Could not interpret input; no semantics implemented.";
        }

        protected virtual object EvalFunction(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalPrimaryExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalParenthesizedExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalUnaryExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalPowerExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalMultiplicativeExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalAdditiveExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalConcatEpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalRelationalExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalEqualityExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalConditionalAndExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalConditionalOrExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalAssignmentExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalExpression(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalParams(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalLiteral(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalIntegerLiteral(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalRealLiteral(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalStringLiteral(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }

        protected virtual object EvalVariable(Context ctx, ParseTree tree, params object[] paramlist)
        {
            throw new NotImplementedException();
        }


    }
    
    #endregion ParseTree
}