// <copyright file="BaseLightHooks.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace TestScript
{
    using System.Linq;
    using Sandbox.ModAPI;
    using VRage.Game.Components;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

    /// <summary>
    /// Base handling class for common behaviour across MyObjectBuilder_ types.
    /// </summary>
    public class BaseLightHooks : MyGameLogicComponent
    {
        private IMyFunctionalBlock targetBlock = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLightHooks"/> class.
        /// </summary>
        public BaseLightHooks()
        {
            // Logging.Instance.WriteLine("MyObjectBuilder_InteriorLightConstructed!");
            LightHookHelper.AttachControls();
        }

        private IMyLightingBlock BaseLight => (IMyLightingBlock)this.Entity;

        /// <inheritdoc/>
        public override void Close()
        {
        }

        /// <inheritdoc/>
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // Logging.Instance.WriteLine("MyObjectBuilder Init started");
            base.Init(objectBuilder);
            this.Entity.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            this.Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        /// <inheritdoc/>
        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            // Logging.Instance.WriteLine("Light update for " + this.BaseLight.DisplayNameText + " started.");
            long targetBlockId = LightHookHelper.GetTargetId(this.BaseLight);

            // Logging.Instance.WriteLine("Found target block: " + targetBlockId.ToString());
            if (targetBlockId != 0)
            {
                var funcBlocks = this.BaseLight.CubeGrid.GetFatBlocks<IMyFunctionalBlock>();
                var target = funcBlocks.FirstOrDefault(found => found.EntityId == targetBlockId);
                this.AttachTarget(target);
            }
            else
            {
                this.DetachFromTarget();
            }
        }

        /// <inheritdoc/>
        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (this.targetBlock != null)
            {
                if (this.targetBlock is IMyShipToolBase)
                {
                    var asTool = this.targetBlock as IMyShipToolBase;
                    var newEn = asTool.Enabled | asTool.IsActivated;
                    if (this.BaseLight.Enabled != newEn)
                    {
                        this.BaseLight.Enabled = newEn;
                    }
                }
                else
                {
                    // Logging.Instance.WriteLine(this.BaseLight.DisplayNameText + "is changing to " + this.targetBlock.Enabled.ToString());
                    this.BaseLight.Enabled = this.targetBlock.Enabled;
                }
            }
        }

        private void DetachFromTarget()
        {
            if (this.targetBlock != null)
            {
                // Logging.Instance.WriteLine(this.BaseLight.DisplayNameText + "is now detached from " + this.targetBlock.DisplayNameText);
                this.targetBlock.OnMarkForClose -= this.TargetBlock_OnMarkForClose;
                this.targetBlock = null;
            }
        }

        private void AttachTarget(IMyFunctionalBlock newTarget)
        {
            this.DetachFromTarget();
            if (newTarget != null)
            {
                this.targetBlock = newTarget;
                this.targetBlock.OnMarkForClose += this.TargetBlock_OnMarkForClose;
            }
        }

        private void TargetBlock_OnMarkForClose(IMyEntity obj)
        {
            this.DetachFromTarget();
        }
    }
}
