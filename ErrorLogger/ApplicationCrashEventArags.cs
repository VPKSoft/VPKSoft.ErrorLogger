#region License
/*
ErrorLogger

A library that logs unhandled application exceptions to a file.
Copyright (C) 2019 VPKSoft, Petteri Kautonen

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

namespace VPKSoft.ErrorLogger
{
    /// <summary>
    /// Provides additional data for the ApplicationCrashData event.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ApplicationCrashEventArgs: EventArgs
    {
        /// <summary>
        /// An exception which caused the application to crash.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Indicates whether the common language runtime is terminating.
        /// </summary>
        public bool IsTerminating { get; internal set; }

        /// <summary>
        /// Gets the sender of the event.
        /// </summary>
        public object Sender { get; internal set; }

        /// <summary>
        /// Gets the <see cref="UnhandledExceptionEventArgs"/> arguments.
        /// </summary>
        public UnhandledExceptionEventArgs Args { get; internal set; }

        /// <summary>
        /// Gets or sets the additional data for the ApplicationCrashData event.
        /// </summary>
        public List<object> AdditionalData { get; internal set; } = new List<object>();
    }
}
