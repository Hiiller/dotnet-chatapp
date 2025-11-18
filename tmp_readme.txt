# ChatApp 项目说明

跨平台即时通信套件，后端基于 **ASP.NET Core 9 + SignalR + EF Core (SQLite)**，前端使用 **Avalonia 11 + ReactiveUI**。系统覆盖私聊、群聊、好友申请、群管理员审批、通知与主题定制、安全问题找回密码等完整功能。

---

## 目录
1. [整体架构](#整体架构)
2. [功能导航](#功能导航)
   - [消息系统](#消息系统)
   - [群组管理](#群组管理)
   - [好友体系](#好友体系)
   - [安全与隐私](#安全与隐私)
   - [个性化体验](#个性化体验)
3. [运行项目](#运行项目)
4. [附加文档](#附加文档)

---

## 整体架构

### 服务器（ChatApp.Server）
- **启动/依赖注册**：配置 Controller、SignalR、SQLite、静态文件、自动迁移以及 `wwwroot/uploads` 初始化。[Program.cs](ChatApp.Server/ChatApp.Server.API/Program.cs)
- **控制器层**：
  - `ChatController` 负责认证、历史记录、未读统计、消息写入。[ChatApp.Server/ChatApp.Server.API/Controllers/ChatController.cs](ChatApp.Server/ChatApp.Server.API/Controllers/ChatController.cs)
  - `GroupsController` 覆盖群创建、搜索、详情、入群审批、描述更新、成员管理。[ChatApp.Server/ChatApp.Server.API/Controllers/GroupsController.cs](ChatApp.Server/ChatApp.Server.API/Controllers/GroupsController.cs)
  - `FriendRequestController` 提供好友申请、查询、审批接口。[ChatApp.Server/ChatApp.Server.API/Controllers/FriendRequestController.cs](ChatApp.Server/ChatApp.Server.API/Controllers/FriendRequestController.cs)
  - `UserController` 处理资料、头像、安全问题、密码修改。[ChatApp.Server/ChatApp.Server.API/Controllers/UserController.cs](ChatApp.Server/ChatApp.Server.API/Controllers/UserController.cs)
  - `FilesController` 处理文件上传并返回公共 URL。[ChatApp.Server/ChatApp.Server.API/Controllers/FilesController.cs](ChatApp.Server/ChatApp.Server.API/Controllers/FilesController.cs)
- **实时通信**：`ChatHub` 管理 SignalR 连接、组成员与即时消息推送。[ChatApp.Server/ChatApp.Server.API/Hubs/ChatHub.cs](ChatApp.Server/ChatApp.Server.API/Hubs/ChatHub.cs)
- **业务与数据层**：`Application`/`Infrastructure` 子项目包含 `UserService`、仓储、`AppDbContext`、迁移脚本，负责业务规则和 EF 配置。

### 客户端（ChatApp.Client）
- **统一服务**：`ChatService` 封装所有 REST/SignalR 调用，包含认证、消息、好友、群、资料、安全问题与文件上传。[ChatApp.Client/Services/ChatService.cs](ChatApp.Client/Services/ChatService.cs)
- **基础设施**：`NotificationSettingsService`（通知偏好）、`MessageSoundPlayer`（提示音）、`ThemeManager`（主题切换）。
- **MVVM 视图**：`ChatList`、`ChatView`、`GroupDetailView`、`ProfileView`、`Settings`/`Dialog` 等 XAML 均配套 ViewModel，ReactiveCommand 驱动逻辑。

---

## 功能导航

### 消息系统
- **登录/注册/上下文管理**：`ChatController` 的登录注册接口负责返回用户身份并设置上下文。[ChatApp.Server/ChatApp.Server.API/Controllers/ChatController.cs#L34-L176](ChatApp.Server/ChatApp.Server.API/Controllers/ChatController.cs#L34-L176)
- **历史记录 & 未读同步**：同一控制器集中提供私聊/群聊历史、未读统计、离线写入、已读回执。[ChatApp.Server/ChatApp.Server.API/Controllers/ChatController.cs#L177-L364](ChatApp.Server/ChatApp.Server.API/Controllers/ChatController.cs#L177-L364)
- **客户端消息流**：`ChatViewModel` 维护 SignalR 连接、消息集合、输入命令；`ChatView.axaml` 负责 UI 与自动滚动。[ChatApp.Client/ViewModels/ChatViewModel.cs](ChatApp.Client/ViewModels/ChatViewModel.cs)｜[ChatApp.Client/Views/ChatView.axaml](ChatApp.Client/Views/ChatView.axaml)
- **附件上传**：`FilesController` 校验扩展名和大小，返回可访问 URL；`ChatService.UploadFileAsync` 供 UI 直接调用。[ChatApp.Server/ChatApp.Server.API/Controllers/FilesController.cs](ChatApp.Server/ChatApp.Server.API/Controllers/FilesController.cs)｜[ChatApp.Client/Services/ChatService.cs#L685-L782](ChatApp.Client/Services/ChatService.cs#L685-L782)

### 群组管理
- **群列表/搜索/创建**：用户可查看自己的群、按群码搜索、创建新群，结果附带成员数和入群状态。[ChatApp.Server/ChatApp.Server.API/Controllers/GroupsController.cs#L22-L169](ChatApp.Server/ChatApp.Server.API/Controllers/GroupsController.cs#L22-L169)
- **入群申请 & 审批**：提交申请、查询待审批、管理员审批、创建者统一待办全部由 `GroupsController` 完成。[ChatApp.Server/ChatApp.Server.API/Controllers/GroupsController.cs#L171-L463](ChatApp.Server/ChatApp.Server.API/Controllers/GroupsController.cs#L171-L463)
- **群详情视图**：`GroupDetailsViewModel` 实时刷新描述、成员数、角色权限，允许创建者修改介绍、批准/拒绝请求、移除成员。[ChatApp.Client/ViewModels/GroupDetailsViewModel.cs](ChatApp.Client/ViewModels/GroupDetailsViewModel.cs)｜[ChatApp.Client/Views/GroupDetailView.axaml](ChatApp.Client/Views/GroupDetailView.axaml)
- **群列表入口**：`ChatListModel` 在“群组”标签展示创建者未处理的入群请求卡片，并绑定审批命令。[ChatApp.Client/ViewModels/ChatListModel.cs](ChatApp.Client/ViewModels/ChatListModel.cs)

### 好友体系
- **好友申请 API**：`FriendRequestController` 支持发送、查询待办、批准/拒绝好友请求以及用户搜索。[ChatApp.Server/ChatApp.Server.API/Controllers/FriendRequestController.cs](ChatApp.Server/ChatApp.Server.API/Controllers/FriendRequestController.cs)
- **服务端校验**：`UserService` 在好友流程中防止重复好友、重复申请，并在批准后自动建立双向关系。[ChatApp.Server/ChatApp.Server.Application/Services/UserService.cs#L223-L245](ChatApp.Server/ChatApp.Server.Application/Services/UserService.cs#L223-L245)
- **客户端体验**：`ChatService` 暴露 `SendFriendRequestAsync`、`GetPendingFriendRequestsAsync`、`RespondFriendRequestAsync`、`SearchUsersAsync`；UI 在 `ChatList.axaml` 的“好友”分区展示卡片与操作按钮。[ChatApp.Client/Services/ChatService.cs#L784-L881](ChatApp.Client/Services/ChatService.cs#L784-L881)｜[ChatApp.Client/Views/ChatList.axaml](ChatApp.Client/Views/ChatList.axaml)

### 安全与隐私
- **资料 / 头像**：`UserController` 与 `ChatService` 提供资料读取、更新、头像上传接口；`EditProfileDialog` 负责 UI。[ChatApp.Server/ChatApp.Server.API/Controllers/UserController.cs#L19-L76](ChatApp.Server/ChatApp.Server.API/Controllers/UserController.cs#L19-L76)｜[ChatApp.Client/Views/EditProfileDialog.axaml](ChatApp.Client/Views/EditProfileDialog.axaml)
- **安全问题维护**：用户可设置「家在哪个城市」「最喜欢的动物」「父亲/母亲的名字」三问，客户端在隐私设置中编辑。[ChatApp.Server/ChatApp.Server.API/Controllers/UserController.cs#L77-L116](ChatApp.Server/ChatApp.Server.API/Controllers/UserController.cs#L77-L116)｜[ChatApp.Client/Views/PrivacySettingsView.axaml](ChatApp.Client/Views/PrivacySettingsView.axaml)
- **忘记密码**：`RecoverPasswordViewModel` 通过 `ChatService.RecoverPasswordAsync` 验证答案并显示当前密码。[ChatApp.Client/ViewModels/RecoverPasswordViewModel.cs](ChatApp.Client/ViewModels/RecoverPasswordViewModel.cs)｜[ChatApp.Client/Views/RecoverPasswordView.axaml](ChatApp.Client/Views/RecoverPasswordView.axaml)
- **修改密码**：`SecurityDialog` 绑定 `SecurityViewModel`，调用 `UserService.ChangePasswordAsync` 完成密码更新并支持关闭命令。[ChatApp.Client/Views/SecurityDialog.axaml](ChatApp.Client/Views/SecurityDialog.axaml)｜[ChatApp.Client/ViewModels/SecurityViewModel.cs](ChatApp.Client/ViewModels/SecurityViewModel.cs)

### 个性化体验
- **通知设置与提示音**：`NotificationSettingsViewModel`/`NotificationSettingsService` 持久化本地偏好；`MessageSoundPlayer` 使用 `Assets/messagering.wav` 播放提示音。[ChatApp.Client/ViewModels/NotificationSettingsViewModel.cs](ChatApp.Client/ViewModels/NotificationSettingsViewModel.cs)｜[ChatApp.Client/Services/NotificationSettingsService.cs](ChatApp.Client/Services/NotificationSettingsService.cs)
- **主题与字体**：`AppearanceSettingsView` 搭配 `ThemeManager` 切换不同 ThemeVariant/字体，提供视觉配置入口。[ChatApp.Client/Views/AppearanceSettingsView.axaml](ChatApp.Client/Views/AppearanceSettingsView.axaml)｜[ChatApp.Client/Services/ThemeManager.cs](ChatApp.Client/Services/ThemeManager.cs)
- **资料页快捷操作**：`ProfileView` 展示亮点速览、分享名片 (`ShareCardDialog`)、快速切换主题等 UI，文本已全部英文化避免乱码。[ChatApp.Client/Views/ProfileView.axaml](ChatApp.Client/Views/ProfileView.axaml)｜[ChatApp.Client/Views/ShareCardDialog.axaml](ChatApp.Client/Views/ShareCardDialog.axaml)

---

## 运行项目

### 环境要求
- .NET 9 SDK
- SQLite（EF Core 启动时自动创建 `chatapp.db`，无需额外配置）

### 启动步骤
1. **还原与构建**
   ```bash
   dotnet restore
   dotnet build
   ```
2. **执行迁移（首次运行）**
   ```bash
   dotnet ef database update \
     --project ChatApp.Server/ChatApp.Server.Infrastructure/ChatApp.Server.Infrastructure.csproj \
     --startup-project ChatApp.Server/ChatApp.Server.API/ChatApp.Server.API.csproj
   ```
3. **运行后端**
   ```bash
   dotnet run --project ChatApp.Server/ChatApp.Server.API/ChatApp.Server.API.csproj
   ```
4. **运行客户端**
   ```bash
   dotnet run --project ChatApp.Client/ChatApp.Client.csproj
   ```
5. 在 Welcome 页面登录（使用种子账号或自行注册）。客户端默认指向 `https://localhost:5001`，若修改后端端口，可在 `WelcomeViewModel` 中调整。

---

## 附加文档
- [`CODE_NAVIGATION.md`](CODE_NAVIGATION.md)：完整代码导航，按服务器/客户端拆分并附 GitHub 风格锚点。
- [`NEW_FEATURES_GROUP_FRIEND.md`](NEW_FEATURES_GROUP_FRIEND.md)：“group and friend” 分支新增特性速览。

如需演示或代码讲解，可结合上述文档与「功能导航」中的链接快速定位源码，欢迎在对应分支提交 Issue 或 PR 推进功能。

