using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;
using ReactiveUI;
using Shared.Models;
using static ChatApp.Client.Helpers.DebugLogger;

namespace ChatApp.Client.Views;

public partial class ChatList : ReactiveUserControl<ChatListModel>
{
    private ContentControl? _rightPanelContentControl;
    
    public ChatList()
    {
        InitializeComponent();
    }
    

    private void InitializeComponent()
    {
        this.WhenActivated(disposables => 
        { 
            if (ViewModel != null)
            {
                // Subscribe to RightPanelContent changes to force ContentControl update
                ViewModel.WhenAnyValue(x => x.RightPanelContent)
                    .Subscribe(content =>
                    {
                        Log("ChatList", $"WhenAnyValue RightPanelContent: type={content?.GetType().Name ?? "null"}");
                        
                        // Force ContentControl to refresh by directly setting Content
                        // This bypasses the binding and forces immediate update
                        if (_rightPanelContentControl != null)
                        {
                            // Use InvokeAsync with higher priority to ensure immediate execution
                            _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                // Directly set Content to force ViewLocator to be called
                                _rightPanelContentControl.Content = content;
                                Log("ChatList", $"Directly set ContentControl.Content to {content?.GetType().Name ?? "null"}");
                                
                                // Force layout update immediately
                                _rightPanelContentControl.InvalidateVisual();
                                _rightPanelContentControl.InvalidateMeasure();
                                _rightPanelContentControl.InvalidateArrange();
                                
                                // Force immediate layout pass
                                _rightPanelContentControl.UpdateLayout();
                            }, Avalonia.Threading.DispatcherPriority.Normal);
                        }
                    })
                    .DisposeWith(disposables);
            }
        });   
        AvaloniaXamlLoader.Load(this);
        
        // Find the ContentControl after loading
        _rightPanelContentControl = this.FindControl<ContentControl>("RightPanelContentControl");
        Log("ChatList", $"ContentControl found: {_rightPanelContentControl != null}");
    }

    
}