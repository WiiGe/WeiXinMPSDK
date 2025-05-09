﻿#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Jeffrey Su & Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/JeffreySu/WeiXinMPSDK/blob/master/license.md

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc
 
    文件名：TenPayV3Results.cs
    文件功能描述：微信支付V3返回结果
    
    
    创建标识：Senparc - 20150211
    
    修改标识：Senparc - 20150303
    修改描述：整理接口

    修改标识：Senparc - 20161202
    修改描述：v14.3.109 命名空间由Senparc.Weixin.MP.AdvancedAPIs改为Senparc.Weixin.MP.TenPayLibV3
    
    修改标识：Senparc - 20161205
    修改描述：v14.3.110 完善XML转换信息

    修改标识：Senparc - 20161206
    修改描述：v14.3.111 处理UnifiedorderResult数据处理问题

    修改标识：Senparc - 20161226
    修改描述：v14.3.112 增加OrderQueryResult,CloseOrderResult,RefundQuery,ShortUrlResult,ReverseResult,MicropayResult

    修改标识：Senparc - 20170215
    修改描述：v14.3.126 增加TransfersResult类

    修改标识：Senparc - 20170215
    修改描述：v14.3.126 增加GetTransferInfoResult类

    修改标识：Senparc - 20170316
    修改描述：v14.3.132 完善UnifiedorderResult 服务商统一订单接口

    修改标识：Senparc - 20170322
    修改描述：v14.3.132 完善OrderQueryResult 服务商查询订单接口
    
    修改标识：jiehanlin & Senparc - 20180309
    修改描述：v14.10.5 TenPayV3Result 增加 ResultXML 只读属性 & 优化代码

    修改标识：jiehanlin & Senparc - 20180309
    修改描述：v14.10.12 新增 TenpayV3GetSignKeyResult

    修改标识：Senparc - 20171129
    修改描述：添加PayBankResult（付款到银行卡）
    
    修改标识：Senparc - 20180409
    修改描述：将 TenPayV3Result.cs 改名为 TenPayV3Results.cs

    修改标识：Senparc - 20181028
    修改描述：v1.0.1 优化 TenPayV3Result.GetXmlValues() 方法

    修改标识：Senparc - 20190906
    修改描述：v1.4.5 添加 GetTransferInfoResult.payment_time 属性

    修改标识：Senparc - 20190925
    修改描述：v1.5.0 商户的企业付款查询结果实体（GetTransferInfoResult）payment_time字段空值修复
    
    修改标识：hesi815 - 20200318
    修改描述：v1.5.401 实现分账接口，添加 ProfitSharingResult、ProfitSharingAddReceiverResult、ProfitSharingRemoveReceiverResult、ProfitSharingQueryResult、ProfitSharingQueryResult

    修改标识：anhuisunfei - 20200731
    修改描述：v1.5.502.4 添加支付退款详情列表

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Utilities;
using Senparc.Weixin.Entities;

namespace Senparc.Weixin.TenPay.V3
{

    #region 基类

    /// <summary>
    /// 基础返回结果（微信支付返回结果基类）
    /// </summary>
    public class TenPayV3Result
    {
        public string return_code { get; set; }
        public string return_msg { get; set; }

        protected XDocument _resultXml;

        /// <summary>
        /// XML内容
        /// </summary>
        public string ResultXml
        {
            get
            {
                return _resultXml.ToString();

                //StringWriter sw = new StringWriter();
                //XmlTextWriter xmlTextWriter = new XmlTextWriter(sw);
                //_resultXml.WriteTo(xmlTextWriter);
                //return sw.ToString();
            }
        }

        public TenPayV3Result(string resultXml)
        {
            _resultXml = XDocument.Parse(resultXml);
            return_code = GetXmlValue("return_code"); // res.Element("xml").Element
            if (!IsReturnCodeSuccess())
            {
                return_msg = GetXmlValue("return_msg"); // res.Element("xml").Element
            }
        }

        /// <summary>
        /// 获取Xml结果中对应节点的值
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public string GetXmlValue(string nodeName)
        {
            if (_resultXml == null || _resultXml.Element("xml") == null
                || _resultXml.Element("xml").Element(nodeName) == null)
            {
                return "";
            }
            return _resultXml.Element("xml").Element(nodeName).Value;
        }

