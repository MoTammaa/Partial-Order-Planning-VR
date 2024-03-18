
namespace POP
{
    using System;
    using static System.ArgumentNullException;

    public class CausalLink : ICloneable
    {
        private Action produceri;
        private Literal linkConditions;
        private Action consumerj;

        public Action Produceri
        {
            get { return produceri; }
            set { produceri = value; }
        }
        public Literal LinkConditions
        {
            get { return linkConditions; }
            set { linkConditions = value; }
        }
        public Action Consumerj
        {
            get { return consumerj; }
            set { consumerj = value; }
        }

#nullable disable warnings
        public CausalLink(Action produceri, Literal linkConditions, Action consumerj)
        {
            ThrowIfNull(produceri, nameof(produceri));
            ThrowIfNull(linkConditions, nameof(linkConditions));
            ThrowIfNull(consumerj, nameof(consumerj));

            this.Produceri = produceri;
            this.LinkConditions = linkConditions;
            this.Consumerj = consumerj;
        }

#nullable restore warnings
        public object Clone()
        {
            return new CausalLink((Action)this.Produceri.Clone(), new Literal(this.LinkConditions), (Action)this.Consumerj.Clone());
        }
    }
}