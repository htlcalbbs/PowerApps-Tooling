﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AppMagic.Authoring.Persistence
{
    // A minimal representation of the data component manifest
    // This is client-only. 
    // $$$ - can we get this from the PA file directly?
    internal class MinDataComponentManifest
    {
        public string Name { get; set; } // a name, "Component1"
        public string TemplateGuid { get; set; } // a guid 
        public string Description { get; set; }

        // $$$ we may allow multiple...
        public string DependentEntityName { get; set; } // "acount" 
        public string DataSetName { get; set; } // "default.cds"

        public JsonElement[] CustomProperties { get; set; }

        public DataComponentDefinitionJson DataComponentDefinitionKey { get; set; }

        internal void Apply(TemplateMetadataJson x)
        {
            x.Validate();

            // $$$ Consistency checks? Or we can just catch this on round-tripping? 
            SetGuid(x.Name);

            this.DataComponentDefinitionKey = x.DataComponentDefinitionKey;
            
            // Clear out volatile state. Will repopulate on write. 
            this.DataComponentDefinitionKey.ControlUniqueId = null; 

            this.CustomProperties = x.CustomProperties;
            this.DependentEntityName = x.DataComponentDefinitionKey.DependentEntityName;
            this.DataSetName = x.DataComponentDefinitionKey.DataComponentExternalDependencies[0].DataComponentCdsDependency.DataSetName;
        }

        internal void Apply(DataComponentsMetadataJson.Entry x)
        {
            this.Name = x.Name;
            this.Description = x.Description;
            SetGuid(x.TemplateName);
        }

        private void SetGuid(string guid)
        {
            if (this.TemplateGuid == null)
            {
                this.TemplateGuid = guid;
                return;
            }
            if (this.TemplateGuid!= guid)
            {
                throw new InvalidOperationException(); // Mismatch
            }
        }
    }


    // We recreate this file from the min version. 
    // Writes to \references\DataComponentSources.json
    public class DataComponentSourcesJson
    {
        public const string NativeCDSDataSourceInfo = "NativeCDSDataSourceInfo";

        // Copy verbatim over. 
        // Should be "portable" - doesn't have fields like Version, timestamp, etc. 
        public class Entry
        {
            // The template guid
            public string AssociatedDataComponentTemplate { get; set; } 

            public string Name { get; set; } // Name of data source, eg, Component1_Table
            public string Type { get; set; } // NativeCDSDataSourceInfo

            /*
            public bool IsSampleData { get; set; } // false
            public bool IsWritable { get; set; } //  true,
            public string DataComponentKind { get; set; } // "Extension",
            public string DatasetName { get; set; } //  "default.cds",
            public string EntitySetName { get; set; } //  "Component1_Table",
            public string LogicalName { get; set; } //  "default.cds",
            public string PreferredName { get; set; } //  "Component1_Table",
            public bool IsHidden { get; set; } //  false,
            public string DependentEntityName { get; set; } // "account"
            */
            // $$$ 
            [JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData { get; set; }
        }

        public Entry[] DataSources { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement> ExtensionData { get; set; }
    }

    // From D:\dev\pa2\PowerApps-Client\src\Cloud\DocumentServer.Core\Document\Document\Persistence\Serialization\Schemas\Control\Template\TemplateMetadataJson.cs
    // No [JsonExtensionData] since we need to be able to fully create this from a min format. 
    internal class TemplateMetadataJson
    {
        public string Name { get; set; }

        // Ok to be null. 
        //  Will default to: DateTime.Now.ToUniversalTime().Ticks.ToString();
        public string Version { get; set; }

        public bool? IsComponentLocked { get; set; }
        public bool? ComponentChangedSinceFileImport { get; set; }
        public bool? ComponentAllowCustomization { get; set; }

        public JsonElement[] CustomProperties { get; set; }

        public DataComponentDefinitionJson DataComponentDefinitionKey { get; set; }

        public void Validate()
        {
            if (DataComponentDefinitionKey?.ComponentRawMetadataKey != null)
            {
                throw new NotSupportedException("Does not support older formats using ComponentRawMetadataKey");
            }
        }
    }

    // Writes to \References\DataComponentTemplates.json
    internal class DataComponentTemplatesJson
    {
        public TemplateMetadataJson[] ComponentTemplates { get; set; }

        // Should be empty...
        //[JsonExtensionData]
        //public Dictionary<string, JsonElement> ExtensionData { get; set; }
    }
    



    // Writes to \ComponentsMetadata.json
    public class DataComponentsMetadataJson
    {
        public class Entry
        {
            public string Name { get; set;  } // "Component1";
            public string TemplateName { get; set; } // "a70e51d571ae4649a16b8bf1622ffdac";

            public string Description { get; set; }
            public bool AllowCustomization { get; set; }

            //[JsonExtensionData]
            //public Dictionary<string, JsonElement> ExtensionData { get; set; }
        }

        public Entry[] Components { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement> ExtensionData { get; set; }
    }
}