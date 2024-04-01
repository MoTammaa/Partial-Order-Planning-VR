
namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class PlanningProblem
    {
        private readonly HashSet<Operator> operators;
        private List<Literal> initialState;
        private List<Literal> goalState;
        private readonly HashSet<Literal> literals = [];

        public HashSet<Operator> Operators
        {
            get { return operators; }
        }
        public List<Literal> InitialState
        {
            get { return initialState; }
        }
        public List<Literal> GoalState
        {
            get { return goalState; }
        }

        public HashSet<Literal> Literals
        {
            get { return literals; }
        }

#nullable disable warnings
        public PlanningProblem(HashSet<Operator> operators, List<Literal> initialState, List<Literal> goalState)
        {
            ThrowIfNull(operators, nameof(operators));
            ThrowIfNull(initialState, nameof(initialState));
            ThrowIfNull(goalState, nameof(goalState));

            this.operators = operators;
            this.initialState = initialState;
            this.goalState = goalState;

            foreach (Operator op in operators)
            {
                foreach (Literal l in op.Effects)
                {
                    if (!literals.Contains(l))
                    {
                        literals.Add(new Literal(l));
                    }
                }
                foreach (Literal l in op.Preconditions)
                {
                    if (!literals.Contains(l))
                    {
                        literals.Add(new Literal(l));
                    }
                }
            }

            foreach (Literal l in initialState)
            {
                if (!literals.Contains(l))
                {
                    literals.Add(new Literal(l));
                }
            }
            foreach (Literal l in goalState)
            {
                if (!literals.Contains(l))
                {
                    literals.Add(new Literal(l));
                }
            }
        }

#nullable restore warnings

        public List<Operator> GetListOfAchievers(Literal l)
        {
            List<Operator> achievers = [];
            // check if the literal is in the effects of the operator
            foreach (Operator op in operators)
            {
                foreach (Literal effect in op.Effects)
                {
                    if (effect.Name == l.Name && effect.IsPositive == l.IsPositive
                        && effect.Variables.Length == l.Variables.Length)
                    {
                        achievers.Add(op);
                        break;
                    }
                }
            }
            return achievers;
        }


        public static void WearShirtProblem()
        {
            PlanningProblem custom = new PlanningProblem(
                [
                    new Operator("Wear", [new Literal("Worn", ["x"])], [new Literal("At", ["Home"])], ["x"]),
                ],
                [new Literal("At", ["Home"])],
                [new Literal("Worn", ["SHIRT"])]
            );

            Planner planner = new Planner(custom);
            PartialPlan? plan = planner.POP();
            Console.WriteLine($"Plan {(plan is null ? "not" : "")} found: \n" + plan);
        }

        public static void SocksShoesProblem()
        {
            PlanningProblem socksShoes = new PlanningProblem(
                operators: [
                    new Operator("RightSock", [new ("RightSockOn", [])], [], []),
                    new Operator("LeftSock", [new ("LeftSockOn", [])], [], []),
                    new Operator("RightShoe", [new ("RightShoeOn", [])], [new ("RightSockOn", [])], []),
                    new Operator("LeftShoe", [new ("LeftShoeOn", [])], [new ("LeftSockOn", [])], []),
                ],
                initialState: [],
                goalState: [new("RightShoeOn", []), new("LeftShoeOn", []), new("RightSockOn", []), new("LeftSockOn", [])]
            );

            Planner planner = new Planner(socksShoes);
            PartialPlan? plan = planner.POP();
            Console.WriteLine($"Plan {(plan is null ? "not" : "")} found: \n" + plan);
        }

        public static void MilkBananasCordlessDrillProblem()
        {
            PlanningProblem milkBananasCordlessDrill = new PlanningProblem(
                operators: [
                    new ("Buy", [ new ("Have", [ "x"] ) ], [ new ("Sells", [ "store", "x"] ) , new("At", ["store"])], [ "x" ]),
                    new ("Go", [ new ("At", ["there"]), new("At", ["here"], false) ], [ new ("At", ["here"]) ], ["there"]),

                ],
                initialState: [new("At", ["Home"]), new("Sells", ["SM", "Milk"]), new("Sells", ["SM", "Bananas"]), new("Sells", ["HWS", "Drill"])],
                goalState: [new("At", ["Home"]), new("Have", ["Milk"]), new("Have", ["Bananas"]), new("Have", ["Drill"])]
            );

            Planner planner = new Planner(milkBananasCordlessDrill);
            PartialPlan? plan = planner.POP();
            Console.WriteLine($"Plan {(plan is null ? "not" : "")} found: \n" + plan);
        }

        public static void SpareTiresProblem()
        {
            PlanningProblem spareTires = new PlanningProblem(
                operators: [
                    new Operator("Remove",
                                variables:      ["obj", "loc"],
                                preconditions:  [new ("At", ["obj", "loc"]), new("Tire", ["obj"])],
                                effects:        [new ("At", ["obj", "Ground"]), new("At", ["obj", "loc"], false)]
                            ),
                    new Operator("PutOn",
                                variables:      ["t", "Axle"],
                                preconditions:  [new ("At", ["t", "Ground"]), new("At", ["Flat", "Axle"], false), new("Tire", ["t"])],
                                effects:        [new ("At", ["t", "Axle"]), new("At", ["t", "Ground"], false)]
                            ),
                    new Operator("LeaveOvernight",
                                variables:      [],
                                preconditions:  [],
                                effects:        [new ("At", ["Spare", "Axle"], false), new("At", ["Spare", "Trunk"], false), new("At", ["Spare", "Ground"], false)
                                                ,new("At", ["Flat", "Axle"], false), new("At", ["Flat", "Trunk"], false), new("At", ["Flat", "Ground"], false)]
                            )
                ],
                initialState: [new("At", ["Flat", "Axle"]), new("At", ["Spare", "Trunk"]), new("Tire", ["Spare"]), new("Tire", ["Flat"])],
                goalState: [new("At", ["Spare", "Axle"]), new("At", ["Flat", "Ground"])]
            );

            Planner planner = new Planner(spareTires);
            PartialPlan? plan = planner.POP();
            Console.WriteLine($"Plan {(plan is null ? "not" : "")} found: \n" + plan);
        }

        public static void GroceriesBuyProblem()
        {
            PlanningProblem groceriesBuy = new PlanningProblem(
                operators: [
                    new Operator("Buy",
                                variables:      ["x"],
                                preconditions:  [new ("At", ["SM"]) , new("At", ["Home"], false)],
                                effects:        [new ("Have", ["x"])]
                            ),
                    new Operator("Go",
                                variables:      ["x"],
                                preconditions:  [new ("At", ["any"]), new("At", ["x"], false)],
                                effects:        [new ("At", ["any"], false), new("At", ["x"])]
                            )
                ],
                initialState: [new("At", ["Home"])],
                goalState: [new("At", ["Home"]), new("Have", ["Groceries"])]
            );

            Planner planner = new Planner(groceriesBuy);
            PartialPlan? plan = planner.POP();
            Console.WriteLine($"Plan {(plan is null ? "not" : "")} found: \n" + plan);
        }


        public override string ToString()
        {
            string str = "Operators:\n";
            foreach (Operator op in operators)
            {
                str += op.ToString() + "\n";
            }
            str += "Initial State:\n";
            foreach (Literal l in initialState)
            {
                str += l.ToString() + "\n";
            }
            str += "Goal State:\n";
            foreach (Literal l in goalState)
            {
                str += l.ToString() + "\n";
            }
            return str;
        }
    }
}