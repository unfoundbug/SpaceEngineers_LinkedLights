// <copyright file="ReflectorLightHooks.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.ModAPI;
    using VRage.Game.Components;

    /// <summary>
    /// Hooks for MyObjectBuilder_ReflectorLight.
    /// </summary>
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_ReflectorLight), false)]
    public class ReflectorLightHooks : BaseLightHooks<IMyReflectorLight>
    {
    }
}