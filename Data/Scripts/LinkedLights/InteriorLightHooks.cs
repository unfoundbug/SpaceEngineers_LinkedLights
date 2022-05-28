// <copyright file="InteriorLightHooks.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using Sandbox.Common.ObjectBuilders;
    using VRage.Game.Components;

    /// <summary>
    /// Hooks for MyObjectBuilder_InteriorLight, also impacts LightPanel.
    /// </summary>
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_InteriorLight), false)]
    public class InteriorLightHooks : BaseLightHooks
    {
    }
}