
namespace POP
{
    using static System.ArgumentNullException;

    public class PartialPlan : ICloneable
    {
        private HashSet<Action> actions;
        private HashSet<CausalLink> causalLinks;
        private List<BindingConstraint> bindingConstraints;
        private List<Tuple<Action, Action>> orderingConstraints;

        public HashSet<Action> Actions
        {
            get { return actions; }
            set { actions = value; }
        }
        public HashSet<CausalLink> CausalLinks
        {
            get { return causalLinks; }
            set { causalLinks = value; }
        }
        public List<BindingConstraint> BindingConstraints
        {
            get { return bindingConstraints; }
            set { bindingConstraints = value; }
        }
        public List<Tuple<Action, Action>> OrderingConstraints
        {
            get { return orderingConstraints; }
            set { orderingConstraints = value; }
        }

#nullable disable warnings
        public PartialPlan(HashSet<Action> actions, HashSet<CausalLink> causalLinks, List<BindingConstraint> bindingConstraints, List<Tuple<Action, Action>> orderingConstraints)
        {
            ThrowIfNull(actions, nameof(actions));
            ThrowIfNull(orderingConstraints, nameof(orderingConstraints));

            this.Actions = actions;
            this.CausalLinks = causalLinks;
            this.BindingConstraints = bindingConstraints;
            this.OrderingConstraints = orderingConstraints;

        }

        public object Clone()
        {
            var newBindConstraints = this.BindingConstraints.Select(item => (BindingConstraint)item.Clone()).ToList();
            var newOrderingConstraints = this.OrderingConstraints.Select(item => new Tuple<Action, Action>((Action)item.Item1.Clone(), (Action)item.Item2.Clone())).ToList();
            var newActions = new HashSet<Action>(this.Actions.Select(action => (Action)action.Clone()));
            var newCausalLinks = new HashSet<CausalLink>(this.CausalLinks.Select(link => (CausalLink)link.Clone()));

            return new PartialPlan(newActions, newCausalLinks, newBindConstraints, newOrderingConstraints);
        }
#nullable restore warnings
    }
}