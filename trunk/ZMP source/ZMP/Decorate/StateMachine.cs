namespace ZMP.Decorate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ZMP.Utilities.Extensions;

    public class StateMachine : ICloneable
    {
        private List<State> states;
        private Dictionary<string, State> labels;

        public StateMachine()
        {
            this.states = new List<State>();
            this.labels = new Dictionary<string, State>();
        }

        public StateMachine(IEnumerable<State> states, IDictionary<string, State> labels)
        {
            this.states = states.ToList();
            this.labels = labels.ToDictionary(p => p.Key, p => p.Value);
        }

        public IEnumerable<State> States
        {
            get
            {
                return this.states;
            }
        }
        
        public IDictionary<string, State> Labels
        {
            get
            {
                return this.labels;
            }
        }        

        public object Clone()
        {             
            var statesAsOriginalToClone = this.States.ToDictionary(p => p, p => p.Clone() as State);
            
            var states = statesAsOriginalToClone.Select(p => p.Value);

            // reroute gotos to the new array
            foreach (var state in states.OfType<GoToState>())
            {
                state.Target = statesAsOriginalToClone[state.Target];
            }

            // reroute labels to the new array
            var labels = this.Labels.ToDictionary(p => p.Key, p => statesAsOriginalToClone[p.Value]);

            return new StateMachine(states, labels);
        }

        public StateMachine CloneForChild(string currentActorName)
        {
            var clone = this.Clone() as StateMachine;

            var newLabels = clone.Labels.ToDictionary(p => p.Key, p => p.Value);
            foreach (var label in clone.Labels)
            {
                if (!label.Key.Contains(':'))
                {
                    newLabels["super::" + label.Key] = label.Value;
                    newLabels.Add(currentActorName + "::" + label.Key, label.Value);                     
                }
            }

            clone.labels = newLabels;

            return clone;
        }

        public void InsertStatesBeforeState(IEnumerable<State> statesToInsert, State targetState)
        {
            if (!statesToInsert.Any())
            {
                return;
            }
            
            int index = this.states.IndexOf(targetState);

            if (index == -1)
            {
                throw new InvalidOperationException("Given state is not owned by this state machine.");
            }

            this.states.InsertRange(index, statesToInsert);

            State firstInsertedState = statesToInsert.First();

            foreach (var label in this.labels.Where(p => p.Value == targetState).ToList())
            {
                this.labels[label.Key] = firstInsertedState;
            }
        }
    }
}
