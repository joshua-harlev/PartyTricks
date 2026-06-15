using System;
using System.Collections.Generic;
using UnityEngine;

namespace TitleScreen {
    [CreateAssetMenu(fileName = "CreditsScreenData", menuName = "Scriptable Objects/CreditsScreenData")]
    public class CreditsScreenData : ScriptableObject {
        public List<CreditsEntry> CreditsEntries;
        public List<CreditsToolAssetEntry> CreditsToolAssetEntries;
    }
    
    [Serializable]
    public class CreditsEntry {
        public string Role;
        public List<string> Names;
    }

    [Serializable]
    public class CreditsToolAssetEntry {
        public string Name;
        public string Usage;
        public string Author;
    }
}
