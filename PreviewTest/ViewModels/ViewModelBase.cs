﻿using Livet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PreviewTest.ViewModels
{
    internal class ViewModelBase : ViewModel
    {
        internal bool IsInDesignMode()
        {
            bool result = true;
            var window = Application.Current.MainWindow;

            if (window != null)
            {
                if (!IsInDesignMode(window))
                {
                    result = false;
                }
            }
            return result;
        }

        bool IsInDesignMode(DependencyObject element)
        {
            return System.ComponentModel.DesignerProperties.GetIsInDesignMode(element);
        }
    }
}
