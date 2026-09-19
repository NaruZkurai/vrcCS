namespace NZK.NaNimate
{
public partial class NZKNaNimateGroupManifest {
[System.Serializable]
        public sealed class MeshSelection
        {
            public string name = string.Empty;
            public bool enabled = true;
            public MeshSelection() { }
            public MeshSelection(string name, bool enabled)
            {
                this.name = name;
                this.enabled = enabled;
            }
        }
}
}
