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

## 提交规范

### Commit Message

格式：

```
[Scope] 简要说明
```

要求：

- 使用简体中文
- 说明动机与影响范围
- 遵循语义化提交
- 禁止拼音 / emoji

------

### Pull Request 要求（重要）

必须包含：

- 背景 & 动机
- 主要变更
- 风险与回滚方案
- 测试说明

### PR 创建规则（关键约束）

- ❗ 创建 Pull Request 时，必须显式指定 `base` 分支
- ❗ `base` 必须是当前工作分支的**上游目标分支**，禁止默认使用 `master` / `main`

执行前必须检查：

```bash
git rev-parse --abbrev-ref HEAD
git branch -vv
```

创建 PR 示例：

```bash
gh pr create --base <目标分支> --head <当前分支>
```

补充规则：

- 若无法自动判断上游分支 → 必须先确认，不允许猜测
- 不允许依赖 `gh` 默认 base 行为
- 必须符合当前仓库的分支合并策略（feature / release / develop 等）

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
