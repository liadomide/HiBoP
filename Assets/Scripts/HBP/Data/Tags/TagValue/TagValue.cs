﻿namespace HBP.Data
{
    public abstract class TagValue<T, I> : BaseTagValue where T : BaseTag
    {
        #region Properties
        public virtual new T Tag
        {
            get
            {
                return (T)base.Tag;
            }
            set
            {
                base.Tag = value;
            }
        }
        public virtual new I Value
        {
            get
            {
                dynamic value = base.Value;
                return (I)value;
            }
            set
            {
                if (Tag != null)
                {
                    base.Value = value;
                }
            }
        }
        #endregion

        #region Constructors
        public TagValue(T tag, I value, string ID) : base(tag, value, ID)
        {
            Value = value;
        }
        public TagValue(T tag, I value) : base(tag, value)
        {
            Value = value;
        }
        public TagValue() : this(null, default)
        {
        }
        #endregion

        #region Public Methods
        public override void Copy(object copy)
        {
            base.Copy(copy);
            if (copy is BaseTagValue baseTagValue)
            {
                if (baseTagValue.Tag is T tag)
                {
                    Tag = tag;
                }
                if (baseTagValue.Value is I value)
                {
                    Value = value;
                }
            }
        }
        #endregion
    }
}