using System.Text;

namespace POP
{
    public class BindingConstraints : ICloneable, IEquatable<BindingConstraints>
    {
        private Dictionary<int, int> pE, rankE, setSizeE;
        private Dictionary<int, HashSet<int>> pNEGraph;
        private Dictionary<string, int> variablesMap = new Dictionary<string, int>();
        int numSetsE;
        private int count = 0;

        public List<string> Variables
        {
            get { return new List<string>(variablesMap.Keys); }
        }

        public BindingConstraints(int initalCapacity = -1)
        {
            if (initalCapacity == -1)
            {
                pE = new Dictionary<int, int>();
                rankE = new Dictionary<int, int>();
                setSizeE = new Dictionary<int, int>();
                pNEGraph = new Dictionary<int, HashSet<int>>();
            }
            else
            {
                pE = new Dictionary<int, int>(initalCapacity);
                rankE = new Dictionary<int, int>(initalCapacity);
                setSizeE = new Dictionary<int, int>(initalCapacity);
                pNEGraph = new Dictionary<int, HashSet<int>>(initalCapacity);
            }
        }

        private string? getStringName(int i)
        {
            foreach (var item in variablesMap)
            {
                if (item.Value == i)
                    return item.Key;
            }
            return null;
        }

        public HashSet<string> getEqualSet(string variable)
        {
            if (!variablesMap.ContainsKey(variable))
                return [];
            HashSet<string> result = [];
            int representative = findRepresentativeEq(variablesMap[variable]);
            foreach (var item in variablesMap)
            {
                if (findRepresentativeEq(item.Value) == representative)
                    result.Add(item.Key);
            }
            return result;
        }
        public string? getBoundEq(string variable)
        {
            if (!variablesMap.ContainsKey(variable))
            {
                if (Helpers.IsUpper(variable[0]))
                    return variable;
                return null;
            }
            return getStringName(findRepresentativeEq(variablesMap[variable]));
        }
        public string[] getBoundNE(string variable)
        {
            if (!variablesMap.ContainsKey(variable))
                return [];
            return pNEGraph[variablesMap[variable]].Select(getStringName).ToArray()!;
        }

        private int findRepresentativeEq(int i)
        {
            if (!pE.ContainsKey(i))
                return -1;
            return pE[i] == i ? i : (pE[i] = findRepresentativeEq(pE[i]));
        }
        public bool isEqual(string a, string b)
        {
            if (!variablesMap.ContainsKey(a) || !variablesMap.ContainsKey(b))
                return false;
            return isSameSetEq(variablesMap[a], variablesMap[b]);
        }
        public bool isNotEqual(string a, string b, int visited = 0)
        {
            if (!variablesMap.ContainsKey(a) || !variablesMap.ContainsKey(b))
                return false;
            if (Helpers.IsUpper(a[0]) && Helpers.IsUpper(b[0]))
                return a != b;

            if (Helpers.IsUpper(a[0]) && pNEGraph.TryGetValue(variablesMap[b], out HashSet<int>? bValue))
                return bValue.Contains(variablesMap[a]);
            if (Helpers.IsUpper(b[0]) && pNEGraph.TryGetValue(variablesMap[a], out HashSet<int>? aValue))
                return aValue.Contains(variablesMap[b]);

            if (pNEGraph.TryGetValue(variablesMap[a], out HashSet<int>? aValues) && aValues.Contains(variablesMap[b]))
                return true;
            if (pNEGraph.TryGetValue(variablesMap[b], out HashSet<int>? bValues) && bValues.Contains(variablesMap[a]))
                return true;

            HashSet<string> equivalentSet = getEqualSet(a);
            if (equivalentSet.Count > 1)
            {
                foreach (string item in equivalentSet)
                {
                    if (item == a || (visited & 1 << variablesMap[item]) != 0)
                        continue;
                    if (isNotEqual(item, b, visited | 1 << variablesMap[item]))
                        return true;
                }
            }

            equivalentSet = getEqualSet(b);
            if (equivalentSet.Count > 1)
            {
                foreach (string item in equivalentSet)
                {
                    if (item == b || (visited & 1 << variablesMap[item]) != 0)
                        continue;
                    if (isNotEqual(item, a, visited | 1 << variablesMap[item]))
                        return true;
                }
            }
            return false;
        }


