using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ATM.UI.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace ATM.UI
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        private static readonly ConcurrentDictionary<string, Type?> TypeCache = new ConcurrentDictionary<string, Type?>();
        private static readonly string[] ViewSuffixes = { "View", "Page", "Window", "Control" };
        
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            try
            {
                var viewType = FindViewType(param.GetType());
                
                if (viewType != null && Activator.CreateInstance(viewType) is Control control)
                {
                    return control;
                }
                
                return CreateNotFoundMessage(param.GetType().Name);
            }
            catch (Exception ex)
            {
                return CreateErrorMessage(param.GetType().Name, ex.Message);
            }
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }

        private static Type? FindViewType(Type viewModelType)
        {
            var typeName = viewModelType.FullName;
            if (string.IsNullOrEmpty(typeName))
                return null;
            
            return TypeCache.GetOrAdd(typeName, _ =>
            {
                foreach (var suffix in ViewSuffixes)
                {
                    var viewName = typeName.Replace("ViewModel", suffix, StringComparison.Ordinal);
                    var viewType = Type.GetType(viewName);
                    
                    if (viewType != null && viewType.IsAssignableTo(typeof(Control)))
                        return viewType;
                }
                
                return null;
            });
        }

        private static TextBlock CreateNotFoundMessage(string viewModelName)
        {
            return new TextBlock 
            { 
                Text = $"View not found for: {viewModelName}",
                Foreground = Avalonia.Media.Brushes.Red
            };
        }

        private static TextBlock CreateErrorMessage(string viewModelName, string errorMessage)
        {
            return new TextBlock 
            { 
                Text = $"Error creating view for {viewModelName}: {errorMessage}",
                Foreground = Avalonia.Media.Brushes.Red
            };
        }
    }
}
