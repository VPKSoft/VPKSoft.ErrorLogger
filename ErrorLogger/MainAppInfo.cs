#region License
/*
ErrorLogger

A library that logs unhandled application exceptions to a file.
Copyright (C) 2018 VPKSoft, Petteri Kautonen

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
using System.Reflection;

namespace VPKSoft.Utils
{
    /// <summary>
    /// Provides information of the entry assembly (the main application).
    /// </summary>
    public class MainAppInfo
    {
        /// <summary>
        /// A static reference to the entry assembly.
        /// </summary>
        public static readonly Assembly EntryAssembly = Assembly.GetEntryAssembly();

        /// <summary>
        /// Gets the version of the entry assembly.
        /// </summary>
        public static Version Version
        {
            get
            {
                return EntryAssembly.GetName().Version;
            }
        }

        /// <summary>
        /// Gets the version of the entry assembly as string.
        /// </summary>
        public static string VersionString
        {
            get
            {
                return EntryAssembly.GetName().Version.ToString();
                
            }
        }

        /// <summary>
        /// Gets the name of the entry assembly.
        /// </summary>
        public static string Name
        {
            get
            {
                return EntryAssembly.GetName().Name;
            }
        }

        /// <summary>
        /// Gets the title of the entry assembly.
        /// </summary>
        public static string Title
        {
            get
            {
                object[] attributes = EntryAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != string.Empty)
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(EntryAssembly.CodeBase);
            }
        }

        /// <summary>
        /// Gets the description of the entry assembly.
        /// </summary>
        public static string Description
        {
            get
            {
                object[] attributes = EntryAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        /// <summary>
        /// Gets the product name of the entry assembly.
        /// </summary>
        public static string Product
        {
            get
            {
                object[] attributes = EntryAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        /// <summary>
        /// Gets the copyright string of the entry assembly.
        /// </summary>
        public static string Copyright
        {
            get
            {
                object[] attributes = EntryAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        /// <summary>
        /// Gets the company of the entry assembly.
        /// </summary>
        public static string Company
        {
            get
            {
                object[] attributes = EntryAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
    }
}
