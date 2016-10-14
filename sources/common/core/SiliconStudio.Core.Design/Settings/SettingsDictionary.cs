﻿using System.Collections.Generic;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml.Events;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// An internal dictionary class used to serialize a <see cref="SettingsProfile"/>.
    /// </summary>
    internal class SettingsDictionary : Dictionary<UFile, List<ParsingEvent>>
    {
        // Used for temporary internal storage
        [DataMemberIgnore]
        internal SettingsProfile Profile;
    }
}
