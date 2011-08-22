namespace ZMP.Decorate
{
    using System;

    public sealed class StopState : State, ICloneable
    {
        public override bool CountsAsState
        {
            get
            {
                return false;
            }
        }

        public override string ToString()
        {
            return "stop";
        }

        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
