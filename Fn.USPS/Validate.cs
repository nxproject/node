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
///--------------------------------------------------------------------------------

using System;
using System.Xml;

using NX.Engine;
using NX.Shared;

namespace Fn.USPS
{
    /// <summary>
    /// 
    /// Validates a street address 
    /// 
    /// </summary>
    public class Validate : FNClass
    {
        public override StoreClass Do(HTTPCallClass call, StoreClass values)
        {
            using (AddressBlock c_Addr = new AddressBlock(values))
            {
                if (!c_Addr.LastError.HasValue())
                {
                    try
                    {
                        // Get interface
                        ManagerClass c_IF = call.Env.Globals.Get<ManagerClass>();
                        // DDo we have one?
                        if (c_IF != null)
                        {
                            string validateUrl = "?API=Verify&XML=<AddressValidateRequest USERID=\"{0}\"><Address ID=\"{1}\"><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Zip5>{6}</Zip5><Zip4>{7}</Zip4></Address></AddressValidateRequest>";
                            string url = c_IF.MakeURL(validateUrl, c_Addr);

                            string addressxml = url.URLGet().FromBytes();
                            if (addressxml.Contains("<Error>"))
                            {
                                int idx1 = addressxml.IndexOf("<Description>") + 13;
                                int idx2 = addressxml.IndexOf("</Description>");
                                int l = addressxml.Length;
                                string errDesc = addressxml.Substring(idx1, idx2 - idx1);

                                c_Addr.LastError = errDesc;
                            }
                            else
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(addressxml);

                                XmlNode element = doc.SelectSingleNode("/AddressValidateResponse/Address");
                                if (element != null)
                                {
                                    c_Addr.LoadXml(element);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        c_Addr.LastError = "Error: " + ex.Message;
                    }
                }

                return c_Addr.Parent;
            }
        }
    }
}