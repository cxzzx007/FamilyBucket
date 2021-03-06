﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Bucket.Values
{
    public class BuriedInformationInput
    {
        /// <summary>
        /// 方法名称 (Open)
        /// </summary>
        public string FuncName { get; set; }
        /// <summary>
        /// 接口方法地址 (没有可以为null)
        /// </summary>
        public string ApiUri { get; set; }
        /// <summary>
        /// 接口方法类型    0普通方法 (默认)    1 Post    2 Get
        /// </summary>
        public int ApiType { get; set; }
        /// <summary>
        /// 对方接口调用是否成功  0成功(默认)     1失败    非接口调用 -1
        /// </summary>
        public int CalledResult { get; set; }
        /// <summary>
        /// 对方接口调用描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 当前自身业务逻辑是否成功    0成功(默认) 1失败
        /// </summary>
        public int BussinessSuccess { get; set; }
        /// <summary>
        /// 业务逻辑描述
        /// </summary>
        public string BussinessDesc { get; set; }
        /// <summary>
        /// 触发警告 0短信触发  1 不触发(默认) 2邮件触发  -1. 短信+邮件触发
        /// </summary>
        public int TrgAlarm { get; set; }
        /// <summary>
        /// 当前接口方法入参 默认为null
        /// </summary>
        public string InputParams { get; set; }
        /// <summary>
        /// 当前接口方法出参 默认为null
        /// </summary>
        public string OutputParams { get; set; }
        /// <summary>
        /// 当前Header头参数 默认为null
        /// </summary>
        public string HeaderParams { get; set; }
        /// <summary>
        /// 错误码   默认为null和 TrgAlarm 配合使用
        /// </summary>
        public string ErrorCode { get; set; }
    }
}
