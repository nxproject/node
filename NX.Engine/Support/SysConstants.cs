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

namespace NX.Engine
{
    public static class SysConstantClass
    {
        /// <summary>
        /// 
        /// Date formats that are O/S independent
        /// 
        /// </summary>
        public const string DateFormat = "yyy-MM-dd HH:mm:ss";
        public const string ShortDateFormat = "yyy-MM-dd HH:mm";
        public const string DateOnlyFormat = "yyy-MM-dd";
        public const string DateFormatSortable = "yyyMMdd";
        public const string USDateFormat = "MM/dd/yyyy";
        public const string USDateFormatShort = "MM/dd/yy";
        public const string DateFormatShort = "yyy-MM-dd";
        public const string DateFormatTS = "MM/dd/yyyy hh:mm tt";
        public const string TimeFormatTS = "hh:mm tt";
        public const string Tick = "yyyMMddHHmm";
        public const string DateFormatTSShort = "MM/dd/yy hh:mmtt";

        public const string DockerVersion = "yyy-MM-dd-hh-mm";
    }
}