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
///--------------------------------------------------------------------------------

using Proc.NginX;
using NX.Engine;
using NX.Shared;

namespace Proc.Traefik
{
    /// <summary>
    /// 
    /// Initializes the traefik manager
    /// 
    /// </summary>
    public class Init : FNClass
    {
        public override void Initialize(EnvironmentClass env)
        {
            // Make the bumble bee
            ManagerClass c_Mgr = env.Globals.Get<ManagerClass>();

            // Need NginX
            NginX.ManagerClass c_NginX = env.Globals.Get<NginX.ManagerClass>();

            // Hook into availability
            c_NginX.AvailabilityChanged += delegate (bool isavailable)
            {
                // According to availability
                if (isavailable)
                {
                    // Add ourselves
                    using (CommandClass c_Cmd = c_Mgr.New())
                    {
                        // Setup
                        c_Cmd.Command = "site.add";
                        c_Cmd.IP = c_NginX.Location.RemoveProtocol();

                        c_Cmd.Send();
                    }
                }
                else
                {
                    // Remove ourselves
                    using (CommandClass c_Cmd = c_Mgr.New())
                    {
                        // Setup
                        c_Cmd.Command = "site.remove";

                        c_Cmd.Send();
                    }
                }
            };

            base.Initialize(env);
        }
    }
}
