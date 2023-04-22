// <copyright file="BaseLightHooks.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

using VRage.Utils;

namespace UnFoundBug.LightLink
{
    using System;
    using System.Text;
    using Sandbox.Game.EntityComponents;
    using Sandbox.ModAPI;
    using VRage.Game.Components;
    using VRage.Game.ModAPI.Network;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRageMath;

    /// <summary>
    /// Base handling class for common behaviour across MyObjectBuilder_ types.
    /// </summary>
    public class BaseLightHooks<typeToRegister> : MyGameLogicComponent
    {
        private IMyFunctionalBlock targetBlock = null;

        private static readonly Guid StorageGuid = new Guid("{F4D66A79-0469-47A3-903C-7964C8F65A25}");

        private MySync<long, SyncDirection.BothWays> syncTargetEntity = null;
        private MySync<bool, SyncDirection.BothWays> syncEnableSubGrid = null;
        private MySync<bool, SyncDirection.BothWays> syncEnableFiltering = null;
        private MySync<LightEnableOptions, SyncDirection.BothWays> syncEnableOption = null;
        private bool valueChanged = false;

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
        }

        public long TargetEntity
        {
            get
            {
                return syncTargetEntity.Value;
            }

            set
            {
                if (value != syncTargetEntity.Value)
                {
                    this.syncTargetEntity.Value = value;
                }
            }
        }

        public bool EnableSubGrid
        {
            get
            {
                return syncEnableSubGrid.Value;
            }

            set
            {
                if(this.syncEnableSubGrid.Value != value)
                {
                    this.syncEnableSubGrid.Value = value;
                }
            }
        }

        public bool EnableFiltering
        {
            get
            {
                return syncEnableFiltering.Value;
            }

            set
            {
                if (this.syncEnableFiltering.Value != value)
                {
                    this.syncEnableFiltering.Value = value;
                }
            }
        }

        public LightEnableOptions LightEnableOption
        {
            get
            {
                return syncEnableOption.Value;
            }

            set
            {
                if (this.syncEnableOption.Value != value)
                {
                    this.syncEnableOption.Value = value;
                }
            }
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
            this.Deserialise();

            this.syncEnableOption.ValueChanged += SyncEnableOption_ValueChanged;
            this.syncEnableSubGrid.ValueChanged += SyncEnableSubGrid_ValueChanged;
            this.syncTargetEntity.ValueChanged += SyncTargetEntity_ValueChanged;

			this.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            this.BaseLight.CustomDataChanged += this.BaseLight_CustomDataChanged;
            this.LoadColours();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            SessionShim.Instance.AttemptControlsInit<typeToRegister>();
        }

        private void SyncTargetEntity_ValueChanged(MySync<long, SyncDirection.BothWays> obj)
        {
            this.Serialise();
        }

        private void SyncEnableSubGrid_ValueChanged(MySync<bool, SyncDirection.BothWays> obj)
        {
            this.valueChanged = true;
            this.Serialise();
        }

        private void SyncEnableOption_ValueChanged(MySync<LightEnableOptions, SyncDirection.BothWays> obj)
        {
            this.Serialise();
        }

        /// <inheritdoc/>
        public override bool IsSerialized()
        {
            return this.valueChanged | base.IsSerialized();
        }

        /// <inheritdoc/>
        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();

            if (this.TargetEntity == 0)
            {
                return;
            }

            if (this.targetBlock?.EntityId != this.TargetEntity)
            {
                this.targetBlock = MyAPIGateway.Entities.GetEntityById(this.TargetEntity) as IMyFunctionalBlock;
            }

            if (this.targetBlock == null)
            {
                return;
            }

            bool newEnable = false;
            bool skipChecks = false;

            if (this.syncEnableSubGrid.Value)
            {
                // Target may be detached
                if (!this.BaseLight.CubeGrid.IsInSameLogicalGroupAs(this.targetBlock.CubeGrid))
                {
                    skipChecks = true;
                }
            }

