using ReactiveUI;
using System;
using System.Windows.Input;
using ChatApp.Client.Helpers;

namespace ChatApp.Client.Models
{
    public class UserModel : ReactiveObject // 继承 ReactiveObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;

        public ICommand ButtonCommand { get; set; } = new RelayCommand(_ => { });

        private string _backgroundColor = "#0078D7"; // 默认背景颜色

        // 使用 ReactiveObject 的 RaiseAndSetIfChanged 方法
        public string BackgroundColor
        {
            get => _backgroundColor;
            set => this.RaiseAndSetIfChanged(ref _backgroundColor, value); // 会自动触发 UI 更新
        }
    }
}