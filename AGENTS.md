# Project Rules (.NET)

## 基础信息

- 技术栈：.NET，ASP.NET Core（WebAPI / gRPC / Worker）
- 供其他项目使用的Nuget组件

---

## 必须执行（最高优先级）

每次修改代码后必须执行：

```bash
dotnet build -c Release
dotnet test
```

禁止提交：

- 编译失败代码
- 测试失败代码

------

## 核心约束（不可违反）

- ❗ API 必须保持向后兼容（OpenAPI 不得破坏）
- ❗ 所有接口统一返回 `ApiResult<T>`
- ❗ Application 层 Service 也必须返回 `ApiResult<T>`
- ❗ 修改代码必须补充或更新测试
- ❗ 不允许引入破坏现有结构的大规模重构

------

## 依赖注入（DI 规范）

- 优先使用：`QYQ.Base.Common.IOCExtensions`
- 若不满足需求，才允许手动注册
- 使用构造函数注入（主构造函数语法）
- 禁止使用静态单例共享状态
- 保持与现有项目注册风格一致

------

## Coding Convention（必须遵守）

生成任何 C# / .NET 代码时：

### 语言规范（全局规则）

- 所有用户可见文本必须使用 **简体中文**
- 代码注释必须使用 **简体中文**
- 专业术语必须使用 **English**（如：API, DTO, Entity, Service）

### 注释要求

- 类必须包含用途说明
- 方法必须包含 XML Summary 注释
- 参数必须有说明
- 返回值必须说明含义
- 注释必须解释“业务意图”，而不是重复代码

------

## 模型与命名规范

### DTO 规范

- 输出模型：`XXXOutput` → `Models/Output`
- 输入模型：`XXXInput` → `Models/Input`

### 命名规则

- Domain Model：名词（Order, User, Position）
- Service / UseCase：动宾结构（CreateOrder, GetUserPosition）

### 禁止

- Utils / Helper 类
- God Class（过大类）

------

## JSON / 配置 / 日志

### JSON

- 使用：`Newtonsoft.Json`

### 配置

- 使用：
  - `appsettings.json`
- 禁止提交 Secrets

### 日志性能规范（非常重要）

在输出 `Debug` / `Information` 级别日志时，必须遵守：

- ❗ 先调用 `logger.IsEnabled(LogLevel.Debug/Information)` 判断日志级别是否开启
- ❗ 避免在日志未开启时执行字符串拼接、对象序列化等高开销操作

------

## 测试要求

- 新增或修改代码必须补充单元测试
- 优先覆盖核心业务逻辑
- 不允许跳过关键路径测试

------

## 工作原则

- 优先复用现有代码
- 避免重复实现
- 保持代码风格一致
- 不引入不必要的第三方库
- 不随意改变已有架构

------

## 执行安全规则（补充）

- 对会影响仓库状态的操作保持谨慎（PR、分支、CI 配置等）
- 执行命令前优先检查上下文，而不是直接使用默认参数
- 输出命令时必须完整，不省略关键参数

------

## 如果不确定如何处理

按以下优先级决策：

1. 查看 `SKILL.md`
2. 保持与现有代码一致
3. 选择最简单、最安全的实现
4. 不要自行引入新架构或新模式
