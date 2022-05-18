// <copyright file="BaseLightHooks.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using System;
    using Sandbox.ModAPI;
    using VRage.Game.Components;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    /// <summary>
    /// Base handling class for common behaviour across MyObjectBuilder_ types.
    /// </summary>
    public class BaseLightHooks : MyGameLogicComponent
    {
        private IMyFunctionalBlock targetBlock = null;

        private StorageHandler sHandler;
        private float startR = 0;
        private float startG = 0;
        private float startB = 0;
        private float endR = 1;
        private float endG = 1;
        private float endB = 1;

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
            this.BaseLight.CustomDataChanged -= this.BaseLight_CustomDataChanged;
        }

        /// <inheritdoc/>
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            this.Entity.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            this.Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            this.sHandler = new StorageHandler(this.Entity);
            this.BaseLight.CustomDataChanged += this.BaseLight_CustomDataChanged;
            this.LoadColours();
        }

        /// <inheritdoc/>
        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            this.sHandler.Deserialise();

            if (this.sHandler.TargetEntity != 0)
            {
                var target = MyAPIGateway.Entities.GetEntityById(this.sHandler.TargetEntity);

                if (target != null)
                {
                    this.AttachTarget(target as IMyFunctionalBlock);
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

            if (this.targetBlock == null || this.sHandler.TargetEntity == 0)
            {
                return;
            }

            var handledEntity = this.targetBlock;

            bool newEnable = false;
            bool skipChecks = false;

            if (this.sHandler.SubGridScanningEnable)
            {
                // Target may be detached
                if (!this.BaseLight.CubeGrid.IsInSameLogicalGroupAs(handledEntity.CubeGrid))
                {
                    skipChecks = true;
                }
            }

            if (!skipChecks)
            {
                if ((this.sHandler.ActiveFlags & LightEnableOptions.Generic_Enable) ==
                    LightEnableOptions.Generic_Enable)
                {
                    newEnable |= handledEntity.Enabled;
                }

                if ((this.sHandler.ActiveFlags & LightEnableOptions.Generic_IsFunctional) ==
                    LightEnableOptions.Generic_IsFunctional)
                {
                    newEnable |= handledEntity.IsFunctional;
                }

                if ((this.sHandler.ActiveFlags & LightEnableOptions.Tool_IsActive) == LightEnableOptions.Tool_IsActive)
                {
                    if (handledEntity is IMyShipToolBase)
                    {
                        var asTool = handledEntity as IMyShipToolBase;
                        newEnable |= asTool.IsActivated;
                    }
                    else if (handledEntity is IMyProductionBlock)
                    {
                        var asRef = handledEntity as IMyProductionBlock;
                        newEnable |= asRef.IsProducing && asRef.Enabled;
                    }
                }

                if (handledEntity is IMyBatteryBlock)
                {
                    var asBatt = handledEntity as IMyBatteryBlock;
                    if ((this.sHandler.ActiveFlags & LightEnableOptions.Battery_Charging) != 0)
                    {
                        newEnable |= asBatt.IsCharging;
                    }

                    if ((this.sHandler.ActiveFlags & LightEnableOptions.Battery_Charged) != 0)
                    {
                        newEnable |= (asBatt.CurrentStoredPower / asBatt.MaxStoredPower) > 0.99;
                    }
                }

                if ((this.sHandler.ActiveFlags & LightEnableOptions.Battery_ChargeMode) ==
                    LightEnableOptions.Battery_ChargeMode)
                {
                    if (handledEntity is IMyBatteryBlock)
                    {
                        var asBatt = handledEntity as IMyBatteryBlock;
                        newEnable |= asBatt.ChargeMode == Sandbox.ModAPI.Ingame.ChargeMode.Recharge;
                    }
                }

                if (handledEntity is IMyGasTank)
                {
                    var asTank = handledEntity as IMyGasTank;
                    if ((this.sHandler.ActiveFlags & LightEnableOptions.Tank_Full) != 0)
                    {
                        newEnable |= asTank.FilledRatio > 0.99;
                    }

                    if ((this.sHandler.ActiveFlags & LightEnableOptions.Tank_Stockpile) != 0)
                    {
                        newEnable |= asTank.Stockpile;
                    }
                }

                if ((this.sHandler.ActiveFlags & LightEnableOptions.Thrust_Power) != 0)
                {
                    var asThrust = handledEntity as IMyThrust;
                    newEnable = true;
                    float endLightIntensity = asThrust.CurrentThrust / asThrust.MaxThrust;
                    this.LoadColours();

                    var resultantColour = Color.Lerp(new Color(this.startR, this.startG, this.startB), new Color(this.endR, this.endG, this.endB), endLightIntensity);

                    if (string.IsNullOrWhiteSpace(this.BaseLight.CustomData))
                    {
                        this.BaseLight.CustomData = "Colours are 0-255 R G B\nHigh: 255 255 255\nLow: 0 0 0";
                    }

                    this.BaseLight.Color = resultantColour;
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

        private void BaseLight_CustomDataChanged(IMyTerminalBlock obj)
        {
            this.LoadColours();
        }

        private void LoadColours()
        {
            var source = this.BaseLight.CustomData;
            var lines = source.Split('\n');
            foreach (var line in lines)
            {
                byte newComp;
                var components = line.Split(' ');
                if (line.StartsWith("High: "))
                {
                    if (byte.TryParse(components[1], out newComp))
                    {
                        this.endR = newComp / (float)byte.MaxValue;
                    }
                    else
                    {
                        this.endR = 1.0f;
                    }

                    if (byte.TryParse(components[2], out newComp))
                    {
                        this.endG = newComp / (float)byte.MaxValue;
                    }
                    else
                    {
                        this.endG = 1.0f;
                    }

                    if (byte.TryParse(components[3], out newComp))
                    {
                        this.endB = newComp / (float)byte.MaxValue;
                    }
                    else
                    {
                        this.endB = 1.0f;
                    }
                }
                else if (line.StartsWith("Low: "))
                {
                    if (byte.TryParse(components[1], out newComp))
                    {
                        this.startR = newComp / (float)byte.MaxValue;
                    }
                    else
                    {
                        this.startR = 1.0f;
                    }

                    if (byte.TryParse(components[2], out newComp))
                    {
                        this.startG = newComp / (float)byte.MaxValue;
                    }
                    else
                    {
                        this.startG = 1.0f;
                    }

                    if (byte.TryParse(components[3], out newComp))
                    {
                        this.startB = newComp / (float)byte.MaxValue;
                    }
                    else
                    {
                        this.startB = 1.0f;
                    }
                }
            }
        }
    }
}