            if (!skipChecks)
            {
                switch (this.syncEnableOption.Value)
                {
                    case LightEnableOptions.None:
                        break;
                    case LightEnableOptions.Generic_Enable:
                        if (this.targetBlock is IMyFunctionalBlock)
                        {
                            var asFunc = this.targetBlock as IMyFunctionalBlock;
                            newEnable |= asFunc.Enabled;
                        }

                        break;
                    case LightEnableOptions.Generic_IsFunctional:
                        if (this.targetBlock is IMyFunctionalBlock)
                        {
                            var asFunc = this.targetBlock as IMyFunctionalBlock;
                            newEnable |= asFunc.IsFunctional;
                        }
                        break;
                    case LightEnableOptions.Tool_IsActive:
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

                        break;
                    case LightEnableOptions.Battery_Charging:
                        if (this.targetBlock is IMyBatteryBlock)
                        {
                            var asBatt = this.targetBlock as IMyBatteryBlock;
                            newEnable |= (asBatt.CurrentStoredPower / asBatt.MaxStoredPower) <= 0.995;
                        }

                        break;
                    case LightEnableOptions.Battery_ChargeMode:
                        if (this.targetBlock is IMyBatteryBlock)
                        {
                            var asBatt = this.targetBlock as IMyBatteryBlock;
                            newEnable |= asBatt.ChargeMode == Sandbox.ModAPI.Ingame.ChargeMode.Recharge;
                        }

                        break;
                    case LightEnableOptions.Battery_Charged:
                        if (this.targetBlock is IMyBatteryBlock)
                        {
                            var asBatt = this.targetBlock as IMyBatteryBlock;
                            newEnable |= (asBatt.CurrentStoredPower / asBatt.MaxStoredPower) > 0.995;
                        }

                        break;
                    case LightEnableOptions.Tank_Full:
                        if (this.targetBlock is IMyGasTank)
                        {
                            var asTank = this.targetBlock as IMyGasTank;
                            newEnable |= asTank.FilledRatio > 0.99; ;
                        }

                        break;
                    case LightEnableOptions.Tank_Stockpile:
                        if (this.targetBlock is IMyGasTank)
                        {
                            var asTank = this.targetBlock as IMyGasTank;
                            newEnable |= asTank.Stockpile;
                        }

                        break;
                    case LightEnableOptions.Tank_Fill:
                        if (this.targetBlock is IMyGasTank)
                        {
                            var asTank = this.targetBlock as IMyGasTank;
                            newEnable |= asTank.FilledRatio > 0.99;
							float endLightIntensity = (float)(asTank.FilledRatio);
                            var resultantColour = Color.Lerp(new Color(this.startR, this.startG, this.startB), new Color(this.endR, this.endG, this.endB), endLightIntensity);

                            if (string.IsNullOrWhiteSpace(this.BaseLight.CustomData))
                            {
                                this.BaseLight.CustomData = "Colours are 0-255 R G B\nHigh: 255 255 255\nLow: 0 0 0";
                            }

                            this.BaseLight.Color = resultantColour;
                        }

                        break;
                    case LightEnableOptions.Thrust_Power:
                        if (this.targetBlock is IMyThrust)
                        {
                            var asThrust = this.targetBlock as IMyThrust;
                            newEnable = true;
                            float endLightIntensity = asThrust.CurrentThrust / asThrust.MaxThrust;

                            var resultantColour = Color.Lerp(new Color(this.startR, this.startG, this.startB), new Color(this.endR, this.endG, this.endB), endLightIntensity);

                            if (string.IsNullOrWhiteSpace(this.BaseLight.CustomData))
                            {
                                this.BaseLight.CustomData = "Colours are 0-255 R G B\nHigh: 255 255 255\nLow: 0 0 0";
                            }

                            this.BaseLight.Color = resultantColour;
                        }

                        break;
                }
            }

            if (this.BaseLight.Enabled != newEnable)
            {
                this.BaseLight.Enabled = newEnable;
            }
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

        private void Deserialise()
        {
            try
            {
                if (this.Entity.Storage != null)
                {
                    if (this.Entity.Storage.ContainsKey(StorageGuid))
                    {
                        string dataSource = this.Entity.Storage.GetValue(StorageGuid);
                        {
                            string[] components = dataSource.Split(',');
                            int versionId = int.Parse(components[0]);
                            switch (versionId)
                            {
                                case 1:
                                {
                                    this.TargetEntity = long.Parse(components[1]);
                                    this.EnableSubGrid = bool.Parse(components[2]);
                                    this.EnableFiltering = bool.Parse(components[3]);
                                    this.LightEnableOption =
                                        (LightEnableOptions) Enum.Parse(typeof(LightEnableOptions), components[4]);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                MyLog.Default.WriteLineAndConsole($"Linked Lights: Error loading light: {Entity.EntityId}");

            }
        }

        private void Serialise()
        {
            this.valueChanged = true;
            StringBuilder sb = new StringBuilder();
            sb.Append("1,");
            sb.Append(this.TargetEntity.ToString());
            sb.Append(",");
            sb.Append(this.EnableSubGrid);
            sb.Append(',');
            sb.Append(this.EnableFiltering);
            sb.Append(',');
            sb.Append(this.LightEnableOption);

            if (this.TargetEntity != 0)
            {
                if (this.Entity.Storage == null)
                {
                    this.Entity.Storage = new MyModStorageComponent();
                }

                this.Entity.Storage.SetValue(StorageGuid, sb.ToString());
            }
            else
            {
                if (this.Entity.Storage != null)
                {
                    if (this.Entity.Storage.ContainsKey(StorageGuid))
                    {
                        this.Entity.Storage.Remove(StorageGuid);
                    }
                }
            }
        }
    }
}
