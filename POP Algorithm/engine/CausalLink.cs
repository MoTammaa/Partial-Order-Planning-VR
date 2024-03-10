using System;

namespace POP
{
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

        public CausalLink()
        {

        }
    }
}