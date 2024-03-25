
namespace POP
{
    using System;

    using static System.ArgumentNullException;



    public class BindingConstraint : ICloneable
    {
        private string variable;
        private List<string> bounds;

        private bool isEqBelong;

        public string Variable
        {
            get { return variable; }
            set { variable = value; }
        }
        public List<string> Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }
        public bool IsEqBelong
        {
            get { return isEqBelong; }
            set { isEqBelong = value; }
        }


#nullable disable warnings
        public BindingConstraint(string variable, List<string> bounds, bool isEqBelong)
        {
            ThrowIfNull(variable, nameof(variable));
            ThrowIfNull(bounds, nameof(bounds));

            this.Variable = variable;
            this.Bounds = bounds;
            this.IsEqBelong = isEqBelong;
        }

#nullable restore warnings
        public object Clone()
        {
            return new BindingConstraint((string)this.Variable.Clone(), new List<string>(this.Bounds), this.IsEqBelong);
        }

        public override string ToString()
        {
            return $"{Variable} = {string.Join(", ", Bounds)}";
        }


    }
}