        public int GetXmlValueAsInt(string nodeName)
        {
            string result = this.GetXmlValue(nodeName);
            if (string.IsNullOrWhiteSpace(result)) return 0;
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// 获取Xml结果中对应节点的集合值
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public List<T> GetXmlValues<T>(string nodeName)
        {
            var result = new List<T>();
            try
            {
                if (_resultXml != null)
                {
                    var xElement = _resultXml.Element("xml");
                    if (xElement != null)
                    {
                        var nodeList = xElement.Elements().Where(z => z.Name.ToString().StartsWith(nodeName));

                        result = nodeList.Select(z => {
                            try
                            {
                                return (z.Value as IConvertible).ConvertTo<T>();
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }).ToList();
                    }
                }
            }
            catch (System.Exception)
            {
                result = null;
            }
            return result;
        }


        public bool IsReturnCodeSuccess()
        {
            return return_code == "SUCCESS";
        }
    }

    /// <summary>
    /// 统一支付接口在 return_code为 SUCCESS的时候有返回
    /// </summary>
    public class Result : TenPayV3Result
    {
        /// <summary>
        /// 微信分配的公众账号ID（付款到银行卡接口，此字段不提供）
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// 微信支付分配的商户号
        /// </summary>
        public string mch_id { get; set; }

        #region 服务商
        /// <summary>
        /// 子商户公众账号ID
        /// </summary>
        public string sub_appid { get; set; }

        /// <summary>
        /// 子商户号
        /// </summary>
        public string sub_mch_id { get; set; }

        #endregion

        /// <summary>
        /// 随机字符串，不长于32 位
        /// </summary>
        public string nonce_str { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string sign { get; set; }

        /// <summary>
        /// SUCCESS/FAIL
        /// </summary>
        public string result_code { get; set; }

        public string err_code { get; set; }
        public string err_code_des { get; set; }

        public Result(string resultXml)
            : base(resultXml)
        {
            result_code = GetXmlValue("result_code"); // res.Element("xml").Element

            if (base.IsReturnCodeSuccess())
            {
                appid = GetXmlValue("appid") ?? "";
                mch_id = GetXmlValue("mch_id") ?? "";

                #region 服务商
                sub_appid = GetXmlValue("sub_appid") ?? "";
                sub_mch_id = GetXmlValue("sub_mch_id") ?? "";
                #endregion

                nonce_str = GetXmlValue("nonce_str") ?? "";
                sign = GetXmlValue("sign") ?? "";
                err_code = GetXmlValue("err_code") ?? "";
                err_code_des = GetXmlValue("err_code_des") ?? "";
            }
        }

        /// <summary>
        /// result_code == "SUCCESS"
        /// </summary>
        /// <returns></returns>
        public bool IsResultCodeSuccess()
        {
            return result_code == "SUCCESS";
        }
    }

    #endregion

    /// <summary>
    /// 统一支付接口在return_code 和result_code 都为SUCCESS 的时候有返回详细信息
    /// </summary>
    public class UnifiedorderResult : Result
    {
        /// <summary>
        /// 微信支付分配的终端设备号
        /// </summary>
        public string device_info { get; set; }

        /// <summary>
        /// 交易类型:JSAPI、NATIVE、APP
        /// </summary>
        public string trade_type { get; set; }

        /// <summary>
        /// 微信生成的预支付ID，用于后续接口调用中使用
        /// </summary>
        public string prepay_id { get; set; }

        /// <summary>
        /// trade_type为NATIVE时有返回，此参数可直接生成二维码展示出来进行扫码支付
        /// </summary>
        public string code_url { get; set; }

        /// <summary>
        /// 在H5支付时返回
        /// </summary>
        public string mweb_url { get; set; }

        ///// <summary>
        ///// 子商户公众账号ID
        ///// </summary>
        //public string sub_appid { get; set; }

        ///// <summary>
        ///// 子商户号
        ///// </summary>
        //public string sub_mch_id { get; set; }

        public UnifiedorderResult(string resultXml)
            : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                device_info = GetXmlValue("device_info") ?? "";
                //sub_appid = GetXmlValue("sub_appid") ?? "";
                //sub_mch_id = GetXmlValue("sub_mch_id") ?? "";

                if (base.IsResultCodeSuccess())
                {
                    trade_type = GetXmlValue("trade_type") ?? "";
                    prepay_id = GetXmlValue("prepay_id") ?? "";
                    code_url = GetXmlValue("code_url") ?? "";
                    mweb_url = GetXmlValue("mweb_url") ?? "";
                }
            }
        }
    }

    /// <summary>
    /// 查询订单接口返回结果
    /// </summary>
    public class OrderQueryResult : Result
    {
        /// <summary>
        /// 微信支付分配的终端设备号
        /// </summary>
        public string device_info { get; set; }

        /// <summary>
        /// 用户在商户appid下的唯一标识
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// 用户是否关注公众账号，Y-关注，N-未关注，仅在公众账号类型支付有效
        /// </summary>
        public string is_subscribe { get; set; }

        /// <summary>
        /// 用户子标识[服务商]
        /// </summary>
        public string sub_openid { get; set; }

        /// <summary>
        /// 是否关注子公众账号[服务商]
        /// </summary>
        public string sub_is_subscribe { get; set; }

        /// <summary>
        /// 调用接口提交的交易类型，取值如下：JSAPI，NATIVE，APP，MICROPAY
        /// </summary>
        public string trade_type { get; set; }

        /// <summary>
        ///SUCCESS—支付成功
        ///REFUND—转入退款
        ///NOTPAY—未支付
        ///CLOSED—已关闭
        ///REVOKED—已撤销（刷卡支付）
        ///USERPAYING--用户支付中
        ///PAYERROR--支付失败(其他原因，如银行返回失败)
        /// </summary>
        public string trade_state { get; set; }

        /// <summary>
        /// 银行类型，采用字符串类型的银行标识
        /// </summary>
        public string bank_type { get; set; }

        /// <summary>
        /// 商品详情[服务商]
        /// </summary>
        public string detail { get; set; }

        /// <summary>
        /// 订单总金额，单位为分
        /// </summary>
        public string total_fee { get; set; }

        /// <summary>
        /// 应结订单金额=订单金额-非充值代金券金额，应结订单金额<=订单金额
        /// </summary>
        public string settlement_total_fee { get; set; }

        /// <summary>
        /// 货币类型，符合ISO 4217标准的三位字母代码，默认人民币：CNY
        /// </summary>
        public string fee_type { get; set; }

        /// <summary>
        /// 现金支付金额订单现金支付金额
        /// </summary>
        public string cash_fee { get; set; }

        /// <summary>
        /// 货币类型，符合ISO 4217标准的三位字母代码，默认人民币：CNY
        /// </summary>
        public string cash_fee_type { get; set; }

        /// <summary>
        /// “代金券”金额<=订单金额，订单金额-“代金券”金额=现金支付金额
        /// </summary>
        public string coupon_fee { get; set; }

        /// <summary>
        /// 代金券使用数量
        /// </summary>
        public string coupon_count { get; set; }

        /// <summary>
        /// CASH--充值代金券 
        ///NO_CASH---非充值代金券
        ///订单使用代金券时有返回（取值：CASH、NO_CASH）。$n为下标,从0开始编号，举例：coupon_type_$0
        ///coupon_type_$n
        /// </summary>
        public List<string> coupon_type_values { get; set; }

        /// <summary>
        /// 代金券ID, $n为下标，从0开始编号
        /// coupon_id_$n
        /// </summary>
        public List<string> coupon_id_values { get; set; }

        /// <summary>
        /// 单个代金券支付金额, $n为下标，从0开始编号
        /// coupon_fee_$n
        /// </summary>
        public List<int> coupon_fee_values { get; set; }

        /// <summary>
        /// 微信支付订单号
        /// </summary>
        public string transaction_id { get; set; }

        /// <summary>
        /// 商户系统的订单号，与请求一致。
        /// </summary>
        public string out_trade_no { get; set; }

        /// <summary>
        /// 附加数据，原样返回
        /// </summary>
        public string attach { get; set; }

        /// <summary>
        /// 订单支付时间，格式为yyyyMMddHHmmss，如2009年12月25日9点10分10秒表示为20091225091010
        /// </summary>
        public string time_end { get; set; }

