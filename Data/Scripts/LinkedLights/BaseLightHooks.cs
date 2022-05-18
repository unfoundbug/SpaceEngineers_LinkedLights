﻿// <copyright file="BaseLightHooks.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using System.Collections.Generic;
    using System.Linq;
    using Sandbox.ModAPI;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
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
            base.Init(objectBuilder);
            this.Entity.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            this.Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            this.sHandler = new StorageHandler(this.Entity);
        }

        /// <inheritdoc/>
        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            this.sHandler.Deserialise();

            if (this.sHandler.TargetEntity != 0)
            {
                List<IMyCubeGrid> activeGrids = new List<IMyCubeGrid>();
                if (!this.sHandler.SubGridScanningEnable)
                {
                    activeGrids.Add(this.BaseLight.CubeGrid);
                }
                else
                {
                    this.BaseLight.CubeGrid.GetGridGroup(GridLinkTypeEnum.Physical).GetGrids(activeGrids);
                }

                foreach (var grid in activeGrids)
                {
                    var funcBlocks = grid.GetFatBlocks<IMyFunctionalBlock>();
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
            bool skipChecks = false;

            if (this.sHandler.SubGridScanningEnable)
            {
                // Target may be detached
                if (!this.BaseLight.CubeGrid.IsInSameLogicalGroupAs(this.targetBlock.CubeGrid))
                {
                    skipChecks = true;
                }
            }

            if (!skipChecks)
            {
                if ((this.sHandler.ActiveFlags & LightEnableOptions.Generic_Enable) ==
                    LightEnableOptions.Generic_Enable)
                {
                    newEnable |= this.targetBlock.Enabled;
                }

                if ((this.sHandler.ActiveFlags & LightEnableOptions.Generic_IsFunctional) ==
                    LightEnableOptions.Generic_IsFunctional)
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
                        newEnable |= asRef.IsProducing && asRef.Enabled;
                    }
                }

                if ((this.sHandler.ActiveFlags & LightEnableOptions.Battery_Charging) ==
                    LightEnableOptions.Battery_Charging)
                {
                    if (this.targetBlock is IMyBatteryBlock)
                    {
                        var asBatt = this.targetBlock as IMyBatteryBlock;
                        newEnable |= asBatt.IsCharging;
                    }
                }

                if ((this.sHandler.ActiveFlags & LightEnableOptions.Battery_Charged) ==
                    LightEnableOptions.Battery_Charged)
                {
                    if (this.targetBlock is IMyBatteryBlock)
                    {
                        var asBatt = this.targetBlock as IMyBatteryBlock;
                        newEnable |= (asBatt.CurrentStoredPower / asBatt.MaxStoredPower) > 0.99;
                    }
                }

                if ((this.sHandler.ActiveFlags & LightEnableOptions.Battery_ChargeMode) ==
                    LightEnableOptions.Battery_ChargeMode)
                {
                    if (this.targetBlock is IMyBatteryBlock)
                    {
                        var asBatt = this.targetBlock as IMyBatteryBlock;
                        newEnable |= asBatt.ChargeMode == Sandbox.ModAPI.Ingame.ChargeMode.Recharge;
                    }
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
