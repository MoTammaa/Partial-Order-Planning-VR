
using POP;

namespace POP
{
    using System;
    using System.Collections.Generic;
    using static System.ArgumentNullException;

    public class PlanningProblem
    {
#nullable enable
        private readonly HashSet<Operator> operators;
        private List<Literal> initialState;
        private List<Literal> goalState;
        private readonly HashSet<Literal> literals = new();

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
            Helpers.ThrowIfNull(operators, nameof(operators));
            Helpers.ThrowIfNull(initialState, nameof(initialState));
            Helpers.ThrowIfNull(goalState, nameof(goalState));

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
            List<Operator> achievers = new();
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


        public static PlanningProblem WearShirtProblem(bool runPlanner, SearchStrategy searchStrategy = SearchStrategy.AStar, int maxDepth = -1)
        {
            PlanningProblem custom = new PlanningProblem(
                new HashSet<Operator> {
                    new Operator("Wear", new List<Literal>{new Literal("Worn", new [] {"x"})}, new List<Literal>{new Literal("At",new []{"Home"})}, new [] {"x"}),
                },
                new List<Literal> { new Literal("At", new string[] { "Home" }) },
                new List<Literal> { new Literal("Worn", new[] { "SHIRT" }) }
            );

            if (!runPlanner) return custom;
            Planner planner = new Planner(custom, searchStrategy, maxDepth);
            PartialPlan? plan = planner.POP();
            Console.WriteLine($"\nPlan {(plan is null ? "not" : "")} found: \n" + plan);
            return custom;
        }

        public static PlanningProblem SocksShoesProblem(bool runPlanner, SearchStrategy searchStrategy = SearchStrategy.AStar, int maxDepth = -1)
        {
            PlanningProblem socksShoes = new PlanningProblem(
                operators: new HashSet<Operator> {
                    new Operator("RightSock", new List<Literal> {new ("RightSockOn", new string []{})}, new(), new string []{}),
                    new Operator("LeftSock", new List<Literal> {new ("LeftSockOn", new string []{})}, new(), new string []{}),
                    new Operator("RightShoe", new List<Literal> {new ("RightShoeOn", new string []{})}, new List<Literal> {new ("RightSockOn", new string []{})}, new string []{}),
                    new Operator("LeftShoe", new List<Literal> {new ("LeftShoeOn", new string []{})}, new List<Literal> {new ("LeftSockOn", new string []{})}, new string []{}),
                },
                initialState: new(),
                goalState: new List<Literal> { new("RightShoeOn", new string[] { }), new("LeftShoeOn", new string[] { }), new("RightSockOn", new string[] { }), new("LeftSockOn", new string[] { }) }
            );

            if (!runPlanner) return socksShoes;
            Planner planner = new Planner(socksShoes, searchStrategy, maxDepth);
            PartialPlan? plan = planner.POP();
            Console.WriteLine($"\nPlan {(plan is null ? "not" : "")} found: \n" + plan);

            return socksShoes;
        }

        public static PlanningProblem MilkBananasCordlessDrillProblem(bool runPlanner, SearchStrategy searchStrategy = SearchStrategy.AStar, int maxDepth = -1)
        {
            PlanningProblem milkBananasCordlessDrill = new PlanningProblem(
                operators: new HashSet<Operator> {
                    new Operator("Buy",
                        variables:      new []{ "x" },
                        preconditions:  new List<Literal> { new ("Sells", new [] { "store", "x" }), new ("At",new [] { "store" }) },
                        effects:        new List<Literal> { new ("Have", new []{ "x" }) }
                    ),
                    new Operator("Go",
                        variables:      new [] { "there" },
                        preconditions:  new List<Literal> { new ("At", new []{ "here" }),       new ("At", new []{ "there" }, false) },
                        effects:        new List<Literal> { new ("At", new []{ "here" }, false), new ("At", new []{ "there" }) }
                    )
                },
                initialState: new List<Literal>{ new("At",new []{"Home"}), new("Sells", new []{"SM", "Milk"}), new("Sells", new []{"SM", "Bananas"}), new("Sells", new []{"HWS", "Drill"})
                                , new("At",new []{"HWS"}, false), new("At", new []{"SM"}, false)},
                goalState: new List<Literal> { new("At", new string[] { "Home" }), new("Have", new[] { "Milk" }), new("Have", new[] { "Bananas" }), new("Have", new[] { "Drill" }) }
            );

            if (!runPlanner) return milkBananasCordlessDrill;
            Planner planner = new Planner(milkBananasCordlessDrill, searchStrategy, maxDepth);
            PartialPlan? plan = planner.POP();
            Console.WriteLine($"\nPlan {(plan is null ? "not" : "")} found: \n" + plan);

            return milkBananasCordlessDrill;
        }

