
namespace POP
{
    using static System.ArgumentNullException;

    public class PartialPlan
    {
        private List<Action> actions;
        private List<CausalLink> causalLinks;
        private List<BindingConstraint> bindingConstraints;
        private List<Tuple<Action, Action>> orderingConstraints;

        public List<Action> Actions
        {
            get { return actions; }
            set { actions = value; }
        }
        public List<CausalLink> CausalLinks
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
        public PartialPlan(List<Action> actions, List<CausalLink> causalLinks, List<BindingConstraint> bindingConstraints, List<Tuple<Action, Action>> orderingConstraints)
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