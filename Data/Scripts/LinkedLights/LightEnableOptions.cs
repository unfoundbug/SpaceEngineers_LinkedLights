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
        /// Not displayed, here for cast reasons.
        /// </summary>
        None = 0,

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

        /// <summary>
        /// The selected Battery is at 99% or above charge
        /// </summary>
        Battery_Charged = 1 << 6,

        /// <summary>
        /// The selected tank is at 99% or above capacity
        /// </summary>
        Tank_Full = 1 << 7,

        /// <summary>
        /// The tank is stockpiling
        /// </summary>
        Tank_Stockpile = 1 << 8,

        /// <summary>
        /// The current thruster output power
        /// </summary>
        Thrust_Power = 1 << 9,
    }
}
