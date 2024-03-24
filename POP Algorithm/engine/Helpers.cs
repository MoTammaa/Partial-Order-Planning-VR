
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class Helpers
    {
        public static Dictionary<Expression, List<Expression>>? Unify(Expression e1, Expression e2)
        {
            return Unify(e1, e2, []);

        }
        public static Dictionary<Expression, List<Expression>>? Unify(Expression? e1, Expression? e2, Dictionary<Expression, List<Expression>>? μ)
        {
            if (e1 == null || e2 == null)
                return null; // failure
            if (μ == null)
                return null; // failure
            if (e1.Equals(e2))
                return μ;
            if (e1.IsVariable)
                return UnifyVar(e1, e2, μ);
            if (e2.IsVariable)
                return UnifyVar(e2, e1, μ);
            if (e1.IsConstant || e2.IsConstant) // if ATOM(e1) or ATOM(e2)
                return null; // failure
            if (!e1.Name.Equals(e2.Name) || e1.Arguments?.Count != e2.Arguments?.Count)
                return null; // failure
            if (e1.Arguments != null && e2.Arguments != null)
            {
                for (int i = 0; i < e1.Arguments.Count && i < e2.Arguments.Count; i++)
                {
                    μ = Unify(e1.Arguments[i], e2.Arguments[i], μ);
                    if (μ == null)
                        return null; // failure
                }
            }
            return μ;
        }

        public static Dictionary<Expression, List<Expression>>? UnifyVar(Expression xvar, Expression e, Dictionary<Expression, List<Expression>> μ)
        {
            if (μ.ContainsKey(xvar))
                // choose the first binding
                for (int i = 0; i < μ[xvar].Count; i++)
                    if (!μ[xvar][i].Equals(xvar))
                        return Unify(μ[xvar][i], e, μ);

            Expression t = Subst(μ, e);
            if (t.Contains(xvar))
                return null; // failure

            // compose {t/xvar} with μ <==> {t/xvar} ∘ μ

            μ[(Expression)xvar.Clone()] = [t];
            // create a new dictionary and add t/xvar
            Dictionary<Expression, List<Expression>> μ2 = new() { [xvar] = [t] };
            // update each occurrence of xvar in μ values to t
            foreach (Expression key in μ.Keys)
            {
                for (int i = 0; i < μ[key].Count; i++)
                {
                    μ[key][i] = Subst(μ2, μ[key][i]);
                }
            }
            return μ;
        }

        private static Expression Subst(Dictionary<Expression, List<Expression>> μ, Expression e)
        {
            if (e.IsVariable && μ.ContainsKey(e))
                return μ[e][0];
            if (e.IsPredicate && e.Arguments != null)
            {
                List<Expression> args = [];
                foreach (Expression arg in e.Arguments)
                {
                    args.Add(Subst(μ, arg));
                }
                return new Expression(e.Name, args);
            }
            return e;
        }


    }

    public class Expression : IEquatable<Expression>, ICloneable
    {
        public string Name { get; set; }
        public List<Expression>? Arguments { get; set; }

        private bool isVariable, isConstant;
        public bool IsPredicate => Arguments != null;
        public bool IsConstant
        {
            get { return isConstant; }
            set { isConstant = value && !IsPredicate; isVariable = !value && !IsPredicate; }
        }
        public bool IsVariable
        {
            get { return isVariable; }
            set { isVariable = value && !IsPredicate; isConstant = !value && !IsPredicate; }
        }

        public Expression(string name, List<Expression>? arguments, bool isConstant = false)
        {
            this.Name = name;
            this.Arguments = arguments;
            this.IsConstant = isConstant;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Expression) return false;
            return this.Equals((Expression)obj);
        }

        public bool Equals(Expression? other)
        {
            if (other == null)
                return false;
            if (this.Name != other.Name)
                return false;
            if (this.Arguments == null && other.Arguments == null)
                return true;
            if (this.Arguments == null || other.Arguments == null)
                return false;
            if (this.Arguments.Count != other.Arguments.Count)
                return false;
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                if (!this.Arguments[i].Equals(other.Arguments[i]))
                    return false;
            }
            return true;
        }

        public bool Contains(Expression e)
        {
            if (this.Equals(e))
                return true;
            if (this.Arguments == null)
                return false;
            foreach (Expression arg in this.Arguments)
            {
                if (arg.Contains(e))
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            if (this.Arguments == null)
                return this.Name;
            string str = this.Name + "(";
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                str += this.Arguments[i].ToString();
                str += (i < this.Arguments.Count - 1) ? ", " : "";
            }
            str += ")";
            return str;
        }

        public object Clone()
        {
            List<Expression>? args = null;
            if (this.Arguments != null)
            {
                args = new List<Expression>();
                foreach (Expression arg in this.Arguments)
                {
                    args.Add((Expression)arg.Clone());
                }
            }
            return new Expression((string)this.Name.Clone(), args, this.IsConstant);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Arguments, Arguments == null ? 0 : Arguments.Count, IsConstant);
        }
    }
}