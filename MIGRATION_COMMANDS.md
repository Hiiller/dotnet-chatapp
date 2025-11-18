# 数据库迁移指令

## 快速开始（推荐）

### 1. 删除旧数据库并重新创建

```powershell
# 停止后端服务器（如果正在运行）
Get-Process -Name dotnet -ErrorAction SilentlyContinue | Stop-Process -Force

# 删除旧数据库
Remove-Item "ChatApp.Server\ChatApp.Server.API\chatapp.db" -ErrorAction SilentlyContinue

# 创建新迁移
cd ChatApp.Server\ChatApp.Server.Infrastructure
dotnet ef migrations add UpdateGroupStructureV2 --startup-project ..\ChatApp.Server.API\ChatApp.Server.API.csproj

# 启动后端服务器（会自动应用迁移）
cd ..\ChatApp.Server.API
dotnet run
```

服务器启动后会自动：
- 应用所有迁移
- 创建新的数据库结构
- 生成种子数据（3个默认群组）

### 2. 启动客户端（在新终端）

```powershell
cd ChatApp.Client
dotnet run
```

## 详细步骤说明

### 创建迁移

```powershell
cd ChatApp.Server\ChatApp.Server.Infrastructure
dotnet ef migrations add UpdateGroupStructureV2 --startup-project ..\ChatApp.Server.API\ChatApp.Server.API.csproj
```

### 应用迁移

后端服务器启动时会自动应用，或手动应用：

```powershell
dotnet ef database update --project ChatApp.Server.Infrastructure\ChatApp.Server.Infrastructure.csproj --startup-project ChatApp.Server.API\ChatApp.Server.API.csproj
```

### 回滚迁移（如有问题）

```powershell
dotnet ef migrations remove --project ChatApp.Server.Infrastructure\ChatApp.Server.Infrastructure.csproj --startup-project ChatApp.Server.API\ChatApp.Server.API.csproj
```

## 数据库变更总结

### 新增表

1. **GroupMembers** - 用户-群组关系
   - Id, GroupId, UserId, Role, JoinedAt
   - 唯一索引: (GroupId, UserId)

2. **GroupRequests** - 加群请求
   - Id, RequesterId, GroupId, CreatedAt, Status

### 修改表

**Groups** 表新增字段：
- GroupCode (8位随机代码)
- CreatorId (创建者ID)
- CreatedAt (创建时间)

## 新功能

1. ✅ 群组代码：每个群组有唯一的8位代码用于搜索
2. ✅ 成员管理：用户-群组关系通过 GroupMembers 表管理
3. ✅ 角色系统：Creator（创建者）、Admin（管理员）、Member（成员）
4. ✅ 搜索功能：支持按群组代码搜索
5. ✅ 用户ID显示：搜索结果中显示用户ID
6. ✅ 独立群组：每个用户只看到自己加入的群组

## 验证

启动后检查：
1. 群组列表为空（因为新数据库）
2. 创建新群组成功
3. 群组显示8位代码
4. 搜索功能正常

