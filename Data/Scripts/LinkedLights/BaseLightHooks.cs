// <copyright file="BaseLightHooks.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI;

namespace UnFoundBug.LightLink
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

        private StorageHandler sHandler;

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
            this.sHandler = new StorageHandler(this.Entity);
        }

        /// <inheritdoc/>
        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            // Logging.Instance.WriteLine("Light update for " + this.BaseLight.DisplayNameText + " started.");
            this.sHandler.Deserialise();

            // Logging.Instance.WriteLine("Found target block: " + targetBlockId.ToString());
            if (this.sHandler.TargetEntity != 0)
            {
                List<IMyCubeGrid> activeGrids = new List<IMyCubeGrid>();
                if (!sHandler.SubGridScanningEnable)
                {
                    activeGrids.Add(this.BaseLight.CubeGrid);
                }
                else
                {
                    var foundGrids = this.BaseLight.CubeGrid.GetGridGroup(GridLinkTypeEnum.Physical).GetGrids(activeGrids);
                }

                foreach (var grid in activeGrids)
                {
                    var funcBlocks = this.BaseLight.CubeGrid.GetFatBlocks<IMyFunctionalBlock>();
                    var target = funcBlocks.FirstOrDefault(found => found.EntityId == this.sHandler.TargetEntity);
                    if (target != null)
                    {
                        this.AttachTarget(target);
                        break;
                    }
                }
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

            if (this.targetBlock == null)
            {
                return;
            }

            bool newEnable = false;

            if ((this.sHandler.ActiveFlags & LightEnableOptions.Generic_Enable) == LightEnableOptions.Generic_Enable)
            {
                newEnable |= this.targetBlock.Enabled;
            }

            if ((this.sHandler.ActiveFlags & LightEnableOptions.Generic_IsFunctional) == LightEnableOptions.Generic_IsFunctional)
            {
                newEnable |= this.targetBlock.IsFunctional;
            }

            if ((this.sHandler.ActiveFlags & LightEnableOptions.Tool_IsActive) == LightEnableOptions.Tool_IsActive)
            {
                if (this.targetBlock is IMyShipToolBase)
                {
                    var asTool = this.targetBlock as IMyShipToolBase;
                    newEnable |= asTool.IsActivated;
                }
                else if (this.targetBlock is IMyProductionBlock)
                {
                    var asRef = this.targetBlock as IMyProductionBlock;
                    newEnable |= asRef.IsProducing;
                }
            }

            if ((this.sHandler.ActiveFlags & LightEnableOptions.Battery_Charging) == LightEnableOptions.Battery_Charging)
            {
                if (this.targetBlock is IMyBatteryBlock)
                {
                    var asBatt = this.targetBlock as IMyBatteryBlock;
                    newEnable |= asBatt.IsCharging;
                }
            }

            if ((this.sHandler.ActiveFlags & LightEnableOptions.Battery_ChargeMode) == LightEnableOptions.Battery_ChargeMode)
            {
                if (this.targetBlock is IMyBatteryBlock)
                {
                    var asBatt = this.targetBlock as IMyBatteryBlock;
                    newEnable |= asBatt.ChargeMode == ChargeMode.Recharge;
                }
            }

            if (this.BaseLight.Enabled != newEnable)
            {
                this.BaseLight.Enabled = newEnable;
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
