using System;
using System.Collections.Generic;
using System.Text;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// Parameters for callback
    /// 
    /// </summary>
    public class ExprCBParams : ChildOfClass<Context>
    {
        #region Constructor
        public ExprCBParams(Context ctx, string prefix, string field, string value, Modes mode)
            : base(ctx)
        {
            //
            this.Prefix = prefix;
            this.Field = field;
            this.Value = value;
            this.Mode = mode;
        }
        #endregion

        #region Enums
        public enum Modes
        {
            Get,
            Set,

            Map
        }
        #endregion

        #region Properties
        public string Prefix { get; internal set; }
        public string Field { get; internal set; }
        public string Value { get; internal set; }
        public Modes Mode { get; internal set; }
        #endregion
    }
}