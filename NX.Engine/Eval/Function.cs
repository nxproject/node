///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (jegbhe@gmail.com) - All Rights Reserved
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

using System.Collections.Generic;

namespace NX.Engine
{
    public delegate object FunctionDelegate(Context ctx, object[] parameters);

    public abstract class Function
    {
        /// <summary>
        /// define the arguments of the dynamic function
        /// </summary>
        public Variables Arguments { get; protected set; }

        /// <summary>
        /// name of the function
        /// </summary>
        public string FN { get; protected set; }

        /// <summary>
        /// minimum number of allowed parameters (default = 0)
        /// </summary>
        public int MaxParameters { get; protected set; }

        /// <summary>
        /// maximum number of allowed parameters (default = 0)
        /// </summary>
        public int MinParameters { get; protected set; }

        /// <summary>
        /// 
        /// Evaluation engine
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="parameters"></param>
        /// <param name="tree"></param>
        /// <returns></returns>
        public abstract object Eval(Context ctx, object[] parameters, ParseTreeEvaluator tree);

        /// <summary>
        /// 
        /// Added By JG
        /// 
        /// </summary>
        public string Formatted
        {
            get
            {
                string sAns = this.FN + "(";

                for (int iLoop = 0; iLoop < this.MinParameters; iLoop++)
                {
                    if (iLoop != 0) sAns += ",";
                    sAns += System.Convert.ToChar(97 + iLoop);
                }

                sAns += ")";

                return sAns;
            }
        }

        /// <summary>
        /// 
        /// The descrition of the function
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// The description of the parameters
        /// 
        /// </summary>
        public List<ParameterDefinitionClass> Parameters { get; set; }
    }

    public class StaticFunction : Function
    {
        /// <summary>
        /// the actual function implementation
        /// </summary>
        public FunctionDelegate FunctionDelegate { get; private set; }

        public override object Eval(Context ctx, object[] parameters, ParseTreeEvaluator tree)
        {
            return FunctionDelegate(ctx, parameters);
        }

        public StaticFunction(string name,
                                FunctionDelegate function,
                                int minParameters,
                                int maxParameters,
                                string desc,
                                params ParameterDefinitionClass[] parameters)
        {
            FN = name;
            FunctionDelegate = function;
            MinParameters = minParameters;
            MaxParameters = maxParameters;
            Arguments = new Variables(null);
            Description = desc;
            Parameters = new List<ParameterDefinitionClass>(parameters);
        }
    }
}