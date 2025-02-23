using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SystemChecker.WPF.ViewModels;

namespace SystemChecker.WPF.Views.Controls
{
    public partial class NetworkTabControl : UserControl
    {
        public NetworkTabControl()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is NetworkViewModel vm)
                {
                    Debug.WriteLine($"NetworkTabControl DataContext updated: {vm.NetworkStatus?.IsConnected}");
                }
            };
        }
    }
} 