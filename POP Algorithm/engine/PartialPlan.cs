
namespace POP
{
    using Agenda = System.Collections.Generic.List<System.Tuple<POP.Action, POP.Literal>>;
    using static System.ArgumentNullException;

    public class PartialPlan
    {
        private List<Action> actions;
        private List<CausalLink> causalLinks;
        private List<BindingConstraint> bindingConstraints;
        private List<Tuple<Action, Action>> orderingConstraints;
        private Agenda agenda;

        public PartialPlan(List<Action> actions, List<CausalLink> causalLinks, List<BindingConstraint> bindingConstraints, List<Tuple<Action, Action>> orderingConstraints, Agenda agenda)
        {
            ThrowIfNull(actions, nameof(actions));
            ThrowIfNull(orderingConstraints, nameof(orderingConstraints));

            this.actions = actions;
            this.causalLinks = causalLinks;
            this.bindingConstraints = bindingConstraints;
            this.orderingConstraints = orderingConstraints;
            this.agenda = agenda;

        }
    }
}