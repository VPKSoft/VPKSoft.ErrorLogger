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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VPKSoft.ErrorLogger;

namespace WinFormsTest
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            ExceptionLogger.LogMessage("Application start: " + Application.ProductName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = int.Parse("0");
            MessageBox.Show((1 / i).ToString());
        }

        private void button2_Click(object sender, EventArgs e)
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
    }
}
