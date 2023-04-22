// <copyright file="SessionShim.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using System;
    using System.Collections.Generic;
    using VRage.Game.Components;

    /// <summary>
    /// Session Based instance.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class SessionShim : MySessionComponentBase
    {
        /// <summary>
        /// Instance for session static container.
        /// </summary>
        public static SessionShim Instance;

        private List<Type> registeredControls = new List<Type>();

        /// <inheritdoc/>
        public override void LoadData()
        {
            Instance = this;
            base.LoadData();
        }

        /// <summary>
        /// Attempt to bind connector controls to the UI.
        /// </summary>
        public void AttemptControlsInit<typeToRegister>()
        {
            if (!this.registeredControls.Contains(typeof(typeToRegister)))
            {
                this.registeredControls.Add(typeof(typeToRegister));
                LightHookHelper.AttachControls< typeToRegister>();
            }
        }

        /// <inheritdoc/>
        protected override void UnloadData()
        {
            Instance = null;
        }


    }
}
