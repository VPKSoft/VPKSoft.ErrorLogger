#region License
/*
ErrorLogger

A library that logs unhandled application exceptions to a file.
Copyright (C) 2015 VPKSoft, Petteri Kautonen

Contact: vpksoft@vpksoft.net

This file is part of ErrorLogger.

ErrorLogger is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ErrorLogger is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with ErrorLogger.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VPKSoft.ErrorLogger;

namespace WPFTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExceptionLogger.Bind();
            ExceptionLogger.LogMessage("Application start: " + System.Windows.Application.ResourceAssembly.GetName().Name);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            int i = int.Parse("0");
            MessageBox.Show((1 / i).ToString());
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int i = int.Parse("0");
                MessageBox.Show((1 / i).ToString());
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogError(ex);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ExceptionLogger.UnBind();
        }
    }
}
