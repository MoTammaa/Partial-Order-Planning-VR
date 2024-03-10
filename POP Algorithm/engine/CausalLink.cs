
namespace POP
{
    using System;
    using static System.ArgumentNullException;

    public class CausalLink
    {
        private Action produceri;
        private List<Literal> linkConditions;
        private Action consumerj;

        public Action Produceri
        {
            get { return produceri; }
            set { produceri = value; }
        }
        public List<Literal> LinkConditions
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
        public CausalLink(Action produceri, List<Literal> linkConditions, Action consumerj)
        {
            ThrowIfNull(produceri, nameof(produceri));
            ThrowIfNull(linkConditions, nameof(linkConditions));
            ThrowIfNull(consumerj, nameof(consumerj));

            this.Produceri = produceri;
            this.LinkConditions = linkConditions;
            this.Consumerj = consumerj;
        }
#nullable restore warnings
    }
}