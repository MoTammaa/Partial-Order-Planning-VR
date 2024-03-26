using POP;
using Action = POP.Action;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World! hahaha");
Console.WriteLine("Hello, World! hahahax2");

// test the unification algorithm
bool variable = false;
Expression e1 = new("P",
    [
        new("x", null, variable),
        new("g", [new("x", null, variable)]),
        new("g", [new("f", [new("A", null, !variable)])]),
    ]
);
Expression e2 = new("P",
    [
        new("f", [new("u", null, variable)]),
        new("v", null, variable),
        new("v", null, variable),
    ]
);

Dictionary<Expression, List<Expression>>? μ = Helpers.Unify(e1, e2);
if (μ != null)
{
    Console.Write("Unification successful\n{" + (μ.Count > 0 ? "\n" : ""));
    foreach (Expression key in μ.Keys)
    {
        foreach (Expression value in μ[key])
        {
            Console.WriteLine("\t" + value + " / " + key);
        }
    }
    Console.WriteLine("}");
}
else
{
    Console.WriteLine("Unification failed");
}




PlanningProblem custom = new PlanningProblem(
    [new("Wear", [new Literal("Worn", ["x"])], [new Literal("At", ["Home"])], ["x"]),],
    [new Literal("At", ["Home"])],
    [new("Worn", ["SHIRT"])]
);

PlanningProblem socksShoes = new PlanningProblem(
    [
        new("RightSock", [new ("RightSockOn", [])], [], []),
        new("LeftSock", [new ("LeftSockOn", [])], [], []),
        new("RightShoe", [new ("RightShoeOn", [])], [ new ("RightSockOn",[])], []),
        new("LeftShoe", [new ("LeftShoeOn", [])], [ new ("LeftSockOn",[])], []),
    ],
    [],
    [new("RightShoeOn", []), new("LeftShoeOn", []), new("RightSockOn", []), new("LeftSockOn", [])]
);

Planner planner = new Planner(custom);
PartialPlan? plan = planner.POP();

Planner planner2 = new Planner(socksShoes);
PartialPlan? plan2 = planner2.POP();


Console.WriteLine($"Plan {(plan2 is null ? "not" : "")} found: \n" + plan2);