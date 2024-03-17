
namespace POP
{
    using static System.ArgumentNullException;

    public class PartialPlan
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
#nullable restore warnings
    }
}