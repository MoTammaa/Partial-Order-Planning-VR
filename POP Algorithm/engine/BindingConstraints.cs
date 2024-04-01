namespace POP
{
    class BindingConstraints
    {
        private Dictionary<int, int> pE, rankE, setSizeE;
        private Dictionary<int, HashSet<int>> pNEGraph;
        private Dictionary<string, int> map = new Dictionary<string, int>();
        int numSetsE;
        private int count = 0;

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
            foreach (var item in map)
            {
                if (item.Value == i)
                    return item.Key;
            }
            return null;
        }

        public HashSet<string> getEqualSet(string variable)
        {
            if (!map.ContainsKey(variable))
                return [];
            HashSet<string> result = [];
            int representative = findRepresentativeEq(map[variable]);
            foreach (var item in map)
            {
                if (findRepresentativeEq(item.Value) == representative)
                    result.Add(item.Key);
            }
            return result;
        }
        public string? getBoundEq(string variable)
        {
            if (!map.ContainsKey(variable))
                return null;
            return getStringName(findRepresentativeEq(map[variable]));
        }
        public string[] getBoundNE(string variable)
        {
            if (!map.ContainsKey(variable))
                return [];
            return pNEGraph[map[variable]].Select(getStringName).ToArray()!;
        }

        private int findRepresentativeEq(int i)
        {
            if (!pE.ContainsKey(i))
                return -1;
            return pE[i] == i ? i : (pE[i] = findRepresentativeEq(pE[i]));
        }
        public bool isEqual(string a, string b)
        {
            if (!map.ContainsKey(a) || !map.ContainsKey(b))
                return false;
            return isSameSetEq(map[a], map[b]);
        }
        public bool isNotEqual(string a, string b, int visited = 0)
        {
            if (!map.ContainsKey(a) || !map.ContainsKey(b))
                return false;
            if (Helpers.IsUpper(a[0]) && Helpers.IsUpper(b[0]))
                return a != b;

            if (Helpers.IsUpper(a[0]) && pNEGraph.TryGetValue(map[b], out HashSet<int>? bValue))
                return bValue.Contains(map[a]);
            if (Helpers.IsUpper(b[0]) && pNEGraph.TryGetValue(map[a], out HashSet<int>? aValue))
                return aValue.Contains(map[b]);

            if (pNEGraph.TryGetValue(map[a], out HashSet<int>? aValues) && aValues.Contains(map[b]))
                return true;
            if (pNEGraph.TryGetValue(map[b], out HashSet<int>? bValues) && bValues.Contains(map[a]))
                return true;

            HashSet<string> equivalentSet = getEqualSet(a);
            if (equivalentSet.Count > 1)
            {
                foreach (string item in equivalentSet)
                {
                    if (item == a || (visited & 1 << map[item]) != 0)
                        continue;
                    if (isNotEqual(item, b, visited | 1 << map[item]))
                        return true;
                }
            }

            equivalentSet = getEqualSet(b);
            if (equivalentSet.Count > 1)
            {
                foreach (string item in equivalentSet)
                {
                    if (item == b || (visited & 1 << map[item]) != 0)
                        continue;
                    if (isNotEqual(item, a, visited | 1 << map[item]))
                        return true;
                }
            }
            return false;
        }


        private bool setParentEq(string variable, string constant)
        {
            if (!map.ContainsKey(variable))
            {
                map.Add(variable, count);
                pE.Add(count, count);
                rankE.Add(count, 0);
                setSizeE.Add(count, 1);
                count++;
                numSetsE++;
            }
            if (!map.ContainsKey(constant))
            {
                map.Add(constant, count);
                pE.Add(count, count);
                rankE.Add(count, 0);
                setSizeE.Add(count, 1);
                count++;
                numSetsE++;
            }

            if (isNotEqual(variable, constant))
                return false;

            string? varRepresentative = getStringName(findRepresentativeEq(map[variable]));
            string? constRepresentative = getStringName(findRepresentativeEq(map[constant]));

            if (varRepresentative == null || constRepresentative == null)
                return false;

            if (Helpers.IsUpper(varRepresentative[0]) && varRepresentative != constRepresentative) // if the variable is bind to a different constant
                return false;

            unionSetEq(map[variable], map[constant]);

            // swap and make the constant the representative
            if (!Helpers.IsUpper(varRepresentative[0]))
            {
                string? newVarRepresentative = getStringName(findRepresentativeEq(map[variable]));
                if (newVarRepresentative == null)
                    return false;

                int temp = map[newVarRepresentative];
                map[newVarRepresentative] = map[constRepresentative];
                map[constRepresentative] = temp;
            }

            return true;
        }

        public bool setEqual(string a, string b)
        {
            if (!map.ContainsKey(a))
            {
                map.Add(a, count);
                pE.Add(count, count);
                rankE.Add(count, 0);
                setSizeE.Add(count, 1);
                count++;
                numSetsE++;
            }
            else if (!pE.ContainsKey(map[a]) && !Helpers.IsUpper(a[0]))
            {
                pE.Add(map[a], map[a]);
                rankE.Add(map[a], 0);
                setSizeE.Add(map[a], 1);
                numSetsE++;
            }

            if (!map.ContainsKey(b))
            {
                map.Add(b, count);
                pE.Add(count, count);
                rankE.Add(count, 0);
                setSizeE.Add(count, 1);
                count++;
                numSetsE++;
            }
            else if (!pE.ContainsKey(map[b]) && !Helpers.IsUpper(b[0]))
            {
                pE.Add(map[b], map[b]);
                rankE.Add(map[b], 0);
                setSizeE.Add(map[b], 1);
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

            unionSetEq(map[a], map[b]);
            return true;
        }

        public bool setNotEqual(string a, string b)
        {
            if (!map.ContainsKey(a))
            {
                map.Add(a, count);
                pNEGraph.Add(count, new HashSet<int>());
                count++;
            }
            else if (!pNEGraph.ContainsKey(map[a]) && !Helpers.IsUpper(a[0]))
            {
                pNEGraph.Add(map[a], new HashSet<int>());
            }

            if (!map.ContainsKey(b))
            {
                map.Add(b, count);
                pNEGraph.Add(count, new HashSet<int>());
                count++;
            }
            else if (!pNEGraph.ContainsKey(map[b]) && !Helpers.IsUpper(b[0]))
            {
                pNEGraph.Add(map[b], new HashSet<int>());
            }

            if (isSameSetEq(map[a], map[b]))
                return false;

            if (Helpers.IsUpper(a[0]) && Helpers.IsUpper(b[0]))
                return a != b;

            if (!Helpers.IsUpper(a[0]))
                pNEGraph[map[a]].Add(map[b]);

            if (!Helpers.IsUpper(b[0]))
                pNEGraph[map[b]].Add(map[a]);

            if (!Helpers.IsUpper(a[0]) && !Helpers.IsUpper(b[0]))
            {
                pNEGraph[map[a]].UnionWith(pNEGraph[map[b]]);
                pNEGraph[map[b]].UnionWith(pNEGraph[map[a]]);
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

    }
}

