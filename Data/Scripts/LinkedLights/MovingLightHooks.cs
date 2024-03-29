﻿// <copyright file="MovingLightHooks.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.ModAPI;
    using VRage.Game.Components;

    /// <summary>
    /// Hooks for MyObjectBuilder_SignalLight.
    /// </summary>
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_SignalLight), false)]
    public class MovingLightHooks : BaseLightHooks<IMyReflectorLight>
    {
    }
}