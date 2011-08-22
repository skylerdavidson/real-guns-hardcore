namespace ZMP.Decorate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class FrameState : State, ICloneable
    {
        public FrameState()
        {
            this.Sprite = "TNT1";
            this.Frame = 'A';
            this.Duration = 0;
            this.Light = string.Empty;
        }

        public override bool CountsAsState
        {
            get
            {
                return true;
            }
        }

        public string Sprite { get; set; }

        public char Frame { get; set; }

        public int Duration { get; set; }

        public bool IsBright { get; set; }

        public string Light { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        public string CodePointer { get; set; }

        public IEnumerable<string> Parameters { get; set; }

        public IEnumerable<State> StateParameters { get; set; }        

        public override string ToString()
        {
            return this.Sprite + " " + this.Frame + " " + this.Duration + (this.IsBright ? " BRIGHT " : " ") + this.CodePointer;
        }

        object ICloneable.Clone()
        {
            FrameState clone = this.MemberwiseClone() as FrameState;

            clone.Parameters = this.Parameters.ToList();
            clone.StateParameters = this.StateParameters.ToList();

            return clone;
        }
    }
}
