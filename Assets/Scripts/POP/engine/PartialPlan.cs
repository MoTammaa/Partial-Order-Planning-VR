
namespace POP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using static System.ArgumentNullException;

    public class PartialPlan : ICloneable, IEquatable<PartialPlan>
    {
#nullable enable
        private HashSet<Action> actions;
        private HashSet<CausalLink> causalLinks;
        private BindingConstraints bindingConstraints;
        private HashSet<Tuple<Action, Action>> orderingConstraints;

        private static bool PRINT_START_FINISH_ORDERINGS = Planner.PRINT_START_FINISH_ORDERINGS, PRINT_AFTER_CONVERTING_VARIABLES = Planner.PRINT_AFTER_CONVERTING_VARIABLES;

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
        public BindingConstraints BindingConstraints
        {
            get { return bindingConstraints; }
            set { bindingConstraints = value; }
        }
        public HashSet<Tuple<Action, Action>> OrderingConstraints
        {
            get { return orderingConstraints; }
            set { orderingConstraints = value; }
        }

#nullable disable warnings
        public PartialPlan(HashSet<Action> actions, HashSet<CausalLink> causalLinks, BindingConstraints bindingConstraints, HashSet<Tuple<Action, Action>> orderingConstraints)
        {
            Helpers.ThrowIfNull(actions, nameof(actions));
            Helpers.ThrowIfNull(orderingConstraints, nameof(orderingConstraints));

            this.Actions = actions;
            this.CausalLinks = causalLinks;
            this.BindingConstraints = bindingConstraints;
            this.OrderingConstraints = orderingConstraints;

        }

        public object Clone()
        {
            var newBindConstraints = this.BindingConstraints.Clone() as BindingConstraints;
            var newOrderingConstraints = this.OrderingConstraints.Select(item => new Tuple<Action, Action>((Action)item.Item1.Clone(), (Action)item.Item2.Clone())).ToHashSet();
            var newActions = new HashSet<Action>(this.Actions.Select(action => (Action)action.Clone()));
            var newCausalLinks = new HashSet<CausalLink>(this.CausalLinks.Select(link => (CausalLink)link.Clone()));

            return new PartialPlan(newActions, newCausalLinks, newBindConstraints, newOrderingConstraints);
        }
#nullable restore warnings

        public bool isAllVariablesBound()
        {
            foreach (Action a in this.Actions)
            {
                if (a.Variables.Any(variable => !BindingConstraintsContains(variable)))
                {
                    return false;
                }
                foreach (Literal l in a.Preconditions)
                {
                    if (l.Variables.Any(variable => !BindingConstraintsContains(variable) && !Helpers.IsUpper(variable[0])))
                    {
                        return false;
                    }
                }
                foreach (Literal l in a.Effects)
                {
                    if (l.Variables.Any(variable => (!BindingConstraintsContains(variable)) && !Helpers.IsUpper(variable[0])))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Action? GetActionByName(string name)
        {
            return this.Actions.FirstOrDefault(action => action.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        public bool BindingConstraintsContains(string variable)
        {
            return this.BindingConstraints.getBoundEq(variable) is not null
                    || this.BindingConstraints.getBoundNE(variable) is not null;
        }
        public string GetBindingConstraintsBounds(string variable)
        {
            return this.BindingConstraints.getBoundEq(variable) ?? variable;
        }

        public string ActionToString(Action a)
        {
            return a.Name + "(" + string.Join(", ", a.Variables.Select(variable => BindingConstraintsContains(variable) && PRINT_AFTER_CONVERTING_VARIABLES ? GetBindingConstraintsBounds(variable) : variable)) + ")";
        }
        public string LiteralToString(Literal l)
        {
            return (l.IsPositive ? "" : " ¬") + l.Name + "(" + string.Join(", ", l.Variables.Select(variable => BindingConstraintsContains(variable) && PRINT_AFTER_CONVERTING_VARIABLES ? GetBindingConstraintsBounds(variable) : variable)) + ")";
        }


        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append("Actions: ");
            sb.Append(string.Join(", ", this.Actions.Select(ActionToString)));
            sb.Append(/*"\n\nCausal */"\n\nLinks: \n");
            sb.Append(string.Join(", \n", this.CausalLinks.Select(link => ActionToString(link.Produceri) + " --" + LiteralToString(link.LinkCondition) + "--> " + ActionToString(link.Consumerj))));
            return $"{sb}\n\nBinding Constraints: {string.Join(", ", this.BindingConstraints)}\n\nOrdering Constraints: {string.Join(", ", this.OrderingConstraints.Select(item => (item.Item1.Name == "Start" || item.Item2.Name == "Start" || item.Item1.Name == "Finish" || item.Item2.Name == "Finish") && !PRINT_START_FINISH_ORDERINGS ? "" : "(" + ActionToString(item.Item1) + " < " + ActionToString(item.Item2) + ")"))}";
        }

        public List<Action> getListOfActionsAchievers(Literal l, Action neededAction)
        {
            List<Action> actions = new List<Action>();
            foreach (Action a in this.Actions)
            {   // check if they are equal by reference
                if (a == neededAction)
                {
                    continue;
                }
                foreach (Literal e in a.Effects)
                {
                    if (e == l)
                    {



                        Dictionary<Expression, List<Expression>>? μ = Helpers.Unify(l, e, BindingConstraints);
                        if (μ is not null)
                        {
                            // if (μ.Count == l.Variables.Length)
                            actions.Add(a);
                            // break;
                        }
                    }
                }
            }
            return actions;
        }

        public bool Equals(PartialPlan? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Actions.SetEquals(other.Actions)
                && this.CausalLinks.SetEquals(other.CausalLinks)
                && this.BindingConstraints.Equals(other.BindingConstraints)
                && this.OrderingConstraints.SetEquals(other.OrderingConstraints);
        }

        public override bool Equals(object? obj)
        {
            return obj is PartialPlan plan && this.Equals(plan);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Actions, this.CausalLinks, this.BindingConstraints, this.OrderingConstraints);
        }

        public static bool operator ==(PartialPlan? left, PartialPlan? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(PartialPlan? left, PartialPlan? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(PartialPlan? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(PartialPlan? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, PartialPlan? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, PartialPlan? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }

    }
}