using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ChaosConductor.Shared
{
    ///////////////////////////////////////////////////////////////////////////
    /// JSON Serializing/Deserializing structure definitions
    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// An object representation of the Ink JSON file.
    /// 
    /// topmostInfo - Info found at the start of the JSON file. Global tags, entry point, etc.
    /// done - Don't worry about this.
    /// knots - All of the functions and knots found within the JSON file.
    /// </summary>
    public class StoryRoot
    {
        public JArray topmostInfo;
        public string done;
        public JObject knots;
    }

    /// <summary>
    /// The bare deserialized version of the Ink JSON, before being massaged into a StoryStructure file.
    /// </summary>
    public class StoryStructureDeserialized
    {
        public int inkVersion { get; set; }
        public List<object> root { get; set; } // 0: unused (entry point container?), 1: "done", 2: ink data
        public JObject listDefs { get; set; }
    }

    public class StoryStructure
    {
        public int inkVersion { get; set; }
        public StoryRoot root { get; set; }
        public Dictionary<string, Dictionary<string, int>> listDefs { get; set; }
        public Dictionary<string, string> tagInfo { get; set; }

        public StoryStructure DeepCopy()
        {
            return new StoryStructure() { inkVersion = this.inkVersion, root = this.root, listDefs = this.listDefs, tagInfo = this.tagInfo };
        }
    }
}