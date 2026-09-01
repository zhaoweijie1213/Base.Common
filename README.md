# .NET基础公共方法

- [x] 公告方法

- class批量注入容器实现
- jwt封装
- 统一Api返回格式
- Http日志中间件

- [x] SqlSugar基础类封装
- [x] Consul注册、发现基础操作封装

- http注册、通过注册名调用http服务
- grpc注册、通过注册名调用grpc服务

- [x] Swagger配置封装
- [x] Apollo配置中心集成封装
- [x] 雪花Id集成，redis 自动注册workerId

- 默认雪花Id生成器通过配置文件或者随机生成一个workerId
- redis默认雪花Id生成器从redis里取出workerId保证workerId唯一性

> 
>
> 2026.09.01
>
> 新增 Apollo 原文 Namespace 扩展 AddQYQRawNamespace
>
> 新增 XML 原文按特性映射为对象的扩展 GetXmlItems / ParseXmlItems
>
> 新增可热更新的 XML 配置选项扩展 AddQYQXmlOptions
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
|         DuplicateId         |     3      |                    重复Id                    |
|       RepeatedRequest       |     4      |                  重复的请求                  |
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
|       OrderNotFound        |  5015  |       找不到订单       |

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

### *Apollo 原文 Namespace*

​	`AddQYQRawNamespace` 用于接入**内容无法被内置解析器摊平**的 Namespace，典型场景是策划表格导出的 XML：同名兄弟节点没有 `Name` 属性时，客户端内置的 `XmlConfigAdapter` 会撞键并抛 `FormatException`，导致服务启动即崩。该扩展把整段原文原样放在 `{sectionKey}:content` 上，解析权交回业务侧。

```csharp
builder.Configuration
    .AddQYQApollo()
    .AddQYQRawNamespace("GameList", "GameList");

// 原文按下一节的映射特性解析成对象
var list = builder.Configuration.GetXmlItems<GameListEntry>("GameList");
```

⚠️ **全局副作用**：适配器按格式注册在 Apollo 客户端的**全局静态表**上，调用后该格式（默认 `Xml`）的**所有** Namespace 都不再自动摊平。同一进程内若还有需要自动摊平的同格式 Namespace，请勿使用本方法。

⚠️ `sectionKey` 为必填且不允许空白：原文统一落在 `content` 键上，缺少前缀会污染配置根级，多个原文 Namespace 之间也会互相覆盖。因此**不要**把这类 Namespace 写进 `apollo:Namespaces` 数组（那条路径的 sectionKey 为 null），必须走本扩展显式注册。

### *XML 原文映射*

​	把上面取回的原文转成业务对象。使用者只定义一个 DTO，用 `[XmlAttributeName]` 标出每个属性对应 XML 节点上的哪个属性名（允许中文），然后传泛型 `T` 即可。

```csharp
using QYQ.Base.Common.Extension;
using QYQ.Base.Common.Xml;

public class GameListEntry
{
    [XmlAttributeName("ID")]       public int Id { get; set; }
    [XmlAttributeName("游戏ID")]   public string Title { get; set; } = string.Empty;
    [XmlAttributeName("GameID")]   public int GameId { get; set; }
    [XmlAttributeName("GameType")] public int GameType { get; set; }
    [XmlAttributeName("DistrictID", Required = false)] public int? DistrictId { get; set; }
}
```

对应的 XML（根节点与子节点同名也没关系）：

```xml
<item>
	<item ID="1" 游戏ID="塔城麻将" GameID="2800" GameType="2" DistrictID="654200"/>
	<item ID="2" 游戏ID="营口麻将" GameID="2809" GameType="2" DistrictID="210800"/>
</item>
```

两个入口：

```csharp
// 从配置的 {sectionKey}:content 取原文再解析，sectionKey 与 AddQYQRawNamespace 传的一致
IReadOnlyList<GameListEntry> list = configuration.GetXmlItems<GameListEntry>("GameList");

// 或者直接解析一段原文；第二个参数可只取指定名字的子节点，不传则取根节点下全部子节点
IReadOnlyList<GameListEntry> list2 = xmlContent.ParseXmlItems<GameListEntry>("item");
```

行为约定：

- **只映射 XML 属性（attribute）**，不读取子元素文本，也不支持嵌套对象与集合属性
- **未标注 `[XmlAttributeName]` 的属性不参与映射**，也不做任何校验
- `Required` 默认为 `true`，节点上缺该属性即抛；`Required = false` 时缺失或留空保留 CLR 默认值（可空类型为 null）
- `Required` 只管「缺不缺」。属性写了但值转换不了（如 `GameID="abc"`），**无论 `Required` 如何都抛**
- 支持的类型：`string`、`bool`、各整数与浮点类型、`decimal`、`DateTime`、`DateTimeOffset`、`TimeSpan`、`Guid`、枚举（名称或数值），以及以上的可空形式。`bool` 额外接受 `1`/`0`；数值与时间一律按 `InvariantCulture` 解析。不受支持的类型在**建立映射时**就抛，不会等到跑数据
- 原文为空（Apollo 尚未下发内容）→ 返回空集合，**不抛**
- 所有失败统一抛 `XmlContentParseException`，携带 `LineNumber` / `LinePosition` / `ItemIndex` / `AttributeName` / `RawValue` / `TargetType`，调用方 catch 一种即可沿用上一份可用快照

### *XML 配置选项（热更新）*

​	`AddQYQXmlOptions<T>` 把上面的解析接成可热更新的配置选项，使用方不用再自己定义 Options 类：

```csharp
// 注册
builder.Configuration.AddQYQApollo().AddQYQRawNamespace("base-game-list", "BaseGameList");
builder.Services.AddQYQXmlOptions<GameListEntry>("BaseGameList");

// 注入
public class GameListService(IOptionsMonitor<XmlConfigOptions<GameListEntry>> options)
{
    public IReadOnlyList<GameListEntry> Items => options.CurrentValue.Items;
}
```

⚠️ 读取端**必须**用 `IOptionsMonitor<T>`（单例可用）或 `IOptionsSnapshot<T>`（请求内可用）。`IOptions<T>` 是单例快照，Apollo 推送再多次也拿不到新值。

为什么要把这一步收进库里：原文是单个字符串键，`Bind` 绑不出集合，只能走 `AddOptions().Configure(委托)`；而 `Configure` 委托**不像 `Bind` 那样自带配置变更令牌**，少注册一个 `IOptionsChangeTokenSource<T>`，`IOptionsMonitor` 的缓存就永不失效——编译通过、运行不报错，只是热更新静默失效。本方法把这两步一起做掉。

**本方法不做容错。** 原文畸形时 `CurrentValue` 会抛 `XmlContentParseException`，且每次访问都抛。需要「解析失败沿用旧快照」或「条数为 0 视为故障」的场景，请在业务侧自建单例目录服务，订阅 `ChangeToken.OnChange(configuration.GetReloadToken, ...)` 自行重算——这类判断是业务决策，不进通用库。

同一个 `T` 只应注册一次；重复注册不同 sectionKey 时后者会覆盖前者。

## QYQ.Base.Consul

​	consul扩展方法







## QYQ.Base.SnowId

​	雪花id生成
