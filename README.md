# 基础公共方法

> 
>
> 2023.01.05
>
> 添加apollo扩展方法
>
> 添加SqlSuagr数据仓储基类
>
> 2022.12.19
>
> 添加SwaggerUI,Api版本控制公共方法
>
> 2022.05.25
>
> 添加雪花id生成nuget
>
> 2022.04.27
>
> 新增兑换订单相关的返回码
>
> 2022.04.25
>
> 新增兑换订单相关的返回码
>
> 2022.01.17
>
> consul新增gRPC服务注册
>
> 2021.09.27
>
> 新增consul扩展方法
>
> 2021.09.02
>
> 新增日志扩展方法
>
> 2021.08.26
>
> 新增返回码
>
> 2021.06.07
>
> 内部服务错误返回码为500
>
> 2021.05.12
>
> 添加 IServiceCollection 扩展，批量注入



## QYQ.Base.Common

### *ApiResult*

​	统一api返回结果格式

​	推送nuget包

```shell
dotnet nuget push -s http://192.168.0.252/v3/index.json QYQ.Base.Common.1.0.0.nupkg -k 1234567890
```



### PageResult

返回分页数据



#### ApiResultCode 

​	返回结果说明

> 1000  登录相关
>
> 

|             Key             | 返回结果码 |                     描述                     |
| :-------------------------: | :--------: | :------------------------------------------: |
|           Success           |     0      |                   操作成功                   |
|     InternalServerError     |    500     |                 内部服务错误                 |
|         ErrorParams         |     2      |                   参数错误                   |
|            Fail             |     -1     |                   操作失败                   |
|         TokenError          |    1004    |                  token错误                   |
|      TokenTimeOutError      |    1005    |                  token失效                   |
|        UserNotFound         |    1006    |                 未找到该用户                 |
|     OtherPlatformError      |    1007    |                 其他平台错误                 |
|       AccountRelation       |    1008    |              已有账户,需要绑定               |
|        SmsCodeError         |    1010    |                  验证码错误                  |
|        SmsSendError         |    1012    |                验证码发送失败                |
|        NotBindError         |    1013    |                未绑定手机号码                |
|      PhoneAlreadyUsed       |    1014    |           电话号码已经被其它人绑定           |
|          PwdError           |    1015    |                   密码错误                   |
|     AccountTakeupError      |    1016    |                  帐号被占用                  |
|       LoginErrorLimit       |    1017    |             密码错误次数达到上限             |
|        FaceBookUsed         |    1018    |               facebook已被绑定               |
|   LoginWithGuestDisabled    |    1019    |             不能使用游客模式登录             |
|     BackupAccountFailed     |    1020    |               账号信息备份失败               |
|     DeleteAccountFailed     |    1021    |               账号信息删除失败               |
| DeleteFacebookAccountFailed |    1022    |           删除关联Facebook信息失败           |
|       ExpirationToken       |    1023    |                  过期token                   |
|    DeviceHasAlreadyUsed     |    1024    | 设备已登陆过一个账号，请使用现有账号进行登陆 |
|     AppKeyNotAvailable      |    1025    |                 AppKey不可用                 |
|      SMS_SendingFailed      |    1026    |                 短信发送失败                 |
|  SMS_TemplateNotAvailable   |    1027    |                短信模板不可用                |
|  MobileNumberNotAvailable   |    1028    |                手机号码不可用                |
|        AccountBlock         |    1029    |                  账户被锁定                  |

>2000  兑换订单相关
>
>

|           Key           | 返回码 |       描述       |
| :---------------------: | :----: | :--------------: |
|    ItemDoesNotExist     |  2001  | 兑换的商品不存在 |
| MaximumPurchaseQuantity |  2002  | 购买数量已达上限 |
|  InsufficientVIPLevel   |  2003  |   VIP等级不够    |
|  InsufficientInventory  |  2004  |   商品库存不足   |
|   OrdersToBeApproved    |  2005  |  存在待审核订单  |



> 3000



|       Key        | 返回码 |     描述      |
| :--------------: | :----: | :-----------: |
| VerifyTokenError |  3001  | 验证token失败 |
| VerifySignError  |  2002  | 验证签名失败  |
|    SignError     |  2003  |   签名错误    |

> Cd Keys

|      Key       | 返回码 |     描述     |
| :------------: | :----: | :----------: |
|  CodeWasUsed   |  4001  | 兑换码已使用 |
| CdkeyCodeError |  4002  |  兑换码错误  |

> 支付

|            Key             | 返回码 |          描述          |
| :------------------------: | :----: | :--------------------: |
|        BundleError         |  5001  |        包名错误        |
|      ProductNotFound       |  5002  |     找不到商品信息     |
|        OrderRepeat         |  5003  |        订单重复        |
|       PlatformError        |  5004  |        平台错误        |
|       PurchaseLimit        |  5005  |    购买上限达到上限    |
|     OutstandingOrders      |  5006  |      有待支付订单      |
| PaymentInformationNotFound |  5007  |     未找到付款信息     |
|    InsufficientBalance     |  5008  |        余额不足        |
|       WithdrawLimit        |  5009  |    提现额度达到上限    |
| NeedPlayGameBeforeWithdraw |  5010  | 提现之前需要玩一局游戏 |
| NeedDepositBeforeWithdraw  |  5011  |    提现之前需要充值    |
|    NotSatisfyConditions    |  5012  |       不满足条件       |
|    InsufficientVipLevel    |  5013  |      Vip等级不足       |
|      WithdrawNumLimit      |  5014  |    提现次数达到上限    |

|       Key        | 返回码 |    描述    |
| :--------------: | :----: | :--------: |
|  ConfigNotFound  |  6001  | 配置未找到 |
| InTheGameAlready |  6002  | 已在游戏中 |
|                  |        |            |



### *Extension*

​	静态扩展方法

- EnumExtension 枚举扩展方法
- IOCExtensions 自定义容器批量注入方法
- LoggerExtensions日志扩展方法

## QYQ.Base.Consul

​	consul扩展方法







## QYQ.Base.SnowId

​	雪花id生成
