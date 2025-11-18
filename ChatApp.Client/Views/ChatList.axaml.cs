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
    private StackPanel? _defaultContentPanel;
    
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
                                // Log current visibility state BEFORE setting content
                                Log("ChatList", $"ContentControl BEFORE: IsVisible={_rightPanelContentControl.IsVisible}, IsEffectivelyVisible={_rightPanelContentControl.IsEffectivelyVisible}, Content={_rightPanelContentControl.Content?.GetType().Name ?? "null"}");
                                
                                // Directly set Content to force ViewLocator to be called
                                _rightPanelContentControl.Content = content;
                                
                                // Force visibility if content is not null
                                if (content != null)
                                {
                                    _rightPanelContentControl.IsVisible = true;
                                    if (_defaultContentPanel != null)
                                    {
                                        _defaultContentPanel.IsVisible = false;
                                    }
                                    Log("ChatList", $"Explicitly set ContentControl IsVisible=true, DefaultPanel IsVisible=false");
                                }
                                else
                                {
                                    _rightPanelContentControl.IsVisible = false;
                                    if (_defaultContentPanel != null)
                                    {
                                        _defaultContentPanel.IsVisible = true;
                                    }
                                    Log("ChatList", $"Explicitly set ContentControl IsVisible=false, DefaultPanel IsVisible=true");
                                }
                                
                                Log("ChatList", $"Directly set ContentControl.Content to {content?.GetType().Name ?? "null"}");
                                
                                // Force layout update immediately
                                _rightPanelContentControl.InvalidateVisual();
                                _rightPanelContentControl.InvalidateMeasure();
                                _rightPanelContentControl.InvalidateArrange();
                                
                                // Force immediate layout pass
                                _rightPanelContentControl.UpdateLayout();
                                
                                // Log final visibility state
                                Log("ChatList", $"ContentControl AFTER: IsVisible={_rightPanelContentControl.IsVisible}, IsEffectivelyVisible={_rightPanelContentControl.IsEffectivelyVisible}, Bounds={_rightPanelContentControl.Bounds}");
                            }, Avalonia.Threading.DispatcherPriority.Normal);
                        }
                    })
                    .DisposeWith(disposables);
            }
        });   
        AvaloniaXamlLoader.Load(this);
        
        // Find the ContentControl and default panel after loading
        _rightPanelContentControl = this.FindControl<ContentControl>("RightPanelContentControl");
        _defaultContentPanel = this.FindControl<StackPanel>("DefaultContentPanel");
        Log("ChatList", $"ContentControl found: {_rightPanelContentControl != null}");
        Log("ChatList", $"DefaultContentPanel found: {_defaultContentPanel != null}");
    }

    
}