        /// <summary>
        /// 对当前查询订单状态的描述和下一步操作的指引
        /// </summary>
        public string trade_state_desc { get; set; }

        public OrderQueryResult(string resultXml)
            : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                if (base.IsResultCodeSuccess())
                {
                    device_info = GetXmlValue("device_info") ?? "";

                    openid = GetXmlValue("openid") ?? "";
                    is_subscribe = GetXmlValue("is_subscribe") ?? "";

                    sub_openid = GetXmlValue("sub_openid") ?? "";               //用户子标识[服务商]
                    sub_is_subscribe = GetXmlValue("sub_is_subscribe") ?? "";   //是否关注子公众账号[服务商]

                    trade_type = GetXmlValue("trade_type") ?? "";
                    trade_state = GetXmlValue("trade_state") ?? "";
                    bank_type = GetXmlValue("bank_type") ?? "";

                    detail = GetXmlValue("detail") ?? "";                       //商品详情[服务商]

                    total_fee = GetXmlValue("total_fee") ?? "";

                    settlement_total_fee = GetXmlValue("settlement_total_fee") ?? "";

                    fee_type = GetXmlValue("fee_type") ?? "";
                    cash_fee = GetXmlValue("cash_fee") ?? "";
                    cash_fee_type = GetXmlValue("cash_fee_type") ?? "";
                    coupon_fee = GetXmlValue("coupon_fee") ?? "";
                    coupon_count = GetXmlValue("coupon_count") ?? "";

                    #region 特殊"$n"

                    coupon_type_values = GetXmlValues<string>("coupon_type_") ?? new List<string>();
                    coupon_id_values = GetXmlValues<string>("coupon_id_") ?? new List<string>();
                    coupon_fee_values = GetXmlValues<int>("coupon_fee_") ?? new List<int>();

                    #endregion

                    transaction_id = GetXmlValue("transaction_id") ?? "";
                    out_trade_no = GetXmlValue("out_trade_no") ?? "";
                    attach = GetXmlValue("attach") ?? "";
                    time_end = GetXmlValue("time_end") ?? "";
                    trade_state_desc = GetXmlValue("trade_state_desc") ?? "";
                }
            }
        }
    }

    /// <summary>
    /// 关闭订单接口
    /// </summary>
    public class CloseOrderResult : Result
    {
        /// <summary>
        /// 对于业务执行的详细描述
        /// </summary>
        public string result_msg { get; set; }

        public CloseOrderResult(string resultXml) : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                result_msg = GetXmlValue("result_msg") ?? "";
            }
        }
    }

    /// <summary>
    /// 申请退款接口
    /// </summary>
    public class RefundResult : Result
    {
        #region 错误代码
        /*
            名称  描述 原因  解决方案
            SYSTEMERROR 接口返回错误 系统超时等   请不要更换商户退款单号，请使用相同参数再次调用API。
        TRADE_OVERDUE 订单已经超过退款期限  订单已经超过可退款的最大期限(支付后一年内可退款)   请选择其他方式自行退款
            ERROR   业务错误 申请退款业务发生错误  该错误都会返回具体的错误原因，请根据实际返回做相应处理。
        USER_ACCOUNT_ABNORMAL 退款请求失败  用户帐号注销 此状态代表退款申请失败，商户可自行处理退款。
        INVALID_REQ_TOO_MUCH 无效请求过多  连续错误请求数过多被系统短暂屏蔽 请检查业务是否正常，确认业务正常后请在1分钟后再来重试
            NOTENOUGH   余额不足 商户可用退款余额不足  此状态代表退款申请失败，商户可根据具体的错误提示做相应的处理。
        INVALID_TRANSACTIONID 无效transaction_id    请求参数未按指引进行填写 请求参数错误，检查原交易号是否存在或发起支付交易接口返回失败
            PARAM_ERROR 参数错误 请求参数未按指引进行填写    请求参数错误，请重新检查再调用退款申请
            APPID_NOT_EXIST APPID不存在 参数中缺少APPID  请检查APPID是否正确
            MCHID_NOT_EXIST MCHID不存在 参数中缺少MCHID  请检查MCHID是否正确
            APPID_MCHID_NOT_MATCH   appid和mch_id不匹配 appid和mch_id不匹配 请确认appid和mch_id是否匹配
            REQUIRE_POST_METHOD 请使用post方法 未使用post传递参数     请检查请求参数是否通过post方法提交
            SIGNERROR   签名错误 参数签名结果不正确   请检查签名参数和方法是否都符合签名算法要求
            XML_FORMAT_ERROR    XML格式错误 XML格式错误 请检查XML参数格式是否正确
            FREQUENCY_LIMITED   频率限制	2个月之前的订单申请退款有频率限制 该笔退款未受理，请降低频率后重试
         */

        #endregion


        /// <summary>
        /// 	微信支付分配的终端设备号，与下单一致
        /// </summary>
        public string device_info { get; set; }

        /// <summary>
        /// 微信订单号
        /// </summary>
        public string transaction_id { get; set; }
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string out_trade_no { get; set; }
        /// <summary>
        /// 商户退款单号	
        /// </summary>
        public string out_refund_no { get; set; }
        /// <summary>
        /// 微信退款单号
        /// </summary>
        public string refund_id { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        public string refund_fee { get; set; }
        /// <summary>
        /// 应结退款金额
        /// </summary>
        public string settlement_refund_fee { get; set; }
        /// <summary>
        /// 标价金额
        /// </summary>
        public string total_fee { get; set; }
        /// <summary>
        /// 应结订单金额
        /// </summary>
        public string settlement_total_fee { get; set; }
        /// <summary>
        /// 标价币种
        /// </summary>
        public string fee_type { get; set; }
        /// <summary>
        /// 现金支付金额
        /// </summary>
        public string cash_fee { get; set; }
        /// <summary>
        /// 现金支付币种
        /// </summary>
        public string cash_fee_type { get; set; }
        /// <summary>
        /// 现金退款金额	
        /// </summary>
        public string cash_refund_fee { get; set; }
        /// <summary>
        /// 代金券退款总金额
        /// </summary>
        public string coupon_refund_fee { get; set; }
        /// <summary>
        /// 退款代金券使用数量
        /// </summary>
        public string coupon_refund_count { get; set; }


        #region 带下标参数

        /// <summary>
        /// 代金券类型
        /// </summary>
        public List<string> coupon_type_n { get; set; }
        /// <summary>
        /// 单个代金券退款金额
        /// </summary>
        public List<int> coupon_refund_fee_n { get; set; }
        /// <summary>
        /// 退款代金券ID	
        /// </summary>
        public List<string> coupon_refund_id_n { get; set; }

        #endregion


        public RefundResult(string resultXml) : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                result_code = GetXmlValue("result_code");
                err_code = GetXmlValue("err_code");
                err_code_des = GetXmlValue("err_code_des");
                appid = GetXmlValue("appid");
                mch_id = GetXmlValue("mch_id");
                device_info = GetXmlValue("device_info");
                nonce_str = GetXmlValue("nonce_str");
                sign = GetXmlValue("sign");
                transaction_id = GetXmlValue("transaction_id");
                out_trade_no = GetXmlValue("out_trade_no");
                out_refund_no = GetXmlValue("out_refund_no");
                refund_id = GetXmlValue("refund_id");
                refund_fee = GetXmlValue("refund_fee");
                settlement_refund_fee = GetXmlValue("settlement_refund_fee");
                total_fee = GetXmlValue("total_fee");
                settlement_total_fee = GetXmlValue("settlement_total_fee");
                fee_type = GetXmlValue("fee_type");
                cash_fee = GetXmlValue("cash_fee");
                cash_fee_type = GetXmlValue("cash_fee_type");
                cash_refund_fee = GetXmlValue("cash_refund_fee");
                coupon_refund_fee = GetXmlValue("coupon_refund_fee");
                coupon_refund_count = GetXmlValue("coupon_refund_count");
                coupon_type_n = GetXmlValues<string>("coupon_type_n");
                coupon_refund_fee_n = GetXmlValues<int>("coupon_refund_fee_n");
                coupon_refund_id_n = GetXmlValues<string>("coupon_refund_id_n");
            }
        }
    }

    /// <summary>
    /// 退款查询接口
    /// </summary>
    public class RefundQueryResult : Result
    {
        /// <summary>
        /// 终端设备号
        /// </summary>
        public string device_info { get; set; }

        /// <summary>
        /// 微信订单号
        /// </summary>
        public string transaction_id { get; set; }

        /// <summary>
        /// 商户系统内部的订单号
        /// </summary>
        public string out_trade_no { get; set; }

        /// <summary>
        /// 订单总金额，单位为分，只能为整数
        /// </summary>
        public string total_fee { get; set; }

        /// <summary>
        /// 应结订单金额=订单金额-非充值代金券金额，应结订单金额<=订单金额
        /// </summary>
        public string settlement_total_fee { get; set; }

        /// <summary>
        /// 订单金额货币类型，符合ISO 4217标准的三位字母代码，默认人民币：CNY
        /// </summary>
        public string fee_type { get; set; }

        /// <summary>
        /// 现金支付金额，单位为分，只能为整数
        /// </summary>
        public string cash_fee { get; set; }

        /// <summary>
        /// 退款记录数
        /// </summary>
        public string refund_count { get; set; }

        /// <summary>
        /// 商户退款单号
        /// </summary>
        ///public string out_refund_no_$n { get; set; }
        /// <summary>
        /// 微信退款单号
        /// </summary>
        //public string refund_id_$n { get; set; }
        /// <summary>
        /// ORIGINAL—原路退款
        //BALANCE—退回到余额
        /// </summary>
        //public string refund_channel_$n { get; set; }
        /// <summary>
        /// 退款总金额,单位为分,可以做部分退款
        /// </summary>
        //public string refund_fee_$n { get; set; }
        /// <summary>
        /// 退款金额=申请退款金额-非充值代金券退款金额，退款金额<=申请退款金额
        /// </summary>
        //public string settlement_refund_fee_$n { get; set; }
        /// <summary>
        /// REFUND_SOURCE_RECHARGE_FUNDS---可用余额退款/基本账户
        //REFUND_SOURCE_UNSETTLED_FUNDS---未结算资金退款
        /// </summary>
        public string refund_account { get; set; }

        /// <summary>
        /// CASH--充值代金券 
        //NO_CASH---非充值代金券
        //订单使用代金券时有返回（取值：CASH、NO_CASH）。$n为下标,从0开始编号，举例：coupon_type_$0
        /// </summary>
        //public string coupon_type_$n { get; set; }
        /// <summary>
        /// 代金券退款金额<=退款金额，退款金额-代金券或立减优惠退款金额为现金
        /// </summary>
        //public string coupon_refund_fee_$n { get; set; }
        /// <summary>
        /// 退款代金券使用数量 ,$n为下标,从0开始编号
        /// </summary>
        //public string coupon_refund_count_$n { get; set; }
        /// <summary>
        /// 退款代金券ID, $n为下标，$m为下标，从0开始编号
        /// </summary>
        //public string coupon_refund_id_$n_$m { get; set; }
        /// <summary>
        /// 单个退款代金券支付金额, $n为下标，$m为下标，从0开始编号
        /// </summary>
        ///public string coupon_refund_fee_$n_$m { get; set; }
        /// <summary>
        /// 退款状态：
        ///SUCCESS—退款成功
        ///FAIL—退款失败
        ///PROCESSING—退款处理中
        ///CHANGE—转入代发，退款到银行发现用户的卡作废或者冻结了，导致原路退款银行卡失败，资金回流到商户的现金帐号，需要商户人工干预，通过线下或者财付通转账的方式进行退款。
        /// </summary>
        //public string refund_status_$n { get; set; }
        /// <summary>
        /// 取当前退款单的退款入账方
        ///1）退回银行卡：
        ///{银行名称
        ///    }{卡类型
        ///}{卡尾号}
        ///2）退回支付用户零钱:
        ///支付用户零钱
        /// </summary>
        //public string refund_recv_accout_$n { get; set; }
        public RefundQueryResult(string resultXml) : base(resultXml)
        {
            if (base.IsResultCodeSuccess())
            {
                device_info = GetXmlValue("device_info") ?? "";
                transaction_id = GetXmlValue("transaction_id") ?? "";
                out_trade_no = GetXmlValue("out_trade_no") ?? "";
                total_fee = GetXmlValue("total_fee") ?? "";
                settlement_total_fee = GetXmlValue("settlement_total_fee") ?? "";
                fee_type = GetXmlValue("fee_type") ?? "";
                cash_fee = GetXmlValue("cash_fee") ?? "";
                refund_count = GetXmlValue("refund_count") ?? "";
                //out_refund_no_$n = GetXmlValue("out_refund_no_$n") ?? "";
                //refund_id_$n = GetXmlValue("refund_id_$n") ?? "";
                //refund_channel_$n = GetXmlValue("refund_channel_$n") ?? "";
                //refund_fee_$n = GetXmlValue("refund_fee_$n") ?? "";
                //settlement_refund_fee_$n = GetXmlValue("settlement_refund_fee_$n") ?? "";
                refund_account = GetXmlValue("refund_account") ?? "";
                //coupon_type_$n = GetXmlValue("coupon_type_$n") ?? "";
                //coupon_refund_fee_$n = GetXmlValue("coupon_refund_fee_$n") ?? "";
                //coupon_refund_count_$n = GetXmlValue("coupon_refund_count_$n") ?? "";
                //coupon_refund_id_$n_$m = GetXmlValue("coupon_refund_id_$n_$m") ?? "";
                //coupon_refund_fee_$n_$m = GetXmlValue("coupon_refund_fee_$n_$m") ?? "";
                //refund_status_$n = GetXmlValue("refund_status_$n") ?? "";
                //refund_recv_accout_$n = GetXmlValue("refund_recv_accout_$n") ?? "";

                ComposeRefundRecords();
            }
        }

        public List<RefundRecord> refundRecords;
        /// <summary>
        /// 组装生成退款记录属性的内容
        /// </summary>
        public void ComposeRefundRecords()
        {

            if (this.refund_count != null && int.TryParse(refund_count, out int refundCount) && refundCount > 0)
            {
                this.refundRecords = new List<RefundRecord>();
                for (int i = 0; i < refundCount; i++)
                {
                    RefundRecord refundRecord = new RefundRecord();
                    this.refundRecords.Add(refundRecord);
                    refundRecord.out_refund_no = this.GetXmlValue("out_refund_no_" + i);
                    refundRecord.refund_id = this.GetXmlValue("refund_id_" + i);
                    refundRecord.refund_channel = this.GetXmlValue("refund_channel_" + i);
                    refundRecord.refund_fee = this.GetXmlValueAsInt("refund_fee_" + i);
                    refundRecord.settlement_refund_fee = this.GetXmlValueAsInt("settlement_refund_fee_" + i);
                    refundRecord.coupon_refund_fee = this.GetXmlValueAsInt("coupon_refund_fee_" + i);
                    refundRecord.coupon_refund_count = this.GetXmlValueAsInt("coupon_refund_count_" + i);
                    refundRecord.refund_status = this.GetXmlValue("refund_status_" + i);
                    refundRecord.refund_recv_accout = this.GetXmlValue("refund_recv_accout_" + i);
                    refundRecord.refund_success_time = this.GetXmlValue("refund_success_time_" + i);
                    if (refundRecord.coupon_refund_count == 0)
                    {
                        continue;
                    }
                    List<WxPayRefundCouponInfo> coupons = new List<WxPayRefundCouponInfo>();
                    for (int j = 0; j < refundRecord.coupon_refund_count; j++)
                    {
                        var coupon = new WxPayRefundCouponInfo();
                        coupon.coupon_refund_id = this.GetXmlValue("coupon_refund_id_" + i + "_" + j);
                        coupon.coupon_refund_fee = this.GetXmlValueAsInt("coupon_refund_fee_" + i + "_" + j);
                        coupon.coupon_type = this.GetXmlValue("coupon_type_" + i + "_" + j);
                        coupons.Add(coupon);
                    }
                    refundRecord.refundCoupons = coupons;

                }
            }
        }
    }

    /// <summary>
    /// 退款代金券信息.
    /// </summary>
    public class WxPayRefundCouponInfo
    {
        /**
          * <pre>
          * 字段名：退款代金券ID.
          * 变量名：coupon_refund_id_$n_$m
          * 是否必填：否
          * 类型：String(20)
          * 示例值：10000
          * 描述：退款代金券ID, $n为下标，$m为下标，从0开始编号
          * </pre>
          */
        public string coupon_refund_id { get; set; }

        /**
         * <pre>
         * 字段名：单个退款代金券支付金额.
         * 变量名：coupon_refund_fee_$n_$m
         * 是否必填：否
         * 类型：Int
         * 示例值：100
         * 描述：单个退款代金券支付金额, $n为下标，$m为下标，从0开始编号
         * </pre>
         */
        public int coupon_refund_fee { get; set; }

        /**
         * <pre>
         * 字段名：代金券类型.
         * 变量名：coupon_type_$n_$m
         * 是否必填：否
         * 类型：String(8)
         * 示例值：CASH
         * 描述：CASH--充值代金券 , NO_CASH---非充值代金券。
         * 开通免充值券功能，并且订单使用了优惠券后有返回（取值：CASH、NO_CASH）。
         * $n为下标,$m为下标,从0开始编号，举例：coupon_type_$0_$1
         * </pre>
         */
        public string coupon_type { get; set; }
    }

    /// <summary>
    /// 退款明细
    /// </summary>
    public class RefundRecord
    {
        /**
         * <pre>
         * 字段名：商户退款单号.
         * 变量名：out_refund_no_$n
         * 是否必填：是
         * 类型：String(32)
         * 示例值：1217752501201407033233368018
         * 描述：商户退款单号
         * </pre>
         */
        public string out_refund_no { get; set; }

        /**
         * <pre>
         * 字段名：微信退款单号.
         * 变量名：refund_id_$n
         * 是否必填：是
         * 类型：String(28)
         * 示例值：1217752501201407033233368018
         * 描述：微信退款单号
         * </pre>
         */

        public string refund_id { get; set; }

        /**
         * <pre>
         * 字段名：退款渠道.
         * 变量名：refund_channel_$n
         * 是否必填：否
         * 类型：String(16)
         * 示例值：ORIGINAL
         * 描述：ORIGINAL—原路退款 BALANCE—退回到余额
         * </pre>
         */

        public string refund_channel { get; set; }

        /**
         * <pre>
         * 字段名：申请退款金额.
         * 变量名：refund_fee_$n
         * 是否必填：是
         * 类型：Int
         * 示例值：100
         * 描述：退款总金额,单位为分,可以做部分退款
         * </pre>
         */

        public int refund_fee { get; set; }

        /**
         * <pre>
         * 字段名：退款金额.
         * 变量名：settlement_refund_fee_$n
         * 是否必填：否
         * 类型：Int
         * 示例值：100
         * 描述：退款金额=申请退款金额-非充值代金券退款金额，退款金额<=申请退款金额
         * </pre>
         */

        public int settlement_refund_fee { get; set; }

        /**
         * <pre>
         * 字段名：退款资金来源.
         * 变量名：refund_account
         * 是否必填：否
         * 类型：String(30)
         * 示例值：REFUND_SOURCE_RECHARGE_FUNDS
         * 描述：REFUND_SOURCE_RECHARGE_FUNDS---可用余额退款/基本账户, REFUND_SOURCE_UNSETTLED_FUNDS---未结算资金退款
         * </pre>
         */

        public string refund_account { get; set; }

        /**
         * <pre>
         * 字段名：代金券退款金额.
         * 变量名：coupon_refund_fee_$n
         * 是否必填：否
         * 类型：Int
         * 示例值：100
         * 描述：代金券退款金额<=退款金额，退款金额-代金券或立减优惠退款金额为现金，说明详见代金券或立减优惠
         * </pre>
         */

        public int coupon_refund_fee { get; set; }

        /**
         * <pre>
         * 字段名：退款代金券使用数量.
         * 变量名：coupon_refund_count_$n
         * 是否必填：否
         * 类型：Int
         * 示例值：1
         * 描述：退款代金券使用数量 ,$n为下标,从0开始编号
         * </pre>
         */
        public int coupon_refund_count { get; set; }


        public List<WxPayRefundCouponInfo> refundCoupons;


        /**
         * <pre>
         * 字段名：退款状态.
         * 变量名：refund_status_$n
         * 是否必填：是
         * 类型：String(16)
         * 示例值：SUCCESS
         * 描述：退款状态：
         *  SUCCESS—退款成功，
         *  FAIL—退款失败，
         *  PROCESSING—退款处理中，
         *  CHANGE—转入代发，
         * 退款到银行发现用户的卡作废或者冻结了，导致原路退款银行卡失败，资金回流到商户的现金帐号，需要商户人工干预，通过线下或者财付通转账的方式进行退款。
         * </pre>
         */
        public String refund_status { get; set; }

        /**
         * <pre>
         * 字段名：退款入账账户.
         * 变量名：refund_recv_accout_$n
         * 是否必填：是
         * 类型：String(64)
         * 示例值：招商银行信用卡0403
         * 描述：取当前退款单的退款入账方，1）退回银行卡：{银行名称}{卡类型}{卡尾号}，2）退回支付用户零钱:支付用户零钱
         * </pre>
         */
        public String refund_recv_accout { get; set; }

        /**
         * <pre>
         * 字段名：退款成功时间.
         * 变量名：refund_success_time_$n
         * 是否必填：否
         * 类型：String(20)
         * 示例值：2016-07-25 15:26:26
         * 描述：退款成功时间，当退款状态为退款成功时有返回。$n为下标，从0开始编号。
         * </pre>
         */
        public String refund_success_time { get; set; }

    }

    /// <summary>
    /// 短链接转换接口
    /// </summary>
    public class ShortUrlResult : Result
    {
        /// <summary>
        /// 转换后的URL
        /// </summary>
        public string short_url { get; set; }

        public ShortUrlResult(string resultXml) : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                short_url = GetXmlValue("short_url") ?? "";
            }
        }
    }

    ///// <summary>
    ///// 对账单接口
    ///// </summary>
    //public class DownloadBillResult : TenPayV3Result
    //{
    //    public DownloadBillResult(string resultXml) : base(resultXml)
    //    {

    //    }
    //}

    /// <summary>
    /// 撤销订单接口
    /// </summary>
    public class ReverseResult : Result
    {
        /// <summary>
        /// 是否需要继续调用撤销，Y-需要，N-不需要
        /// </summary>
        public string recall { get; set; }

        public ReverseResult(string resultXml) : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                recall = GetXmlValue("recall") ?? "";
            }
        }
    }

    /// <summary>
    /// 刷卡支付
    /// 提交被扫支付
    /// </summary>
    public class MicropayResult : Result
    {
        /// <summary>
        /// 调用接口提交的终端设备号
        /// </summary>
        public string device_info { get; set; }

        /// <summary>
        /// 用户在商户appid 下的唯一标识
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// 用户是否关注公众账号，仅在公众账号类型支付有效，取值范围：Y或N;Y-关注;N-未关注
        /// </summary>
        public string is_subscribe { get; set; }

        /// <summary>
        /// 支付类型为MICROPAY(即扫码支付)
        /// </summary>
        public string trade_type { get; set; }

        /// <summary>
        /// 银行类型，采用字符串类型的银行标识
        /// </summary>
        public string bank_type { get; set; }

        /// <summary>
        /// 符合ISO 4217标准的三位字母代码，默认人民币：CNY
        /// </summary>
        public string fee_type { get; set; }

        /// <summary>
        /// 订单总金额，单位为分，只能为整数
        /// </summary>
        public string total_fee { get; set; }

        /// <summary>
        /// 应结订单金额=订单金额-非充值代金券金额，应结订单金额<=订单金额
        /// </summary>
        public string settlement_total_fee { get; set; }

        /// <summary>
        /// “代金券”金额<=订单金额，订单金额-“代金券”金额=现金支付金额
        /// </summary>
        public string coupon_fee { get; set; }

        /// <summary>
        /// 符合ISO 4217标准的三位字母代码，默认人民币：CNY
        /// </summary>
        public string cash_fee_type { get; set; }

        /// <summary>
        /// 订单现金支付金额
        /// </summary>
        public string cash_fee { get; set; }

        /// <summary>
        /// 微信支付订单号
        /// </summary>
        public string transaction_id { get; set; }

        /// <summary>
        /// 商户系统的订单号，与请求一致
        /// </summary>
        public string out_trade_no { get; set; }

        /// <summary>
        /// 商家数据包，原样返回
        /// </summary>
        public string attach { get; set; }

        /// <summary>
        /// 订单生成时间，格式为yyyyMMddHHmmss，如2009年12月25日9点10分10秒表示为20091225091010
        /// </summary>
        public string time_end { get; set; }

        public MicropayResult(string resultXml) : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                device_info = GetXmlValue("device_info") ?? "";
                if (base.IsResultCodeSuccess())
                {
                    openid = GetXmlValue("openid") ?? "";
                    is_subscribe = GetXmlValue("is_subscribe") ?? "";
                    trade_type = GetXmlValue("trade_type") ?? "";
                    bank_type = GetXmlValue("bank_type") ?? "";
                    fee_type = GetXmlValue("fee_type") ?? "";
                    total_fee = GetXmlValue("total_fee") ?? "";
                    settlement_total_fee = GetXmlValue("settlement_total_fee") ?? "";
                    coupon_fee = GetXmlValue("coupon_fee") ?? "";
                    cash_fee_type = GetXmlValue("cash_fee_type") ?? "";
                    cash_fee = GetXmlValue("cash_fee") ?? "";
                    transaction_id = GetXmlValue("transaction_id") ?? "";
                    out_trade_no = GetXmlValue("out_trade_no") ?? "";
                    attach = GetXmlValue("attach") ?? "";
                    time_end = GetXmlValue("time_end") ?? "";
                }
            }
        }
    }


    /// <summary>
    /// 企业付款
    /// </summary>
    public class TransfersResult : TenPayV3Result
    {
        /// <summary>
        /// 商户appid
        /// </summary>
        public string mch_appid { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string mchid { get; set; }

        /// <summary>
        /// 设备号
        /// </summary>
        public string device_info { get; set; }


        /// <summary>
        /// 随机字符串
        /// </summary>
        public string nonce_str { get; set; }

        /// <summary>
        ///业务结果 SUCCESS/FAIL
        /// </summary>
        public string result_code { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string err_code { get; set; }

        /// <summary>
        /// 错误代码描述
        /// </summary>
        public string err_code_des { get; set; }

        /// <summary>
        ///商户订单号
        /// </summary>
        public string partner_trade_no { get; set; }

        /// <summary>
        /// 微信订单号
        /// </summary>
        public string payment_no { get; set; }

        /// <summary>
        /// 微信支付成功时间 
        /// </summary>
        public string payment_time { get; set; }

        public TransfersResult(string resultXml) : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                mch_appid = GetXmlValue("mch_appid") ?? "";
                mchid = GetXmlValue("mchid") ?? "";
                device_info = GetXmlValue("device_info") ?? "";
                nonce_str = GetXmlValue("nonce_str") ?? "";
                result_code = GetXmlValue("result_code") ?? "";
                err_code = GetXmlValue("err_code") ?? "";
                err_code_des = GetXmlValue("err_code_des") ?? "";
                if (IsResultCodeSuccess())
                {
                    partner_trade_no = GetXmlValue("partner_trade_no") ?? "";
                    payment_no = GetXmlValue("payment_no") ?? "";
                    payment_time = GetXmlValue("payment_time") ?? "";
                }
            }
        }
        public bool IsResultCodeSuccess()
        {
            return result_code == "SUCCESS";
        }
    }

    /// <summary>
    /// 商户的企业付款操作进行结果查询，返回付款操作详细结果
    /// </summary>
    public class GetTransferInfoResult : TenPayV3Result
    {

        /// <summary>
        ///业务结果 SUCCESS/FAIL
        /// </summary>
        public string result_code { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string err_code { get; set; }

        /// <summary>
        /// 错误代码描述
        /// </summary>
        public string err_code_des { get; set; }

        /// <summary>
        ///商户单号
        /// </summary>
        public string partner_trade_no { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string mch_id { get; set; }

        /// <summary>
        /// 付款单号
        /// </summary>
        public string detail_id { get; set; }

        /// <summary>
        ///转账状态
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        public string reason { get; set; }

        /// <summary>
        /// 收款用户openid
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// 收款用户姓名
        /// </summary>
        public string transfer_name { get; set; }

        /// <summary>
        ///付款金额
        /// </summary>
        public int payment_amount { get; set; }

        /// <summary>
        /// 转账时间
        /// </summary>
        public string transfer_time { get; set; }

        /// <summary>
        /// 付款描述
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        /// 企业付款成功时间，如：2015-04-21 20:01:00	
        /// </summary>
        public string payment_time { get; set; }

        public GetTransferInfoResult(string resultXml) : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                result_code = GetXmlValue("result_code") ?? "";
                err_code = GetXmlValue("err_code") ?? "";
                err_code_des = GetXmlValue("err_code_des") ?? "";
                if (IsResultCodeSuccess())
                {
                    partner_trade_no = GetXmlValue("partner_trade_no") ?? "";
                    mch_id = GetXmlValue("mch_id") ?? "";
                    detail_id = GetXmlValue("detail_id") ?? "";
                    status = GetXmlValue("status") ?? "";
                    reason = GetXmlValue("reason") ?? "";
                    openid = GetXmlValue("openid") ?? "";
                    transfer_name = GetXmlValue("transfer_name") ?? "";
                    payment_amount = int.Parse(GetXmlValue("payment_amount"));
                    transfer_time = GetXmlValue("transfer_time") ?? "";
                    desc = GetXmlValue("desc") ?? "";
                    payment_time = GetXmlValue("payment_time") ?? "";
                }
            }
        }
        public bool IsResultCodeSuccess()
        {
            return result_code == "SUCCESS";
        }
    }



    /// <summary>
    /// 获取验签秘钥API 返回结果
    /// </summary>
    public class TenpayV3GetSignKeyResult : TenPayV3Result
    {
        ///// <summary>
        ///// SUCCESS/FAIL 此字段是通信标识，非交易标识
        ///// </summary>
        //public string return_code { get; set; }

        ///// <summary>
        ///// 返回信息，如非空，为错误原因 ，签名失败 ，参数格式校验错误
        ///// </summary>
        //public string return_msg { get; set; }

        /// <summary>
        /// 微信支付分配的微信商户号
        /// </summary>
        public string mch_id { get; set; }

        /// <summary>
        /// 返回的沙箱密钥
        /// </summary>
        public string sandbox_signkey { get; set; }

        /// <summary>
        /// 获取验签秘钥API 返回结果 构造函数
        /// </summary>
        /// <param name="resultXml"></param>
        public TenpayV3GetSignKeyResult(string resultXml) : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                mch_id = GetXmlValue("mch_id") ?? "";
                sandbox_signkey = GetXmlValue("sandbox_signkey") ?? "";
            }
        }
    }


    /// <summary>
    /// 单次分账操作结果
    /// https://pay.weixin.qq.com/wiki/doc/api/allocation_sl.php?chapter=25_1&index=1 或者 
    /// https://pay.weixin.qq.com/wiki/doc/api/allocation.php?chapter=27_1&index=1 
    /// </summary>
    public class ProfitSharingResult : Result
    {
        /// <summary>
        /// 微信订单号 
        /// </summary>
        public string transaction_id
        { get; set; }

        /// <summary>
        /// 商户分账单号,调用接口提供的商户系统内部的分账单号 
        /// </summary>
        public string out_order_no
        { get; set; }

        /// <summary>
        /// 微信分账单号,微信分账单号，微信系统返回的唯一标识 
        /// </summary>
        public string order_id
        { get; set; }

        /// <summary>
        /// 分账接收方
        /// 分账接收方对象（不包含分账接收方全称）
        /// </summary>
        public TenpayV3ProfitShareingAddReceiverRequestData_ReceiverInfo receiver
        { get; set; }


        public ProfitSharingResult(string resultXml)
            : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                transaction_id = GetXmlValue("transaction_id");
                out_order_no = GetXmlValue("out_order_no");
                order_id = GetXmlValue("order_id");
            }
        }
    }


    /// <summary>
    /// 添加分账接收方的操作结果操作结果
    /// 服务商特约商户新增分账接收方https://pay.weixin.qq.com/wiki/doc/api/allocation.php?chapter=27_3&index=4 或者 
    /// 服务商特约商户新增分账接收方https://pay.weixin.qq.com/wiki/doc/api/allocation_sl.php?chapter=25_3&index=4 
    /// </summary>
    public class ProfitSharingAddReceiverResult : Result
    {
        /// <summary>
        /// 分账接收方
        /// </summary>
        public TenpayV3ProfitShareing_ReceiverInfo receiver
        { get; set; }


        public ProfitSharingAddReceiverResult(string resultXml)
            : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                var receiverJsonString = GetXmlValue("receiver"); //xml中包含有Json字符串,但是 CommonJson
                this.receiver = SerializerHelper.GetObject<TenpayV3ProfitShareing_ReceiverInfo>(receiverJsonString);
            }
        }
    }


    /// <summary>
    /// 删除分账接收方的操作结果
    /// 服务商特约商户: https://pay.weixin.qq.com/wiki/doc/api/allocation_sl.php?chapter=25_4&index=5
    /// 境内普通商户: https://pay.weixin.qq.com/wiki/doc/api/allocation.php?chapter=27_4&index=5
    /// </summary>
    public class ProfitSharingRemoveReceiverResult : Result
    {
        /// <summary>
        /// 分账接收方
        /// </summary>
        public TenpayV3ProfitShareing_ReceiverInfo receiver
        { get; set; }


        public ProfitSharingRemoveReceiverResult(string resultXml)
            : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                var receiverJsonString = GetXmlValue("receiver"); //xml中包含有Json字符串,但是 CommonJson
                this.receiver = SerializerHelper.GetObject<TenpayV3ProfitShareing_ReceiverInfo>(receiverJsonString);
            }
        }
    }


    /// <summary>
    /// 分账查询的操作结果
    /// 服务商特约商户: https://pay.weixin.qq.com/wiki/doc/api/allocation_sl.php?chapter=25_2&index=3
    /// 境内普通商户: https://pay.weixin.qq.com/wiki/doc/api/allocation.php?chapter=27_2&index=3
    /// </summary>
    public class ProfitSharingQueryResult : Result
    {
        /// <summary>
        /// 微信支付订单号 
        /// </summary>
        public string transaction_id
        { get; set; }

        /// <summary>
        /// 商户系统内部的分账单号，在商户系统内部唯一（单次分账、多次分账、完结分账应使用不同的商户分账单号），
        /// 同一分账单号多次请求等同一次。
        /// 只能是数字、大小写字母_-|*@  
        /// </summary>
        public string order_id
        { get; set; }

        /// <summary>
        /// 微信分账单号
        /// </summary>
        public string out_order_no
        { get; set; }

        /// <summary>
        /// 分账单状态： 
        /// ACCEPTED—受理成功
        /// PROCESSING—处理中
        /// FINISHED—处理完成
        /// CLOSED—处理失败，已关单
        /// </summary>
        public string status
        { get; set; }

        /// <summary>
        /// 关单原因 
        /// </summary>
        public string close_reason
        { get; set; }

        /// <summary>
        /// 分账接收方的分账信息
        /// </summary>
        public TenpayV3ProfitShareingQuery_ReceiverInfo[] receivers
        { get; set; }


        /// <summary>
        /// 分账完结的原因描述，仅当查询分账完结的执行结果时，存在本字段 
        /// </summary>
        public string description
        { get; set; }


        /// <summary>
        /// 分账完结的分账金额，单位为分， 仅当查询分账完结的执行结果时，存在本字段 
        /// </summary>
        public int? amount
        { get; set; }

        public ProfitSharingQueryResult(string resultXml)
            : base(resultXml)
        {
            if (base.IsReturnCodeSuccess())
            {
                this.transaction_id = GetXmlValue("transaction_id");
                this.out_order_no = GetXmlValue("out_order_no");
                this.order_id = GetXmlValue("order_id");

                this.status = GetXmlValue("status");
                this.close_reason = GetXmlValue("close_reason");

                if (this.status == "FINISHED")
                {
                    var amount = GetXmlValue("amount");
                    if (!string.IsNullOrEmpty(amount))
                    {
                        var iamount = 0;
                        if (Int32.TryParse(amount, out iamount))
                        {
                            this.amount = iamount;
                        }
                    }
                    this.description = GetXmlValue("description");
                }


                var receiverJsonString = GetXmlValue("receivers"); //xml中包含有Json字符串,但是 CommonJson
                this.receivers = SerializerHelper.GetObject<TenpayV3ProfitShareingQuery_ReceiverInfo[]>(receiverJsonString);

            }
        }
    }
}
