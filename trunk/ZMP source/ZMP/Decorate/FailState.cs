namespace ZMP.Decorate
{
    using System;

    public sealed class FailState : State, ICloneable
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
            return "fail";
        }

        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
