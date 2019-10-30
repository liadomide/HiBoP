﻿using System.ComponentModel;

namespace HBP.Data.Tags
{
    [DisplayName("Empty")]
    public class EmptyTag : Tag
    {
        #region Constructors
        public EmptyTag() : base()
        {
        }
        public EmptyTag(string name) : base(name)
        {
        }
        public EmptyTag(string name, string ID) : base(name, ID)
        {

        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            return new EmptyTag(Name, ID);
        }
        #endregion
    }
}