using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn about building custom controls see 
// https://learn.microsoft.com/windows/apps/winui/winui3/xaml-templated-controls-csharp-winui-3

namespace UnoScrollReveal.Presentation
{
    public sealed partial class CustomListView : ListView, IDisposable
    {
        public CustomListView()
        {
            this.DefaultStyleKey = typeof(CustomListView);
            
        }

        public static readonly DependencyProperty LoadMoreCommandProperty =
                  DependencyProperty.Register(nameof(LoadMoreCommand), typeof(ICommand), typeof(CustomListView), new PropertyMetadata(default(ICommand)));

        public ICommand LoadMoreCommand
        {
            get { return (ICommand)GetValue(LoadMoreCommandProperty); }
            set { SetValue(LoadMoreCommandProperty, value); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