        private bool setParentEq(string variable, string constant)
        {
            if (!variablesMap.ContainsKey(variable))
            {
                variablesMap.Add(variable, count);
                pE.Add(count, count);
                rankE.Add(count, 0);
                setSizeE.Add(count, 1);
                count++;
                numSetsE++;
            }
            if (!variablesMap.ContainsKey(constant))
            {
                variablesMap.Add(constant, count);
                pE.Add(count, count);
                rankE.Add(count, 0);
                setSizeE.Add(count, 1);
                count++;
                numSetsE++;
            }

            if (isNotEqual(variable, constant))
                return false;

            string? varRepresentative = getStringName(findRepresentativeEq(variablesMap[variable]));
            string? constRepresentative = getStringName(findRepresentativeEq(variablesMap[constant]));

            if (varRepresentative == null || constRepresentative == null)
                return false;

            if (Helpers.IsUpper(varRepresentative[0]) && varRepresentative != constRepresentative) // if the variable is bind to a different constant
                return false;

            unionSetEq(variablesMap[variable], variablesMap[constant]);

            // swap and make the constant the representative
            if (!Helpers.IsUpper(varRepresentative[0]))
            {
                string? newVarRepresentative = getStringName(findRepresentativeEq(variablesMap[variable]));
                if (newVarRepresentative == null)
                    return false;

                int temp = variablesMap[newVarRepresentative];
                variablesMap[newVarRepresentative] = variablesMap[constRepresentative];
                variablesMap[constRepresentative] = temp;
            }

            return true;
        }

        public bool setEqual(string a, string b)
        {
            if (!variablesMap.ContainsKey(a))
            {
                variablesMap.Add(a, count);
                pE.Add(count, count);
                rankE.Add(count, 0);
                setSizeE.Add(count, 1);
                count++;
                numSetsE++;
            }
            else if (!pE.ContainsKey(variablesMap[a]) && !Helpers.IsUpper(a[0]))
            {
                pE.Add(variablesMap[a], variablesMap[a]);
                rankE.Add(variablesMap[a], 0);
                setSizeE.Add(variablesMap[a], 1);
                numSetsE++;
            }

            if (!variablesMap.ContainsKey(b))
            {
                variablesMap.Add(b, count);
                pE.Add(count, count);
                rankE.Add(count, 0);
                setSizeE.Add(count, 1);
                count++;
                numSetsE++;
            }
            else if (!pE.ContainsKey(variablesMap[b]) && !Helpers.IsUpper(b[0]))
            {
                pE.Add(variablesMap[b], variablesMap[b]);
                rankE.Add(variablesMap[b], 0);
                setSizeE.Add(variablesMap[b], 1);
                numSetsE++;
            }

            if (isNotEqual(a, b))
                return false;

            if (Helpers.IsUpper(a[0]) && Helpers.IsUpper(b[0]))
                return a == b;

            if (Helpers.IsUpper(a[0]))
                return setParentEq(b, a);

            if (Helpers.IsUpper(b[0]))
                return setParentEq(a, b);

            unionSetEq(variablesMap[a], variablesMap[b]);
            return true;
        }

        public bool setNotEqual(string a, string b)
        {
            if (!variablesMap.ContainsKey(a))
            {
                variablesMap.Add(a, count);
                pNEGraph.Add(count, new HashSet<int>());
                count++;
            }
            else if (!pNEGraph.ContainsKey(variablesMap[a]) && !Helpers.IsUpper(a[0]))
            {
                pNEGraph.Add(variablesMap[a], new HashSet<int>());
            }

            if (!variablesMap.ContainsKey(b))
            {
                variablesMap.Add(b, count);
                pNEGraph.Add(count, new HashSet<int>());
                count++;
            }
            else if (!pNEGraph.ContainsKey(variablesMap[b]) && !Helpers.IsUpper(b[0]))
            {
                pNEGraph.Add(variablesMap[b], new HashSet<int>());
            }

            if (isSameSetEq(variablesMap[a], variablesMap[b]))
                return false;

            if (Helpers.IsUpper(a[0]) && Helpers.IsUpper(b[0]))
                return a != b;

            if (!Helpers.IsUpper(a[0]))
                pNEGraph[variablesMap[a]].Add(variablesMap[b]);

            if (!Helpers.IsUpper(b[0]))
                pNEGraph[variablesMap[b]].Add(variablesMap[a]);

            if (!Helpers.IsUpper(a[0]) && !Helpers.IsUpper(b[0]))
            {
                pNEGraph[variablesMap[a]].UnionWith(pNEGraph[variablesMap[b]]);
                pNEGraph[variablesMap[b]].UnionWith(pNEGraph[variablesMap[a]]);
            }

            return true;
        }


