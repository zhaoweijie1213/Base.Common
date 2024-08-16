using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.ApiResult
{
    /// <summary>
    /// 返回码说明
    /// </summary>
    public enum ApiResultCode : int
    {
        /// <summary>
        /// 操作成功
        /// </summary>
        [Description("Success")]
        Success = 0,

        /// <summary>
        /// 系统错误
        /// </summary>
        [Description("Internal Server Error")]
        InternalServerError = 500,

        /// <summary>
        /// 参数错误
        /// </summary>
        [Description("Error Params")]
        ErrorParams = 2,

        /// <summary>
        /// 重复Id
        /// </summary>
        [Description("Duplicate id")]
        DuplicateId = 3,

        /// <summary>
        /// 重复的请求
        /// </summary>
        [Description("Repeated request")]
        RepeatedRequest = 4,

        /// <summary>
        /// 不支持该国家
        /// </summary>
        [Description("Not support the country")]
        UnSupportTheCountry = 5,

        /// <summary>
        /// 找不到数据
        /// </summary>
        [Description("Data is not found")]
        DataNotFound = 6,

        /// <summary>
        /// 操作失败
        /// </summary>
        [Description("Fail")]
        Fail = -1,

        #region 账号相关

        /// <summary>
        /// token错误
        /// </summary>
        [Description("token error")]
        TokenError = 1004,

        /// <summary>
        /// token失效
        /// </summary>
        [Description("token time out")]
        TokenTimeOutError = 1005,

        /// <summary>
        /// 未找到该用户
        /// </summary>
        [Description("User not found")]
        UserNotFound = 1006,

        /// <summary>
        /// 其他平台错误
        /// </summary>
        [Description("Other platform error")]
        OtherPlatformError = 1007,

        /// <summary>
        /// 已有账户,需要绑定
        /// </summary>
        [Description("Account need bind")]
        AccountRelation = 1008,

        /// <summary>
        /// 验证码错误
        /// </summary>
        [Description("message code error")]
        SmsCodeError = 1010,

        /// <summary>
        /// 验证码发送失败
        /// </summary>
        [Description("message send error")]
        SmsSendError = 1012,

        /// <summary>
        /// 未绑定手机号码
        /// </summary>
        [Description("Not bind error")]
        NotBindError = 1013,

        /// <summary>
        /// 电话号码已经被其它人绑定
        /// </summary>
        [Description("Phone was used")]
        PhoneAlreadyUsed = 1014,

        /// <summary>
        /// 密码错误
        /// </summary>
        [Description("PassWord error")]
        PwdError = 1015,

        /// <summary>
        /// 帐号被占用
        /// </summary>
        [Description("Account Takeup Error")]
        AccountTakeupError = 1016,

        /// <summary>
        /// 密码错误次数达到上限
        /// </summary>
        [Description("Login Error Limit")]
        LoginErrorLimit = 1017,

        /// <summary>
        /// facebook已被绑定
        /// </summary>
        [Description("Facebook has been bound")]
        FaceBookUsed = 1018,

        /// <summary>
        /// 不能使用游客模式登录
        /// </summary>
        [Description("Login with guest is disabled")]
        LoginWithGuestDisabled = 1019,

        /// <summary>
        /// 账号信息备份失败
        /// </summary>
        [Description("Backup Account Failed")]
        BackupAccountFailed = 1020,

        /// <summary>
        /// 账号信息删除失败
        /// </summary>
        [Description("Delete Account Failed")]
        DeleteAccountFailed = 1021,

        /// <summary>
        /// 删除关联Facebook信息失败
        /// </summary>
        [Description("Delete Facebook Account Failed")]
        DeleteFacebookAccountFailed = 1022,

        /// <summary>
        /// 过期token
        /// </summary>
        [Description("token is expiration")]
        ExpirationToken = 1023,

        /// <summary>
        /// 设备已登陆过一个账号，请使用现有账号进行登陆
        /// </summary>
        [Description("Your device has already logged in to an account, please use the existing account to log in")]
        DeviceHasAlreadyUsed = 1024,

        /// <summary>
        /// AppKey不可用
        /// </summary>
        [Description("AppKey not available")]
        AppKeyNotAvailable = 1025,

        /// <summary>
        /// 短信发送失败
        /// </summary>
        [Description("SMS sending failed")]
        SMS_SendingFailed = 1026,

        /// <summary>
        /// 短信模板不可用
        /// </summary>
        [Description("SMS template not available")]
        SMS_TemplateNotAvailable = 1027,

        /// <summary>
        /// 手机号码不可用
        /// </summary>
        [Description("Mobile number not available")]
        MobileNumberNotAvailable = 1028,

        /// <summary>
        /// 账户被锁定
        /// </summary>
        [Description("Account is block")]
        AccountBlock = 1029,

        #endregion

        #region 兑换订单相关
        /// <summary>
        /// 兑换的商品不存在
        /// </summary>
        [Description("The exchanged product does not exist")]
        ItemDoesNotExist = 2001,

        /// <summary>
        /// 购买数量已达上限
        /// </summary>
        [Description("The purchase quantity has reached the maximum")]
        MaximumPurchaseQuantity = 2002,

        /// <summary>
        /// VIP等级不够
        /// </summary>
        [Description("Insufficient VIP level")]
        InsufficientVIPLevel = 2003,

        /// <summary>
        /// 商品库存不足
        /// </summary>
        [Description("Insufficient inventory")]
        InsufficientInventory = 2004,

        /// <summary>
        /// 存在待审核订单
        /// </summary>
        [Description("Orders to be approved")]
        OrdersToBeApproved = 2005,

        #endregion

        /// <summary>
        /// 验证token失败
        /// </summary>
        [Description("verify token error")]
        VerifyTokenError = 3001,

        /// <summary>
        /// 验证签名失败
        /// </summary>
        [Description("verify token error")]
        VerifySignError = 3002,

        /// <summary>
        /// 签名错误
        /// </summary>
        [Description("sign error")]
        SignError = 3003,

        #region CdKeys

        /// <summary>
        /// 兑换码已使用
        /// </summary>
        [Description("Code was used")]
        CodeWasUsed = 4001,

        /// <summary>
        /// 兑换码错误
        /// </summary>
        [Description("Code error")]
        CdkeyCodeError = 4002,

        #endregion


        #region 支付

        /// <summary>
        /// 包名错误
        /// </summary>
        [Description("Bundle error")]
        BundleError = 5001,

        /// <summary>
        /// 找不到商品信息
        /// </summary>
        [Description("Product is not found")]
        ProductNotFound = 5002,

        /// <summary>
        /// 订单重复
        /// </summary>
        [Description("order repeat")]
        OrderRepeat = 5003,

        /// <summary>
        /// 平台错误
        /// </summary>
        [Description("platform error")]
        PlatformError = 5004,

        /// <summary>
        /// 购买上限达到上限
        /// </summary>
        [Description("The purchase limit!")]
        PurchaseLimit = 5005,

        /// <summary>
        /// 有待支付订单
        /// </summary>
        [Description("Outstanding orders")]
        OutstandingOrders = 5006,

        /// <summary>
        /// 未找到付款信息
        /// </summary>
        [Description("Payment information not found")]
        PaymentInformationNotFound = 5007,

        /// <summary>
        /// 余额不足
        /// </summary>
        [Description("Insufficient balance")]
        InsufficientBalance = 5008,

        /// <summary>
        /// 提现达到上限
        /// </summary>
        [Description("Withdrawal limit has been reached.")]
        WithdrawLimit = 5009,

        /// <summary>
        /// 提现之前需要玩局游戏
        /// </summary>
        [Description("You need to play a game before you can withdraw.")]
        NeedPlayGameBeforeWithdraw = 5010,


        /// <summary>
        /// 提现之前需要充值
        /// </summary>
        [Description("Deposit is required before withdrawal.")]
        NeedDepositBeforeWithdraw = 5011,

        /// <summary>
        /// 不满足条件
        /// </summary>
        [Description("does not satisfy the conditions")]
        NotSatisfyConditions = 5012,

        /// <summary>
        /// Vip等级不足
        /// </summary>
        [Description("")]
        InsufficientVipLevel = 5013,


        /// <summary>
        /// 提现次数达到上限
        /// </summary>
        [Description("The number of withdrawals has reached the limit.")]
        WithdrawNumLimit = 5014,

        /// <summary>
        /// 找不到订单
        /// </summary>
        [Description("order not found.")]
        OrderNotFound = 5015,

        #endregion

        /// <summary>
        /// 配置未找到
        /// </summary>
        [Description("config is not found!")]
        ConfigNotFound = 6001,

        /// <summary>
        /// 已在游戏中
        /// </summary>
        [Description("In the game already")]
        InTheGameAlready = 6002,



    }

}
