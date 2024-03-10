
namespace POP
{
    using Agenda = System.Collections.Generic.List<System.Tuple<POP.Action, POP.Literal>>;

    public class PartialPlan
    {
        private List<Action> actions;
        private List<CausalLink> causalLinks;
        private List<BindingConstraint> bindingConstraints;
        private List<Tuple<Action, Action>> orderingConstraints;
        private Agenda agenda;

        public PartialPlan()
        {

        }
    }
}