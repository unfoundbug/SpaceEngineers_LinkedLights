// <copyright file="StorageHandler.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage.ModAPI;

namespace UnFoundBug.LightLink
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Serialises and Deserialises entity storage
    /// </summary>
    public class StorageHandler
    {
        private static Guid StorageGuid = new Guid("{F4D66A79-0469-47A3-903C-7964C8F65A25}");

        private LightEnableOptions flags = LightEnableOptions.Generic_Enable;
        private bool subGridScanning = false;
        private bool blockFiltering = false;
        private long targetEntity = 0;
        private IMyEntity source;

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

        public StorageHandler(IMyEntity source)
        {
            this.source = source;
            if (!this.Deserialise())
            {
                this.Serialise();
            }
        }

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
