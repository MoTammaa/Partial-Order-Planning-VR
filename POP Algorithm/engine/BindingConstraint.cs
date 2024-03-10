using System;


namespace POP
{
    public enum Operation
    {
        EQUALS,
        NOT_EQUALS,
        LESS_THAN,
        GREATER_THAN,
        LESS_THAN_OR_EQUAL,
        GREATER_THAN_OR_EQUAL
    }



    public class BindingConstraint
    {
        private string var1, var2;
        private bool isVar1Constant = false, isVar2Constant = false;

        public string Var1
        {
            get { return var1; }
            set { var1 = value; }
        }
        public string Var2
        {
            get { return var2; }
            set { var2 = value; }
        }
        public bool IsVar1Constant
        {
            get { return isVar1Constant; }
            set { isVar1Constant = value; }
        }
        public bool IsVar2Constant
        {
            get { return isVar2Constant; }
            set { isVar2Constant = value; }
        }


        public BindingConstraint()
        {

        }
        public static string OpToString(Operation op)
        {
            switch (op)
            {
                case Operation.EQUALS:
                    return "=";
                case Operation.NOT_EQUALS:
                    return "!=";
                case Operation.LESS_THAN:
                    return "<";
                case Operation.GREATER_THAN:
                    return ">";
                case Operation.LESS_THAN_OR_EQUAL:
                    return "<=";
                case Operation.GREATER_THAN_OR_EQUAL:
                    return ">=";
                default:
                    throw new Exception("Invalid operation");
            }
        }

        public static Operation StringToOp(string op)
        {
            switch (op)
            {
                case "=":
                    return Operation.EQUALS;
                case "!=":
                    return Operation.NOT_EQUALS;
                case "<":
                    return Operation.LESS_THAN;
                case ">":
                    return Operation.GREATER_THAN;
                case "<=":
                    return Operation.LESS_THAN_OR_EQUAL;
                case ">=":
                    return Operation.GREATER_THAN_OR_EQUAL;
                default:
                    throw new Exception("Invalid operation");
            }
        }

    }
}