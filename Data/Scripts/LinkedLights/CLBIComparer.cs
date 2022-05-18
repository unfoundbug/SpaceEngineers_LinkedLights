// <copyright file="CLBIComparer.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using System.Collections.Generic;
    using VRage.ModAPI;

    /// <summary>
    /// List item comparer.
    /// </summary>
    public class CLBIComparer : IComparer<MyTerminalControlListBoxItem>
    {
        /// <inheritdoc/>
        public int Compare(MyTerminalControlListBoxItem x, MyTerminalControlListBoxItem y)
        {
            return x.Text.String.CompareTo(y.Text.String);
        }
    }
}