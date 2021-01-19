///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020 Jose E. Gonzalez (jegbhe@gmail.com) - All Rights Reserved
/// 
/// This work is covered by GPL v3 as defined in https://www.gnu.org/licenses/gpl-3.0.en.html
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Chore context storage
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ContextStoreClass<T>
    {
        #region Constructor
        public ContextStoreClass()
        { }
        #endregion

        #region Indexer
        public T this[string key]
        {
            get
            {
                T c_Ans = default(T);

                key = this.SanitizeName(key).IfEmpty(this.Default).IfEmpty("passed");

                if (this.Values.ContainsKey(key))
                {
                    c_Ans = this.Values[key];
                }
                else
                {
                    this.Values[key] = c_Ans;
                }

                return c_Ans;
            }
            set
            {
                key = this.SanitizeName(key).IfEmpty(this.Default).IfEmpty("passed");

                this.Values[key] = value;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The items being stored
        /// 
        /// </summary>
        public List<string> Keys { get { return new List<string>(this.Values.Keys); } }

        /// <summary>
        /// 
        /// The values
        /// 
        /// </summary>
        private NamedListClass<T> Values { get; set; } = new NamedListClass<T>();

        /// <summary>
        /// 
        /// The default entry
        /// 
        /// </summary>
        public string Default { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// Assures a clean name
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string SanitizeName(string name)
        {
            return Regex.Replace(name.IfEmpty(), @"[^a-zA-Z0-9]", "").ToLower();
        }

        /// <summary>
        /// 
        /// Sets the default entry
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void Use(string key)
        {
            this.Default = key;
        }

        /// <summary>
        /// 
        /// Cheks to see if entry is defined
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Has(string key)
        {
            return this.Values.Contains(key);
        }

        /// <summary>
        /// 
        /// Removes all entries
        /// 
        /// </summary>
        public void Clear()
        {
            this.Values = new NamedListClass<T>();
        }
        #endregion
    }
}