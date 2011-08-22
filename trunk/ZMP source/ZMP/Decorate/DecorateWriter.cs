namespace ZMP.Decorate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ZMP.Utilities.Extensions;

    public class DecorateWriter
    {        
        private TextWriter writer;

        public DecorateWriter(TextWriter writer)
        {
            this.writer = writer;

            this.writer.WriteLine(Program.FileHeader);
        }

        public void WriteActors(IEnumerable<Actor> actors)
        {
            var localActors =
                from root in actors
                where root.Parent == null
                from actor in root.Descendants
                where !actor.IsFromNativeFile
                select actor;

            foreach (Actor actor in localActors)
            {
                this.WriteActor(actor);
            }
        }        

        protected virtual void WriteActor(Actor actor)
        {
            this.WriteActorHeader(actor);

            this.writer.WriteLine("{");

            this.WriteActorCombos(actor);
            this.WriteActorFlags(actor);
            this.WriteActorProperties(actor);
            this.writer.WriteLine("States{");
            this.WriteActorStateMachine(actor);
            this.writer.WriteLine("}");

            this.writer.WriteLine("}");
        }

        protected virtual void WriteActorHeader(Actor actor)
        {
            this.writer.WriteLine("actor \"" + actor.Name + "\" : \"" + actor.Parent + "\" ");

            if (actor.ReplacesName != string.Empty)
            {
                this.writer.WriteLine("replaces \"" + actor.ReplacesName + "\" ");
            }

            if (actor.IsNative)
            {
                this.writer.WriteLine("native ");
            }

            if (actor.DoomEdNum != 0)
            {
                this.writer.WriteLine(actor.DoomEdNum.ToString() + " ");
            }
        }

        protected virtual void WriteActorCombos(Actor actor)
        {
            if (actor.HasMonsterCombo)
            {
                this.writer.WriteLine("MONSTER");
            }

            if (actor.HasProjectileCombo)
            {
                this.writer.WriteLine("PROJECTILE");
            }
        }

        protected virtual void WriteActorFlags(Actor actor)
        {
            foreach (string flag in actor.EnabledFlags)
            {
                this.writer.WriteLine("+" + flag);
            }

            foreach (string flag in actor.DisabledFlags)
            {
                this.writer.WriteLine("-" + flag);
            }
        }

        protected virtual void WriteActorProperties(Actor actor)
        {
            var keyValPairs = 
                from property in actor.Properties
                from value in actor.Properties[property.Key]
                select new { Key = property.Key, Value = value };

            foreach (var pair in keyValPairs)
            {
                this.writer.WriteLine(pair.Key + " " + pair.Value);
            }
        }

        protected virtual void WriteActorStateMachine(Actor actor)
        {            
            Random r = new Random();

            var jumpTargetLabelsByState = actor.StateMachine.States.OfType<GoToState>().ToDictionary(p => p, p => "__j" + r.Next(0, int.MaxValue));
            var labelsFromJumpTargets = jumpTargetLabelsByState.ToLookup(p => p.Key.Target, p => p.Value);

            ILookup<State, string> labelsByState = actor.StateMachine.Labels.ToLookup(p => p.Value, p => p.Key).Union(labelsFromJumpTargets);

            foreach (State state in actor.StateMachine.States)
            {
                if (labelsByState.Contains(state))
                {
                    foreach (string label in labelsByState[state].Where(p => !p.Contains(':')))
                    {
                        this.writer.WriteLine(label + ":");
                    }
                }

                if (state is FrameState)
                {
                    FrameState frameState = state as FrameState;
                    this.writer.WriteLine(frameState.Sprite + " " + frameState.Frame + " " + frameState.Duration + (frameState.IsBright ? " BRIGHT " : " ") + frameState.CodePointer);
                }
                else if (state is GoToState)
                {
                    this.writer.WriteLine("goto " + jumpTargetLabelsByState[state as GoToState]);
                }
                else if (state is FailState)
                {
                    this.writer.WriteLine("fail");
                }
                else if (state is StopState)
                {
                    this.writer.WriteLine("stop");
                }
            }
        }
    }
}
