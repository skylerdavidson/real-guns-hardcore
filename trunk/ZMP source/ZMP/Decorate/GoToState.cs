namespace ZMP.Decorate
{
    using System;

    public sealed class GoToState : State, ICloneable
    {
        public override bool CountsAsState
        {
            get
            {
                return false;
            }
        }

        public State Target { get; set; }

        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
