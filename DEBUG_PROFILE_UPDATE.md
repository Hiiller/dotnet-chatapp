# Profile DisplayName 更新问题排查指南

## 已添加的调试日志

我已经在以下位置添加了详细的调试日志：

1. **EditProfileViewModel.SaveCommand** - 记录保存操作
2. **ChatService.UpdateProfile** - 记录 HTTP PUT 请求和响应
3. **ChatService.GetProfile** - 记录 HTTP GET 请求和响应
4. **ProfileViewModel.LoadProfileAsync** - 记录从数据库加载的数据
5. **ProfileViewModel.DisplayName setter** - 记录属性更新
6. **ProfileViewModel.ProfileUpdated event handler** - 记录事件触发
7. **ProfileView.WhenActivated** - 记录视图激活

## 排查步骤

### 1. 检查数据库是否更新

**操作：** 修改个人资料后，查看控制台输出

**查找：** `[EditProfileViewModel] UpdateProfile succeeded!`
- 检查返回的 `DisplayName` 和 `Bio` 是否正确
- 如果返回的值不对，说明后端更新失败

**查找：** `[ChatService] UpdateProfile response body:`
- 查看服务器返回的 JSON，确认数据库已更新

### 2. 检查事件是否触发

**操作：** 保存后，查看控制台输出

**查找：** `[EditProfileViewModel] Raising ProfileUpdated event`
- 确认事件被触发
- 检查传递的 `userId` 和 `displayName` 是否正确

**查找：** `[ProfileViewModel] ProfileUpdated event received`
- 确认 ProfileViewModel 收到了事件
- 检查 `userId` 是否匹配

### 3. 检查数据加载

**操作：** 打开个人资料界面，查看控制台输出

**查找：** `[ProfileView] WhenActivated`
- 确认视图激活时执行了加载

**查找：** `[ProfileViewModel] LoadProfileAsync called`
- 确认加载方法被调用

**查找：** `[ChatService] GetProfile`
- 检查 HTTP 请求是否成功
- 检查返回的 `DisplayName` 值

**查找：** `[ProfileViewModel] GetProfile returned:`
- 检查从数据库读取的值是否正确

### 4. 检查属性更新

**查找：** `[ProfileViewModel] DisplayName property changing`
- 确认属性 setter 被调用
- 检查旧值和新值

**查找：** `[ProfileViewModel] DisplayName property set to same value`
- 如果看到这个，说明值没有变化，可能是数据库没有更新

## 常见问题检查点

### 问题 1: 数据库没有更新
**症状：** `UpdateProfile` 返回的值和发送的值不一致
**解决：** 检查后端 API 实现

### 问题 2: 事件没有触发
**症状：** 看不到 `ProfileUpdated event received`
**解决：** 检查事件订阅是否正确

### 问题 3: 加载时机不对
**症状：** `LoadProfileAsync` 在事件更新之前执行
**解决：** 检查事件处理中的 `LoadProfileAsync` 调用

### 问题 4: 属性没有更新
**症状：** 看到 `DisplayName property set to same value`
**解决：** 检查数据库返回的值是否真的改变了

### 问题 5: ViewModel 实例不同
**症状：** 事件中的 `userId` 和当前 `UserId` 不匹配
**解决：** 可能是创建了新的 ViewModel 实例

## 测试流程

1. **启动应用**，打开控制台
2. **打开个人资料界面**，记录初始值
3. **修改昵称和个性签名**，点击保存
4. **查看控制台输出**，按照上述检查点逐一排查
5. **返回个人资料界面**，检查是否更新

## 关键日志标记

- `[EditProfileViewModel]` - 编辑界面相关
- `[ChatService]` - HTTP 请求相关
- `[ProfileViewModel]` - 个人资料 ViewModel 相关
- `[ProfileView]` - 个人资料视图相关

## 下一步

运行应用，执行上述测试流程，然后将控制台输出发给我，我可以帮你分析问题所在。