        private bool isSameSetEq(int i, int j)
        {
            int iRep = findRepresentativeEq(i);
            int jRep = findRepresentativeEq(j);
            if (iRep == -1 || jRep == -1)
                return false;
            return iRep == jRep;
        }
        private bool unionSetEq(int i, int j)
        {
            if (isSameSetEq(i, j))
                return false;
            numSetsE--;
            int x = findRepresentativeEq(i), y = findRepresentativeEq(j);
            if (rankE[x] > rankE[y])
            {
                pE[y] = x;
                setSizeE[x] += setSizeE[y];
            }
            else
            {
                pE[x] = y;
                setSizeE[y] += setSizeE[x];
                if (rankE[x] == rankE[y])
                    rankE[y]++;
            }
            return true;
        }

        public int numEqDisjointSets()
        {
            return numSetsE;
        }
        public int sizeOfEqSet(int i)
        {
            return setSizeE[findRepresentativeEq(i)];
        }

        public object Clone()
        {
            BindingConstraints newBindingConstraints = new BindingConstraints();
            newBindingConstraints.pE = new Dictionary<int, int>(pE);
            newBindingConstraints.rankE = new Dictionary<int, int>(rankE);
            newBindingConstraints.setSizeE = new Dictionary<int, int>(setSizeE);
            newBindingConstraints.numSetsE = numSetsE;
            newBindingConstraints.count = count;
            foreach (var item in variablesMap)
            {
                newBindingConstraints.variablesMap.Add((string)item.Key.Clone(), item.Value);
            }
            foreach (var item in pNEGraph)
            {
                newBindingConstraints.pNEGraph.Add(item.Key, new HashSet<int>(item.Value));
            }
            return newBindingConstraints;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n{\nEqual:\n");
            bool[] showed = new bool[variablesMap.Count];
            foreach (var item in variablesMap)
            {
                if (showed[item.Value])
                    continue;

                HashSet<string> equivalentSet = getEqualSet(item.Key);
                if (equivalentSet.Count > 1)
                {
                    sb.Append(string.Join(" = ", equivalentSet) + ",\n");
                    foreach (string equivalent in equivalentSet)
                    {
                        showed[variablesMap[equivalent]] = true;
                    }
                }
            }

            sb.Append("\nNot Equal:\n");
            bool[,] showedNE = new bool[variablesMap.Count, variablesMap.Count];
            foreach (var item in pNEGraph)
            {
                foreach (int value in item.Value)
                {
                    if (showedNE[item.Key, value] || showedNE[value, item.Key])
                        continue;
                    sb.Append(getStringName(item.Key) + " != " + getStringName(value) + "\n");
                    showedNE[item.Key, value] = true;
                    showedNE[value, item.Key] = true;
                }
            }
            return sb.Append("}\n").ToString();
        }

        public bool Equals(BindingConstraints? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.pE.SequenceEqual(other.pE)
                && this.rankE.SequenceEqual(other.rankE)
                && this.setSizeE.SequenceEqual(other.setSizeE)
                && this.pNEGraph.SequenceEqual(other.pNEGraph)
                && this.variablesMap.SequenceEqual(other.variablesMap);
        }

        public override bool Equals(object? obj)
        {
            return obj is BindingConstraints constraints && this.Equals(constraints);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(pE, rankE, setSizeE, pNEGraph, variablesMap);
        }


        public static bool operator ==(BindingConstraints? left, BindingConstraints? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(BindingConstraints? left, BindingConstraints? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(BindingConstraints? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(BindingConstraints? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, BindingConstraints? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, BindingConstraints? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }

    }
}

