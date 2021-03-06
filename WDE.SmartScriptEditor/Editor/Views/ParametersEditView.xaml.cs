﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WDE.SmartScriptEditor.Editor.Views
{
    /// <summary>
    /// Interaction logic for ParametersEditView.xaml
    /// </summary>
    public partial class ParametersEditView : Window
    {
        public ParametersEditView()
        {
            InitializeComponent();
            
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FindVisualChild<TextBox>(Parameters.ItemContainerGenerator.ContainerFromIndex(0))?.Focus();
        }
        
        public static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null
                        && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null)
                        return childItem;
                }
            }
            return null;
        }
        
        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AcceptCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
