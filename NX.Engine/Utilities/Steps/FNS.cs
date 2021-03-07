///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (nxoffice2021@gmail.com) - All Rights Reserved
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
///--------------------------------------------------------------------------------

using System.Text;
using System.Collections.Generic;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// This class handles the routing of function calls
    /// to a pre-defined set of functions.
    /// 
    /// </summary>
    public class FNSClass : ExtManagerClass<FNClass>
    {
        #region Constructor
        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="env">The current environment</param>
        public FNSClass(EnvironmentClass env)
            : base(env)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Returns the function code for a given name
        /// 
        /// </summary>
        /// <param name="name">The function name</param>
        /// <returns>The function code (if any)</returns>
        public FNClass GetFN(string name)
        {
            return (FNClass)this.Get(name);
        }

        /// <summary>
        /// 
        /// Generate MD file
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void GenerateMD(string path)
        {
            //
            StringBuilder c_Buffer = new StringBuilder();

            // Get the list
            List<string> c_Fns = this.Names;
            // Sort
            c_Fns.Sort();

            // :oop thru
            foreach (string sFN in c_Fns)
            {
                FNClass c_FN = this.GetFN(sFN);

                BaseDescriptionClass c_Desc = c_FN.Description;
                if (c_Desc != null)
                {
                    string sLine = "|" + c_FN.Name + "|" + c_Desc.Description + "|";
                    // If none. ue empty
                    if (c_Desc.Parameters.Count == 0)
                    {
                        c_Buffer.AppendLine(sLine + " |");
                    }
                    else
                    {
                        bool bShowLine = true;

                        foreach (string sParam in c_Desc.Parameters.Keys)
                        {
                            ParamDefinitionClass c_P = c_Desc.Parameters[sParam];
                            //
                            string sP = sParam + "|" + c_P.Description + "|" + (c_P.Type == ParamDefinitionClass.Types.Required ? c_P.Type.ToString() : "") + "|";

                            if (bShowLine)
                            {
                                c_Buffer.AppendLine(sLine + sP);
                                bShowLine = false;
                            }
                            else
                            {
                                c_Buffer.AppendLine("| | |" + sP);
                            }

                        }
                    }
                }

                else
                {
                    c_Buffer.AppendLine("|" + c_FN.Name + "|MISSING DESCRIPTION|");
                }
            }

            // Make header
            string sHeader = "|Command|Description|Parameter|Use| |";
            string sDelim = "|-|-|-|-|-|";

            // Make
            string sText = sHeader + "\n" + sDelim + "\n" + c_Buffer.ToString();

            // Read template
            string sTemplate = (path + ".template").ReadFile();
            // Replace
            sTemplate = sTemplate.Replace("{{fns}}", sText);
            // Write
            path.WriteFile(sTemplate);
        }
        #endregion
    }
}