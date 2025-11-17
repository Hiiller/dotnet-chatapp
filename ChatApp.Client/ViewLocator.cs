// ChatApp.Client/ViewLocator.cs
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;
using static ChatApp.Client.Helpers.DebugLogger;

namespace ChatApp.Client;

public class ViewLocator : IDataTemplate
{

    public Control? Build(object? param)
    {
        try
        {
            if (param is null)
            {
                Log("ViewLocator", "Build: param is null");
                return null;
            }
            
            var viewModelName = param.GetType().Name;
            var viewName = viewModelName.Replace("ViewModel", "View", StringComparison.Ordinal);
            var fullViewName = param.GetType().FullName!.Replace("ViewModels", "Views").Replace("ViewModel", "View");
            
            Log("ViewLocator", $"Build: ViewModel: {viewModelName}, Looking for view: {fullViewName}");
            
            // Try full name first
            var type = Type.GetType(fullViewName);
            
            // If not found, try searching in the same assembly
            if (type == null)
            {
                var assembly = param.GetType().Assembly;
                Log("ViewLocator", $"Build: Trying assembly.GetType with full name: {fullViewName}");
                type = assembly.GetType(fullViewName);
                
                if (type == null)
                {
                    var shortName = $"ChatApp.Client.Views.{viewName}";
                    Log("ViewLocator", $"Build: Trying assembly.GetType with short name: {shortName}");
                    type = assembly.GetType(shortName);
                }
                
                // Last resort: search all types in assembly
                if (type == null)
                {
                    Log("ViewLocator", $"Build: Searching all types in assembly for: {viewName}");
                    type = assembly.GetTypes().FirstOrDefault(t => t.Name == viewName && t.Namespace?.Contains("Views") == true);
                }
            }

            if (type != null)
            {
                Log("ViewLocator", $"Build: Found view type: {type.FullName}");
                try
                {
                    var instance = Activator.CreateInstance(type);
                    if (instance is Control control)
                    {
                        // Set DataContext - Avalonia will handle this automatically via ContentControl,
                        // but we set it explicitly to ensure it's correct
                        control.DataContext = param;
                        Log("ViewLocator", $"Build: Successfully created {type.Name}, DataContext set to {param.GetType().Name}");
                        return control;
                    }
                    else
                    {
                        Log("ViewLocator", $"Build: Created instance is not a Control: {instance?.GetType().Name}");
                    }
                }
                catch (Exception createEx)
                {
                    Log("ViewLocator", $"Build: Error creating instance: {createEx.Message}");
                    Log("ViewLocator", $"Build: StackTrace: {createEx.StackTrace}");
                }
            }
            else
            {
                Log("ViewLocator", $"Build: View type not found for: {viewName} (searched: {fullViewName})");
            }
            
            return new TextBlock { Text = "Not Found: " + viewName, Foreground = Avalonia.Media.Brushes.Red };
        }
        catch (Exception ex)
        {
            Log("ViewLocator", $"Build ERROR: {ex.Message}");
            Log("ViewLocator", $"Build: StackTrace: {ex.StackTrace}");
            return new TextBlock { Text = $"Error: {ex.Message}" };
        }
    }

    public bool Match(object? data)
    {
        if (data == null)
        {
            Log("ViewLocator", "Match: data is null, returning false");
            return false;
        }
        
        var result = data is ViewModelBase || data?.GetType().Name.EndsWith("ViewModel") == true;
        Log("ViewLocator", $"Match: data type={data.GetType().Name}, is ViewModelBase={data is ViewModelBase}, ends with ViewModel={data.GetType().Name.EndsWith("ViewModel")}, result={result}");
        return result;
    }
}
