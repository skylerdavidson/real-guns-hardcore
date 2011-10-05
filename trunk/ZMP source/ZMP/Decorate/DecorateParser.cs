namespace ZMP.Decorate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;    
    using ZMP.Utilities.Extensions;

    public enum DecorateFileType
    {
        Native,
        User
    }

    public class DecorateParser
    {
        private readonly List<DecorateFileInfo> files = new List<DecorateFileInfo>();

        public IEnumerable<DecorateFileInfo> Files 
        { 
            get 
            { 
                return this.files; 
            }
        }

        public void AddFile(FileInfo file, DecorateFileType fileType = DecorateFileType.User)
        {
            this.files.Add(new DecorateFileInfo(file, fileType));
        }

        public void AddFiles(IEnumerable<FileInfo> files, DecorateFileType fileType = DecorateFileType.User)
        {
            foreach (var file in files)
            {
                this.AddFile(file, fileType);
            }
        }

        public IEnumerable<Actor> GetActors()
        {
            Dictionary<string, Actor> actorsByName = this.GetActorsWithoutDependencies().Distinct().ToDictionary(actor => actor.Name);

            // create parent-chlid relationships
            foreach (var actorPair in actorsByName)
            {
                if (actorPair.Value.ParentName != string.Empty)
                {
                    try
                    {
                        actorPair.Value.Parent = actorsByName[actorPair.Value.ParentName.ToLower()];
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new KeyNotFoundException("Actor \"" + actorPair.Value.ParentName + "\" referenced as parent by \"" + actorPair.Key + "\" was not found.");
                    }
                }

                if (actorPair.Value.ReplacesName != string.Empty)
                {
                    try
                    {
                        actorPair.Value.Replaces = actorsByName[actorPair.Value.ReplacesName.ToLower()];
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new KeyNotFoundException("Actor \"" + actorPair.Value.ReplacesName + "\" replaced by \"" + actorPair.Key + "\" was not found.");
                    }
                }
            }

            // parse states in topological order
            var actors = 
                from rootActor in actorsByName.Values
                where rootActor.Parent == null
                from actor in rootActor.DescendantsOrSelf
                select actor;

            foreach (var actor in actors)
            {
                actor.EvaluateCustomProperties();
                actor.StateMachine = this.ParseStateMachine(actor);
            }

            return actorsByName.Values;
        }

        public IEnumerable<Actor> GetActorsWithoutDependencies()
        {
            foreach (DecorateFileInfo file in this.Files.AsParallel().AsUnordered())
            {
                foreach (Actor actor in this.GetActorsFromFile(file))
                {
                    yield return actor;
                }
            }
        }

        protected IEnumerable<Actor> GetActorsFromFile(DecorateFileInfo file)
        {
            Console.WriteLine("    Parsing file " + file.FileInfo.FullName);
            //// if(cached && cacheValid){
            ////      load from cache
            //// }
            //// else 
            //// {
            string text = string.Empty;
            using (var reader = file.FileInfo.OpenText())
            {
                text = reader.ReadToEnd();
            }

            // remove comments
            text = Regex.Replace(text, "//(.*?)\\n", Environment.NewLine, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "/\\*(.*?)\\*/", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);            

            // remove action and const definitions
            if (file.Origin == DecorateFileType.Native)
            {
                text = Regex.Replace(text, "(action)?\\s*(native|const)?(.*?);", string.Empty, RegexOptions.IgnoreCase);
            }

            string actorPattern = @"
                actor\s+
                    ([A-Za-z0-9_]+|"".*?"")                     # actor name (with or without quotes)
                    \s*
                    (:\s*([A-Za-z0-9_]+|"".*?"")\s*)?           # parent name (with or without quotes)
                    (replaces\s*([A-Za-z0-9_]+|"".*?"")\s*)?    # replaced actor name (with or without quotes)
                    (\d*\s*)?                                   # doomEdNum 
                    (native\s*)?                                # native yes/no
                {
                    (.*?)                                       # properties and flags
                    (states\s*{(.*?)}\s*)?                      # state machine
                }
            ";
            
            var matches = Regex.Matches(text, actorPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            foreach (Match match in matches)
            {
                string fullText = match.Groups[0].Value;
                string name = match.Groups[1].Value;
                string parent = match.Groups[3].Value;
                string replaces = match.Groups[5].Value;
                string doomednum = match.Groups[6].Value;
                string native = match.Groups[7].Value;                
                string content = match.Groups[8].Value;
                string statesContent = match.Groups[10].Value;

                Actor actor = null;

                actor = new Actor()
                {
                    File = file.FileInfo,
                    Name = name,
                    ParentName = parent == string.Empty ? (name.ToLower() == "actor" ? string.Empty : "actor") : parent,
                    ReplacesName = replaces,
                    IsNative = native != string.Empty,
                    IsFromNativeFile = file.Origin == DecorateFileType.Native,
                    DoomEdNum = doomednum != string.Empty ? Int32.Parse(doomednum) : 0,
                    PropertiesAndFlagsText = content,
                    StatesText = statesContent,
                    OriginalText = fullText
                };

                yield return actor;
            }            
        }

        protected StateMachine ParseStateMachine(Actor actor)
        {
            string[] lines = actor.StatesText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            var states = new List<State>();
            var labels = new Dictionary<string, State>();
            
            // non-root actors inheirit state machine from their parents
            if (actor.Parent != null)
            {
                StateMachine parentMachine = actor.Parent.StateMachine.CloneForChild(actor.Parent.Name);
                states = parentMachine.States.ToList();
                labels = parentMachine.Labels.ToDictionary(p => p.Key, p => p.Value);
            }

            var openGoTos = new Dictionary<GoToState, KeyValuePair<string, int>>();

            List<string> openLabels = new List<string>();

            string lastLabel = string.Empty;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed == string.Empty)
                {
                    continue;
                }

                string framePattern = @"
                    ([A-Za-z0-9_\-""#]{4,6})    # sprite name
                    \s+
                    ([A-Za-z\[\]\\""#]+)         # frame list
                    \s+                         
                    (-?\d+)                     # duration
                    (\s+bright\s+)              # bright yes/no
                    ?(.*)                       # code pointer
                ";

                Match labelMatch = Regex.Match(line, "([A-Za-z0-9_.]+):", RegexOptions.IgnoreCase);
                Match gotoMatch = Regex.Match(line, @"goto\s+([A-Za-z0-9_:.]+)?(\s*\+\s*(\d+))?", RegexOptions.IgnoreCase);
                Match frameMatch = Regex.Match(line, framePattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                                
                List<State> currentLineStates = new List<State>();

                if (trimmed.Equals("fail", StringComparison.OrdinalIgnoreCase))
                {
                    currentLineStates.Add(new FailState());
                }
                else if (trimmed.Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    currentLineStates.Add(new StopState());
                }
                else if (trimmed.Equals("loop", StringComparison.OrdinalIgnoreCase))
                {
                    GoToState state = new GoToState();
                    openGoTos.Add(state, new KeyValuePair<string, int>(lastLabel, 0));
                    currentLineStates.Add(state);
                }
                else if (trimmed.Equals("wait", StringComparison.OrdinalIgnoreCase))
                {
                    GoToState state = new GoToState();
                    state.Target = states.Last();
                    currentLineStates.Add(state);
                }
                else if (gotoMatch.Success)
                {
                    int offset = 0;

                    int.TryParse(gotoMatch.Groups[3].Value, out offset);

                    State state = new GoToState();
                    openGoTos.Add(state as GoToState, new KeyValuePair<string, int>(gotoMatch.Groups[1].Value.ToLower(), offset));
                    currentLineStates.Add(state);
                }
                else if (labelMatch.Success)
                {
                    openLabels.Add(labelMatch.Groups[1].Value.ToLower());
                    lastLabel = labelMatch.Groups[1].Value.ToLower();
                }
                else if (frameMatch.Success)
                {
                    foreach (char frame in frameMatch.Groups[2].Value.TrimQuotes())
                    {
                        FrameState newState = new FrameState()
                        {
                            Sprite = frameMatch.Groups[1].Value,
                            Frame = frame,
                            Duration = int.Parse(frameMatch.Groups[3].Value),
                            IsBright = frameMatch.Groups[4].Value != string.Empty,
                            CodePointer = frameMatch.Groups[5].Value
                        };

                        currentLineStates.Add(newState);
                    }
                }
                else
                {
                    throw new Exception("Could not parse state line \"" + trimmed + "\" in actor \"" + actor.Name + "\" in file \"" + actor.File.FullName + "\".");
                }
                
                if (currentLineStates.Count > 0)
                {
                    foreach (string label in openLabels)
                    {
                        labels[label] = currentLineStates[0];
                    }

                    openLabels.Clear();
                }                

                states.AddRange(currentLineStates);
            }

            foreach (var openGoTo in openGoTos)
            {
                GoToState state = openGoTo.Key;
                string targetLabel = openGoTo.Value.Key;
                int offset = openGoTo.Value.Value;

                State targetState = null;

                if (targetLabel == string.Empty)
                {
                    targetState = state;
                }
                else
                {
                    if (!labels.ContainsKey(targetLabel))
                    {
                        throw new Exception("Jump target \"" + targetLabel + "\" not found in actor \"" + actor.Name + "\" in file \"" + actor.File.FullName + "\".");
                    }

                    targetState = labels[targetLabel];
                }

                int stateIndex = states.IndexOf(targetState);

                Debug.Assert(stateIndex >= 0, "Goto target state not found!");

                state.Target = states[stateIndex + offset];

                // only FrameStates count towards the offset
                int numStatesToSkip = offset;
                int i = stateIndex;
                state.Target = states[i];
                while (numStatesToSkip >= 1)
                {
                    i++;
                    state.Target = states[i];

                    if (state.Target.CountsAsState)
                    {
                        numStatesToSkip--;
                    }
                }
            }

            return new StateMachine(states, labels);
        }
    }
}
