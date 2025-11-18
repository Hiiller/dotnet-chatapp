# 数据库迁移指南

## 重要提示

由于对 Group 实体进行了重大修改，需要重新创建数据库或进行数据迁移。

## 方法 1：删除现有数据库并重新创建（推荐，简单）

### 步骤：

1. 停止后端服务器
2. 删除现有数据库文件：
   ```powershell
   Remove-Item "ChatApp.Server\ChatApp.Server.API\chatapp.db" -ErrorAction SilentlyContinue
   ```

3. 创建新的迁移：
   ```powershell
   cd ChatApp.Server\ChatApp.Server.Infrastructure
   dotnet ef migrations add UpdateGroupStructureAndMembers --startup-project ..\ChatApp.Server.API\ChatApp.Server.API.csproj
   ```

4. 重新启动后端服务器（会自动应用迁移并创建种子数据）：
   ```powershell
   cd ..\ChatApp.Server.API
   dotnet run
   ```

## 方法 2：保留现有数据并迁移（复杂）

### 步骤：

1. 创建迁移：
   ```powershell
   cd ChatApp.Server\ChatApp.Server.Infrastructure
   dotnet ef migrations add UpdateGroupStructureAndMembers --startup-project ..\ChatApp.Server.API\ChatApp.Server.API.csproj
   ```

2. 手动修改生成的迁移文件，添加数据转换逻辑：
   - 为现有群组添加默认的 GroupCode、CreatorId
   - 为现有群组成员创建 GroupMember 记录

3. 应用迁移：
   ```powershell
   cd ..\ChatApp.Server.API
   dotnet ef database update --project ..\ChatApp.Server.Infrastructure\ChatApp.Server.Infrastructure.csproj
   ```

## 新增的数据库表

- **GroupMembers**：用户-群组关系表
  - 字段：Id, GroupId, UserId, Role, JoinedAt
  - 索引：(GroupId, UserId) 唯一索引

- **GroupRequests**：加群请求表
  - 字段：Id, RequesterId, GroupId, CreatedAt, Status
  - 索引：(RequesterId, GroupId, Status)

## Group 表的新字段

- **GroupCode**：8位随机群组代码（用于搜索加入）
- **CreatorId**：群创建者ID
- **CreatedAt**：创建时间

## 数据库结构变更总结

1. Group 表增加 3 个字段
2. 新增 GroupMembers 表（用户-群组多对多关系）
3. 新增 GroupRequests 表（加群请求系统）
4. 创建群组时自动将创建者添加为成员（角色：Creator）

## 验证迁移

启动后端服务器后，检查日志输出，应看到：
```
Seeded default groups.
```

如果没有错误，迁移成功。