        public static PlanningProblem SpareTiresProblem(bool runPlanner, SearchStrategy searchStrategy = SearchStrategy.AStar, int maxDepth = -1)
        {
            PlanningProblem spareTires = new PlanningProblem(
                operators: new HashSet<Operator> {
                    new Operator("Remove",
                                variables:      new []{"obj", "loc"},
                                preconditions:  new List<Literal>{ new ("At", new []{"obj", "loc"}), new("Tire", new []{"obj"})},
                                effects:        new List<Literal>{ new ("At", new []{"obj", "Ground"}), new("At", new []{"obj", "loc"}, false)}
                            ),
                    new Operator("PutOn",
                                variables:      new []{"t", "Axle"},
                                preconditions:  new List<Literal>{ new ("At", new []{"t", "Ground"}), new("At", new []{"Flat", "Axle"}, false), new("Tire", new []{"t"})},
                                effects:        new List<Literal>{ new ("At", new []{"t", "Axle"}), new("At", new []{"t", "Ground"}, false)}
                            ),
                    new Operator("LeaveOvernight",
                                variables:      new string []{},
                                preconditions:  new(),
                                effects:        new List<Literal>{ new ("At", new []{"Spare", "Axle"}, false), new("At", new []{"Spare", "Trunk"}, false), new("At", new []{"Spare", "Ground"}, false)
                                                ,new("At", new []{"Flat", "Axle"}, false), new("At", new []{"Flat", "Trunk"}, false), new("At", new []{"Flat", "Ground"}, false)}
                            )
                },
                initialState: new List<Literal> { new("At", new[] { "Flat", "Axle" }), new("At", new[] { "Spare", "Trunk" }), new("Tire", new[] { "Spare" }), new("Tire", new[] { "Flat" }) },
                goalState: new List<Literal> { new("At", new[] { "Spare", "Axle" }), new("At", new[] { "Flat", "Ground" }) }
            );

            if (!runPlanner) return spareTires;
            Planner planner = new Planner(spareTires, searchStrategy, maxDepth);
            PartialPlan? plan = planner.POP();
            Console.WriteLine($"\nPlan {(plan is null ? "not" : "")} found: \n" + plan);

            return spareTires;
        }

        public static PlanningProblem GroceriesBuyProblem(bool runPlanner, SearchStrategy searchStrategy = SearchStrategy.AStar, int maxDepth = -1)
        {
            PlanningProblem groceriesBuy = new PlanningProblem(
                operators: new HashSet<Operator> {
                    new Operator("Buy",
                                variables:      new [] {"x"},
                                preconditions:  new List<Literal>{ new ("At", new []{"SM"}) , new("At",new []{"Home"}, false)},
                                effects:        new List<Literal>{ new ("Have", new [] {"x"})}
                            ),
                    new Operator("Go",
                                variables:      new [] {"x"},
                                preconditions:  new List<Literal>{ new ("At", new []{"any"}), new("At", new [] {"x"}, false)},
                                effects:        new List<Literal>{ new ("At", new []{"any"}, false), new("At", new [] {"x"})}
                            )
                },
                initialState: new List<Literal> { new("At", new string[] { "Home" }), new("At", new[] { "SM" }, false) },
                goalState: new List<Literal> { new("At", new string[] { "Home" }), new("Have", new[] { "Groceries" }) }
            );

            if (!runPlanner) return groceriesBuy;
            Planner planner = new Planner(groceriesBuy, searchStrategy, maxDepth);
            PartialPlan? plan = planner.POP();
            Console.WriteLine($"\nPlan {(plan is null ? "not" : "")} found: \n" + plan);

            return groceriesBuy;
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