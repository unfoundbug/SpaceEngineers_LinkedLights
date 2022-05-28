// <copyright file="SessionShim.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
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

        private bool controlsInit = false;

        /// <inheritdoc/>
        public override void LoadData()
        {
            Instance = this;
            base.LoadData();
        }

        /// <summary>
        /// Attempt to bind connector controls to the UI.
        /// </summary>
        public void AttemptControlsInit()
        {
            if (!this.controlsInit)
            {
                this.controlsInit = true;
                LightHookHelper.AttachControls();
            }
        }

        /// <inheritdoc/>
        protected override void UnloadData()
        {
            Instance = null;
        }


    }
}
