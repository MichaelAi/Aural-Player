﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Aural.CustomControls
{
    public class TitleBarPage : Page
    {
        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.RegisterAttached("ForegroundColor", typeof(Color),
            typeof(TitleBarPage),
            new PropertyMetadata(null, OnForegroundColorPropertyChanged));

        public static Color GetForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ForegroundColorProperty);
        }

        public static void SetForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ForegroundColorProperty, value);
        }

        private static void OnForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = color;
        }

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.RegisterAttached("BackgroundColor", typeof(Color),
            typeof(TitleBarPage),
            new PropertyMetadata(null, OnBackgroundColorPropertyChanged));

        public static Color GetBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(BackgroundColorProperty);
        }

        public static void SetBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(BackgroundColorProperty, value);
        }

        private static void OnBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = color;
        }

        public static readonly DependencyProperty ButtonForegroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonForegroundColor", typeof(Color),
            typeof(TitleBarPage),
            new PropertyMetadata(null, OnButtonForegroundColorPropertyChanged));

        public static Color GetButtonForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonForegroundColorProperty);
        }

        public static void SetButtonForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonForegroundColorProperty, value);
        }

        private static void OnButtonForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = color;
        }

        public static readonly DependencyProperty ButtonBackgroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonBackgroundColor", typeof(Color),
            typeof(TitleBarPage),
            new PropertyMetadata(null, OnButtonBackgroundColorPropertyChanged));

        public static Color GetButtonBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonBackgroundColorProperty);
        }

        public static void SetButtonBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonBackgroundColorProperty, value);
        }

        private static void OnButtonBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = color;
        }

        /////////////////////////////////////////////////////////////////////

        public static readonly DependencyProperty InactiveBackgroundColorProperty =
           DependencyProperty.RegisterAttached("InactiveBackgroundColor", typeof(Color),
           typeof(TitleBarPage),
           new PropertyMetadata(null, OnInactiveBackgroundColorPropertyChanged));

        public static Color GetInactiveBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(InactiveBackgroundColorProperty);
        }

        public static void SetInactiveBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(InactiveBackgroundColorProperty, value);
        }

        private static void OnInactiveBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.InactiveBackgroundColor = color;
        }

        ////////////////////////////////////////////////////////////////////////

        public static readonly DependencyProperty ButtonInactiveBackgroundColorProperty =
   DependencyProperty.RegisterAttached("ButtonInactiveBackgroundColor", typeof(Color),
   typeof(TitleBarPage),
   new PropertyMetadata(null, OnButtonInactiveBackgroundColorPropertyChanged));

        public static Color GetButtonInactiveBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonInactiveBackgroundColorProperty);
        }

        public static void SetButtonInactiveBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonInactiveBackgroundColorProperty, value);
        }

        private static void OnButtonInactiveBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonInactiveBackgroundColor = color;
        }

    }
}