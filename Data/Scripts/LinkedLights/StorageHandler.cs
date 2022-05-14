// <copyright file="StorageHandler.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using System;
    using System.Linq;
    using System.Text;
    using Sandbox.Game.EntityComponents;
    using Sandbox.ModAPI;
    using VRage.ModAPI;

    /// <summary>
    /// Serialises and Deserialises entity storage.
    /// </summary>
    public class StorageHandler
    {
        private static readonly Guid StorageGuid = new Guid("{F4D66A79-0469-47A3-903C-7964C8F65A25}");
        private readonly IMyEntity source;

        private LightEnableOptions flags = LightEnableOptions.Generic_Enable;
        private bool subGridScanning = false;
        private bool blockFiltering = true;
        private long targetEntity = 0;

        /*
         *  V0
         *      EntityId
         *
         *  V1
         *      EntityId
         *      SubGridScanning
         *      BlockFiltering
         *      ActiveFlags
         */

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageHandler"/> class.
        /// </summary>
        /// <param name="source">Source entity to load from.</param>
        public StorageHandler(IMyEntity source)
        {
            this.source = source;
            if (!this.Deserialise())
            {
                // load reduction where possible
                if (!MyAPIGateway.Multiplayer.MultiplayerActive)
                {
                    this.Serialise();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value representing the flags for light enable source.
        /// </summary>
        public LightEnableOptions ActiveFlags
        {
            get
            {
                return this.flags;
            }

            set
            {
                this.flags = value;
                this.Serialise();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether sub-grids should be scanned for the target entity.
        /// </summary>
        public bool SubGridScanningEnable
        {
            get
            {
                return this.subGridScanning;
            }

            set
            {
                this.subGridScanning = value;
                this.Serialise();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the blocks available on the UI for linkage are filtered down.
        /// </summary>
        public bool BlockFiltering
        {
            get
            {
                return this.blockFiltering;
            }

            set
            {
                this.blockFiltering = value;
                this.Serialise();
            }
        }

        /// <summary>
        /// Gets or sets the Linked entity ID.
        /// </summary>
        public long TargetEntity
        {
            get
            {
                return this.targetEntity;
            }

            set
            {
                this.targetEntity = value;
                this.Serialise();
            }
        }

        /// <summary>
        /// Deserialises the entity mod storage into current properties.
        /// </summary>
        /// <returns>Returns true if the loaded settings should be saved back to mod storage.</returns>
        public bool Deserialise()
        {
            bool newSettingsRequired = true;
            if (this.source.Storage != null)
            {
                if (this.source.Storage.ContainsKey(StorageGuid))
                {
                    string dataSource = this.source.Storage.GetValue(StorageGuid);
                    if (!dataSource.Contains(','))
                    {
                        // V0 Processing, contains only target entity ID
                        this.targetEntity = long.Parse(dataSource);
                    }
                    else
                    {
                        string[] components = dataSource.Split(',');
                        int versionId = int.Parse(components[0]);
                        switch (versionId)
                        {
                            case 1:
                                {
                                    this.targetEntity = long.Parse(components[1]);
                                    this.subGridScanning = bool.Parse(components[2]);
                                    this.blockFiltering = bool.Parse(components[3]);
                                    var rawFlag = int.Parse(components[4]);
                                    this.flags = (LightEnableOptions)rawFlag;

                                    newSettingsRequired = false;
                                    break;
                                }
                        }
                    }
                }
            }

            Logging.Warn("Settings Upgrade required for " + this.source.DisplayName);
            return newSettingsRequired;
        }

        private void Serialise()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("1,");
            sb.Append(this.targetEntity.ToString());
            sb.Append(",");
            sb.Append(this.subGridScanning);
            sb.Append(',');
            sb.Append(this.blockFiltering);
            sb.Append(',');
            sb.Append((int)this.flags);

            if (this.source.Storage == null)
            {
                this.source.Storage = new MyModStorageComponent();
            }

            this.source.Storage.SetValue(StorageGuid, sb.ToString());
        }
    }
}
