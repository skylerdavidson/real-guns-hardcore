namespace ZMP.Decorate
{
    using System;

    public abstract class State : ICloneable
    {
        public abstract bool CountsAsState { get; }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }        
    }    
}
