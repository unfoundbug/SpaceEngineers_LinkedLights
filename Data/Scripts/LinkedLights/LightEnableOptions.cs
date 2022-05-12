// <copyright file="LightEnableOptions.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Enumeration Flags for Linkage.
    /// </summary>
    [Flags]
    public enum LightEnableOptions
    {
        /// <summary>
        /// Loads from the Enable option
        /// </summary>
        Generic_Enable = 1 << 1,

        /// <summary>
        /// If available, Is Functional is used
        /// </summary>
        Generic_IsFunctional = 1 << 2,

        /// <summary>
        /// If available, IsActive is used
        /// </summary>
        Tool_IsActive = 1 << 3,

        /// <summary>
        /// The selected Battery is in Charge Mode
        /// </summary>
        Battery_Charging = 1 << 4,

        /// <summary>
        /// The selected battery is in charge mode and full
        /// </summary>
        Battery_ChargeMode = 1 << 5,
    }
}
