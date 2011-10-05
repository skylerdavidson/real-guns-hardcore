namespace ZMP.Decorate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;
    using ZMP.Utilities;
    using ZMP.Utilities.Extensions;

    [DataContract]
    public class Actor
    {
        private static int maxId = 1;

        [DataMember]
        private int id;

        [DataMember]
        private FileInfo file;

        [DataMember]
        private string originalName = string.Empty;

        [DataMember]
        private string parentName = string.Empty;

        [DataMember]
        private Actor parent;        

        [DataMember]
        private string replacesName = string.Empty;

        [DataMember]
        private Actor replaces;

        [DataMember]
        private bool isNative;

        [DataMember]
        private bool isFromNativeFile;

        [DataMember]
        private int doomEdNum;

        [DataMember]
        private string propertiesAndFlagsText = string.Empty;

        [DataMember]
        private string statesText = string.Empty;

        [DataMember]
        private string originalText = string.Empty;

        [DataMember]
        private List<Actor> children = new List<Actor>();

        [DataMember]
        private List<Actor> replacedBy = new List<Actor>();

        [DataMember]
        private List<string> enabledFlags = new List<string>();

        [DataMember]
        private List<string> disabledFlags = new List<string>();

        [DataMember]
        private ILookup<string, string> properties = Enumerable.Empty<string>().ToLookup(p => string.Empty, p => string.Empty);

        [DataMember]
        private ILookup<string, string> customProperties = Enumerable.Empty<string>().ToLookup(p => string.Empty, p => string.Empty);

        [DataMember]
        private bool hasMonsterCombo;

        [DataMember]
        private bool hasProjectileCombo;        

        [DataMember]
        private StateMachine stateMachine = new StateMachine();

        public Actor()
        {
            this.id = Actor.maxId;
            Actor.maxId++;
        }

        public int ID
        {
            get
            {
                return this.id;
            }
        }

        public FileInfo File
        {
            get
            {
                return this.file;
            }

            set
            {
                this.file = value;
            }
        }

        public string Name
        {
            get
            {
                return this.originalName.ToLower();
            }

            set
            {
                this.originalName = value.TrimQuotes();
            }
        }

        public string OriginalName
        {
            get
            {
                return this.originalName;
            }
        }

        public string Tag
        {
            get
            {
                // standard HasProperty/GetPropertyValue accessors can't be used, tag is never inherited
                if (this.Properties.Contains("tag"))
                {
                    string tag = this.Properties["tag"].First();
                    return tag.Substring(1, tag.Length - 2);
                }
                else
                {
                    return this.OriginalName;
                }
            }
        }

        public Actor Parent
        {
            get
            {
                return this.parent;
            }

            set
            {
                if (this.parentName == string.Empty || value.Name == this.parentName)
                {
                    value.children.Add(this);
                    this.parent = value;
                }
                else
                {
                    throw new Exception("Parent string doesn't match");
                }
            }
        }

        public string ParentName
        {
            get
            {
                if (this.parent == null)
                {
                    return this.parentName;
                }
                else
                {
                    return this.parent.Name;
                }
            }

            set
            {
                this.parentName = value == string.Empty ? string.Empty : value.TrimQuotes().ToLower();
                this.parent = null;
            }
        }

        public Actor Replaces
        {
            get
            {
                return this.replaces;
            }

            set
            {
                if (this.replacesName == string.Empty || value.Name == this.ReplacesName)
                {
                    value.replacedBy.Add(this);
                    this.replaces = value;
                }
                else
                {
                    throw new Exception("Replaces string doesn't match");
                }
            }
        }

        public string ReplacesName
        {
            get
            {
                if (this.replaces == null)
                {
                    return this.replacesName;
                }                
                else
                {
                    return this.replaces.Name;
                }
            }

            set
            {
                this.replacesName = value.TrimQuotes().ToLower();
                this.replaces = null;
            }
        }

        public bool IsNative
        {
            get
            {
                return this.isNative;
            }

            set
            {
                this.isNative = value;
            }
        }

        public bool IsFromNativeFile
        {
            get
            {
                return this.isFromNativeFile;
            }

            set
            {
                this.isFromNativeFile = value;
            }
        }

        public int DoomEdNum
        {
            get
            {
                return this.doomEdNum;
            }

            set
            {
                this.doomEdNum = value;
            }
        }

        public bool HasMonsterCombo
        {
            get
            {
                return this.hasMonsterCombo;
            }

            set
            {
                this.hasMonsterCombo = value;
                this.propertiesAndFlagsText = string.Empty;
            }
        }

        public bool HasProjectileCombo
        {
            get
            {
                return this.hasProjectileCombo;
            }

            set
            {
                this.hasProjectileCombo = value;
                this.propertiesAndFlagsText = string.Empty;
            }
        }

        public string PropertiesAndFlagsText
        {
            get
            {
                return this.propertiesAndFlagsText;
            }

            set
            {
                this.propertiesAndFlagsText = value;
                this.ParseFlagsAndProperties(this.propertiesAndFlagsText);
            }
        }

        public string StatesText
        {
            get
            {
                return this.statesText;
            }

            set
            {
                this.statesText = value;
                this.stateMachine = new StateMachine();
            }
        }        

        public IEnumerable<Actor> Ancestors
        {
            get
            {
                if (this.Parent != null)
                {
                    yield return this.Parent;

                    foreach (Actor ancestor in this.Parent.Ancestors)
                    {
                        yield return ancestor;
                    }
                }
            }
        }

        public IEnumerable<Actor> AncestorsOrSelf
        {
            get
            {
                yield return this;

                foreach (var actor in this.Ancestors)
                {
                    yield return actor;
                }
            }
        }

        public IEnumerable<Actor> Children
        {
            get
            {
                return this.children;
            }
        }

        public IEnumerable<Actor> ReplacedBy
        {
            get
            {
                return this.replacedBy;
            }
        }

        public IEnumerable<Actor> ReplacementTree
        {
            get
            {
                yield return this;

                foreach (Actor replacer in this.ReplacedBy)
                {
                    foreach (Actor replacer2 in replacer.ReplacementTree)
                    {
                        yield return replacer2;
                    }
                }
            }
        }

        public IEnumerable<Actor> Descendants
        {
            get
            {
                foreach (Actor child in this.Children)
                {
                    yield return child;

                    foreach (Actor descendant in child.Descendants)
                    {
                        yield return descendant;
                    }
                }
            }
        }

        public IEnumerable<Actor> DescendantsOrSelf
        {
            get
            {
                yield return this;

                foreach (var actor in this.Descendants)
                {
                    yield return actor;
                }
            }
        }

        public IEnumerable<string> EnabledFlags
        {
            get
            {
                return this.enabledFlags;
            }
        }

        public IEnumerable<string> DisabledFlags
        {
            get
            {
                return this.disabledFlags;
            }
        }

        public ILookup<string, string> Properties
        {
            get
            {
                return this.properties;
            }

            set
            {
                this.properties = value;
                this.propertiesAndFlagsText = string.Empty;
            }
        }

        public ILookup<string, string> CustomProperties
        {
            get
            {
                return this.customProperties;
            }

            set
            {
                this.customProperties = value;
                this.propertiesAndFlagsText = string.Empty;
            }
        }

        public StateMachine StateMachine
        {
            get
            {
                return this.stateMachine;
            }

            set
            {
                this.stateMachine = value;
                this.statesText = string.Empty;
            }
        }

        internal string OriginalText
        {
            get
            {
                return this.originalText;
            }

            set
            {
                this.originalText = value;
            }
        }

        public override string ToString()
        {
            return this.OriginalName;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj as Actor == null)
            {
                return false;
            }

            return this.Name == (obj as Actor).Name;
        }

        public IEnumerable<string> GetActualFlags()
        {
            if (this.Parent == null)
            {
                return this.EnabledFlags;
            }
            
            return this.
                Parent.
                GetActualFlags().
                Union(this.GetFlagsFromCombos()).
                Union(this.EnabledFlags).
                Except(this.DisabledFlags).
                Distinct();
        }

        public bool HasFlagEnabled(string flag)
        {
            return this.GetActualFlags().Where(p => p == flag).Any();
        }

        public bool HasProperty(string property)
        {
            string propertyLower = property.ToLower();

            return this.properties.Contains(propertyLower) || (this.Parent != null && this.Parent.HasProperty(propertyLower));
        }

        public string GetPropertyValue(string property)
        {
            string propertyLower = property.ToLower();
            
            // inherit the value from parent if it is not present in this actor
            if (this.properties.Contains(propertyLower))
            {
                return this.Properties[propertyLower].First();
            }
            else if (this.Parent != null)
            {
                return this.Parent.GetPropertyValue(propertyLower);
            }
            else
            {
                return string.Empty;
            }
        }

        public IEnumerable<string> GetPropertyValues(string property)
        {
            string propertyLower = property.ToLower();

            // inherit the value from parent if it is not present in this actor
            if (this.properties.Contains(propertyLower))
            {
                return this.Properties[propertyLower];
            }
            else if (this.Parent != null)
            {
                return this.Parent.GetPropertyValues(propertyLower);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        public ILookup<string, string> GetActualProperties()
        {
            if (this.Parent == null)
            {
                return this.Properties;
            }

            var keyValPairs =
                from property in this.Properties
                from value in property
                select new KeyValuePair<string, string>(property.Key, value);

            var parentKeyValPairs =
                from property in this.Parent.Properties
                where !keyValPairs.Where(p => p.Key == property.Key).Any()
                from value in property
                select new KeyValuePair<string, string>(property.Key, value);

            return keyValPairs.Union(parentKeyValPairs).ToLookup(p => p.Key, p => p.Value);
        }

        public bool HasCustomProperty(string property)
        {
            string propertyLower = property.ToLower();

            return this.customProperties.Contains(propertyLower) || (this.Parent != null && this.Parent.HasCustomProperty(propertyLower));
        }

        public string GetCustomPropertyValue(string property)
        {
            string propertyLower = property.ToLower();
            
            // inherit the value from parent if it is not present in this actor
            if (this.customProperties.Contains(propertyLower))
            {
                return this.Properties[propertyLower].First();
            }
            else if (this.Parent != null)
            {
                return this.Parent.GetCustomPropertyValue(propertyLower);
            }
            else
            {
                return string.Empty;
            }
        }

        public IEnumerable<string> GetCustomPropertyValues(string property)
        {
            string propertyLower = property.ToLower();

            // inherit the value from parent if it is not present in this actor
            if (this.customProperties.Contains(propertyLower))
            {
                return this.CustomProperties[propertyLower];
            }
            else if (this.Parent != null)
            {
                return this.Parent.GetCustomPropertyValues(propertyLower);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        public ILookup<string, string> GetActualCustomProperties()
        {
            if (this.Parent == null)
            {
                return this.CustomProperties;
            }

            var keyValPairs =
                from property in this.CustomProperties
                from value in property
                select new KeyValuePair<string, string>(property.Key, value);

            var parentKeyValPairs =
                from property in this.Parent.CustomProperties
                where !keyValPairs.Where(p => p.Key == property.Key).Any()
                from value in property
                select new KeyValuePair<string, string>(property.Key, value);

            return keyValPairs.Union(parentKeyValPairs).ToLookup(p => p.Key, p => p.Value);
        }

        public void EvaluateCustomProperties()
        {
            this.customProperties = this.customProperties.Map((key, val) => this.EvaluateCustomPropertiesForString(val));
            this.properties = this.properties.Map((key, val) => this.EvaluateCustomPropertiesForString(val));

            this.statesText = this.EvaluateCustomPropertiesForString(this.statesText);
            this.stateMachine = new StateMachine();
        }

        private string EvaluateCustomPropertiesForString(string str)
        {
            return Regex.Replace(
                input: str,
                pattern: @"\$(PARENT::)?([a-z_]+)",
                options: RegexOptions.IgnoreCase,
                evaluator: delegate(Match match)
                {
                    // use parent if PARENT:: was present
                    Actor actor;
                    if (match.Groups[1].Value != string.Empty())
                    {
                        actor = this.Parent;
                    }
                    else
                    {
                        actor = this;
                    }

                    var propertyValues = actor.GetCustomPropertyValues(match.Groups[2].Value);

                    if (propertyValues.Any())
                    {
                        return propertyValues.First();
                    }
                    else
                    {
                        // ingore the property if there is no suitable replacement
                        return match.Value;
                    }
                });
        }

        private IEnumerable<string> GetFlagsFromCombos()
        {
            if (this.hasMonsterCombo)
            {
                yield return "SHOOTABLE";
                yield return "COUNTKILL";
                yield return "SOLID";
                yield return "CANPUSHWALLS";
                yield return "CANUSEWALLS";
                yield return "ACTIVATEMCROSS";
                yield return "CANPASS";
                yield return "ISMONSTER";
            }

            if (this.hasProjectileCombo)
            {
                yield return "NOBLOCKMAP";
                yield return "NOGRAVITY";
                yield return "DROPOFF";
                yield return "MISSILE";
                yield return "ACTIVATEIMPACT";
                yield return "ACTIVATEPCROSS";
                yield return "NOTELEPORT";

                // yield return "BLOODSPATTER"; // this is used in Hexen and Heretic only
            }
        }

        private void ParseFlagsAndProperties(string text)
        {
            string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            var temporaryPropertyTable = new List<KeyValuePair<string, string>>();
            var temporaryCustomPropertyTable = new List<KeyValuePair<string, string>>();

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed == string.Empty)
                {
                    continue;
                }

                if (
                    trimmed.StartsWith("native ", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("const ", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("action ", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (trimmed[0] == '+')
                {
                    this.enabledFlags.Add(trimmed.Substring(1).ToUpper());
                }
                else if (trimmed[0] == '-')
                {
                    this.disabledFlags.Add(trimmed.Substring(1).ToUpper());
                }
                else if (trimmed.ToLower() == "projectile")
                {
                    this.hasProjectileCombo = true;
                }
                else if (trimmed.ToLower() == "monster")
                {
                    this.hasMonsterCombo = true;
                }
                else if (trimmed[0] == '$')
                {
                    var match = Regex.Match(trimmed.Substring(1), "([A-Za-z0-9\\.]+)\\s+(.*)?");

                    string name = match.Groups[1].Value.ToLower();
                    string value = match.Groups[2].Value;

                    temporaryCustomPropertyTable.Add(new KeyValuePair<string, string>(name, value));
                }
                else
                {
                    var match = Regex.Match(trimmed, "([A-Za-z0-9\\.]+)\\s+(.*)?");

                    string name = match.Groups[1].Value.ToLower();
                    string value = match.Groups[2].Value;

                    temporaryPropertyTable.Add(new KeyValuePair<string, string>(name, value));
                }
            }

            this.properties = temporaryPropertyTable.ToLookup(p => p.Key, p => p.Value);
            this.customProperties = temporaryCustomPropertyTable.ToLookup(p => p.Key, p => p.Value);
        }
    }
}
