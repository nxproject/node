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
///--------------------------------------------------------------------------------

/// Packet Manager Requirements
/// 
/// Install-Package Microsoft.CodeAnalysis.CSharp -Version 3.6.0
/// 

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Text;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Compiler support
    /// 
    /// </summary>
    public static class Compilers
    {
        /// <summary>
        /// 
        /// List of using entries to include
        /// 
        /// </summary>
        public static List<string> Using { get; set; } = new List<string>()
        {
            "NX.Shared",
            "NX.Engine",
            "NX.Engine.Hive",
            "NX.Engine.MinIO"
        };

        /// <summary>
        /// 
        /// Makes a block of using statements
        /// 
        /// </summary>
        /// <param name="fmt"></param>
        /// <returns></returns>
        private static string MakeUsing(string fmt)
        {
            string sAns = "";

            foreach(string sUsing in Using)
            {
                sAns += fmt.FormatString(sUsing);
            }

            return sAns;
        }

        #region C#
        /// <summary>
        /// 
        /// Compiles C# source code
        /// 
        /// </summary>
        /// <param name="name">The name of the file (Program.cs)</param>
        /// <param name="code">The UTF-8 byte array of the code</param>
        /// <returns></returns>
        public static byte[] CSharp(this string name, byte[] code)
        {
            // There must be code!
            if (code != null)
            {
                // Check to see if the byte array is already compiled
                if (code.Length >= 2 && code.SubArray(0, 2).FromBytes().IsSameValue("MZ"))
                {
                    // It is, all done
                }
                else
                {
                    // Add ourselves just in case user forgot
                    string sPrefix = MakeUsing("using {0};\r\n");

                    // The output stream for the compiler
                    using (MemoryStream c_Stream = new MemoryStream())
                    {
                        // Convert to text
                        var c_Code = SourceText.From(sPrefix + code.FromBytes());
                        // And we are using C# 7.3
                        var c_Syntax = CSharpParseOptions.Default.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest);
                        // Parse the code
                        var c_Tree = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseSyntaxTree(c_Code, c_Syntax);

                        // Add all of the libraries that are available
                        List<MetadataReference> c_Refs = new List<MetadataReference>()
                        {
                            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(EnvironmentClass).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(StoreClass).Assembly.Location)
                        };

                        // And tell the compiler what to output
                        var c_Options = new CSharpCompilationOptions(
                                    OutputKind.DynamicallyLinkedLibrary,
                                    optimizationLevel: OptimizationLevel.Release,
                                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default);

                        // Compile
                        var c_Compiled = CSharpCompilation.Create(name,
                            new[] { c_Tree },
                            references: c_Refs.ToArray(),
                            options: c_Options).Emit(c_Stream);

                        // Did we make it?
                        if (c_Compiled.Success)
                        {
                            // Rewind output stream
                            c_Stream.Seek(0, SeekOrigin.Begin);
                            // And get code
                            code = c_Stream.ToArray();
                        }
                        else
                        {
                            // No good
                            code = null;
                        }
                    }
                }
            }

            // ANd we return the compiled code or null if anything failed
            return code;
        }

        /// <summary>
        /// 
        /// Compiles C# source code
        /// 
        /// </summary>
        /// <param name="name">The name of the file (Program.cs)</param>
        /// <param name="code">The UTF-8 byte array of the code</param>
        /// <returns></returns>
        public static byte[] VB(this string name, byte[] code)
        {
            // There must be code!
            if (code != null)
            {
                // Check to see if the byte array is already compiled
                if (code.Length >= 2 && code.SubArray(0, 2).FromBytes().IsSameValue("MZ"))
                {
                    // It is, all done
                }
                else
                {
                    // Add ourselves just in case user forgot
                    string sPrefix = MakeUsing("Imports {0}\r\n");

                    // The output stream for the compiler
                    using (MemoryStream c_Stream = new MemoryStream())
                    {
                        // Convert to text
                        var c_Code = SourceText.From(sPrefix + code.FromBytes());
                        // And we are using C# 7.3
                        var c_Syntax = VisualBasicParseOptions.Default.WithLanguageVersion(Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.Latest);
                        // Parse the code
                        var c_Tree = Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory.ParseSyntaxTree(c_Code, c_Syntax);

                        // Add all of the libraries that are available
                        List<MetadataReference> c_Refs = new List<MetadataReference>()
                        {
                            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(EnvironmentClass).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(StoreClass).Assembly.Location)
                        };

                        // And tell the compiler what to output
                        var c_Options = new VisualBasicCompilationOptions(
                                    OutputKind.DynamicallyLinkedLibrary,
                                    optimizationLevel: OptimizationLevel.Release,
                                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default);

                        // Compile
                        var c_Compiled = VisualBasicCompilation.Create(name,
                            new[] { c_Tree },
                            references: c_Refs.ToArray(),
                            options: c_Options).Emit(c_Stream);

                        // Did we make it?
                        if (c_Compiled.Success)
                        {
                            // Rewind output stream
                            c_Stream.Seek(0, SeekOrigin.Begin);
                            // And get code
                            code = c_Stream.ToArray();
                        }
                        else
                        {
                            // No good
                            code = null;
                        }
                    }
                }
            }

            // ANd we return the compiled code or null if anything failed
            return code;
        }
        #endregion
    }
}