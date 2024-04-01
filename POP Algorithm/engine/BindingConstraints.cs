namespace POP
{
    class BindingConstraints
    {
        private Dictionary<int, int> pE, rankE, setSizeE, pNE, rankNE, setSizeNE;
        private Dictionary<string, int> map = new Dictionary<string, int>();
        int numSetsE, numSetsNE;
        private int count = 0;

        public BindingConstraints(int initalCapacity = -1)
        {
            if (initalCapacity == -1)
            {
                pE = new Dictionary<int, int>();
                rankE = new Dictionary<int, int>();
                setSizeE = new Dictionary<int, int>();
                pNE = new Dictionary<int, int>();
                rankNE = new Dictionary<int, int>();
                setSizeNE = new Dictionary<int, int>();
            }
            else
            {
                pE = new Dictionary<int, int>(initalCapacity);
                rankE = new Dictionary<int, int>(initalCapacity);
                setSizeE = new Dictionary<int, int>(initalCapacity);
                pNE = new Dictionary<int, int>(initalCapacity);
                rankNE = new Dictionary<int, int>(initalCapacity);
                setSizeNE = new Dictionary<int, int>(initalCapacity);
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

        public string? getBoundEq(string variable)
        {
            if (!map.ContainsKey(variable))
                return null;
            return getStringName(findRepresentativeEq(map[variable]));
        }
        public string? getBoundNE(string variable)
        {
            if (!map.ContainsKey(variable))
                return null;
            return getStringName(findRepresentativeNE(map[variable]));
        }

        private int findRepresentativeEq(int i)
        {
            if (!pE.ContainsKey(i))
                return -1;
            return pE[i] == i ? i : (pE[i] = findRepresentativeEq(pE[i]));
        }
        private int findRepresentativeNE(int i)
        {
            if (!pNE.ContainsKey(i))
                return -1;
            return pNE[i] == i ? i : (pNE[i] = findRepresentativeNE(pNE[i]));
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

            if (isSameSetNE(map[variable], map[constant]))
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
        private bool setParentNE(string variable, string constant)
        {
            if (!map.ContainsKey(variable))
            {
                map.Add(variable, count);
                pNE.Add(count, count);
                rankNE.Add(count, 0);
                setSizeNE.Add(count, 1);
                count++;
                numSetsNE++;
            }
            if (!map.ContainsKey(constant))
            {
                map.Add(constant, count);
                pNE.Add(count, count);
                rankNE.Add(count, 0);
                setSizeNE.Add(count, 1);
                count++;
                numSetsNE++;
            }

            if (isSameSetEq(map[variable], map[constant]))
                return false;

            string? varRepresentative = getStringName(findRepresentativeNE(map[variable]));
            string? constRepresentative = getStringName(findRepresentativeNE(map[constant]));

            if (varRepresentative == null || constRepresentative == null)
                return false;

            if (Helpers.IsUpper(varRepresentative[0]) && varRepresentative != constRepresentative) // if the variable is bind to a different constant
                return false;

            unionSetNE(map[variable], map[constant]);

            // swap and make the constant the representative
            if (!Helpers.IsUpper(varRepresentative[0]))
            {
                string? newVarRepresentative = getStringName(findRepresentativeNE(map[variable]));
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

            if (isSameSetNE(map[a], map[b]))
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
                pNE.Add(count, count);
                rankNE.Add(count, 0);
                setSizeNE.Add(count, 1);
                count++;
                numSetsNE++;
            }
            else if (!pNE.ContainsKey(map[a]) && !Helpers.IsUpper(a[0]))
            {
                pNE.Add(map[a], map[a]);
                rankNE.Add(map[a], 0);
                setSizeNE.Add(map[a], 1);
                numSetsNE++;
            }

            if (!map.ContainsKey(b))
            {
                map.Add(b, count);
                pNE.Add(count, count);
                rankNE.Add(count, 0);
                setSizeNE.Add(count, 1);
                count++;
                numSetsNE++;
            }
            else if (!pNE.ContainsKey(map[b]) && !Helpers.IsUpper(b[0]))
            {
                pNE.Add(map[b], map[b]);
                rankNE.Add(map[b], 0);
                setSizeNE.Add(map[b], 1);
                numSetsNE++;
            }

            if (isSameSetEq(map[a], map[b]))
                return false;

            if (Helpers.IsUpper(a[0]) && Helpers.IsUpper(b[0]))
                return a != b;

            if (Helpers.IsUpper(a[0]))
                return setParentNE(b, a);

            if (Helpers.IsUpper(b[0]))
                return setParentNE(a, b);

            unionSetNE(map[a], map[b]);
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
        private bool isSameSetNE(int i, int j)
        {
            int iRep = findRepresentativeNE(i);
            int jRep = findRepresentativeNE(j);
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
        private bool unionSetNE(int i, int j)
        {
            if (isSameSetNE(i, j))
                return false;
            numSetsNE--;
            int x = findRepresentativeNE(i), y = findRepresentativeNE(j);
            if (rankNE[x] > rankNE[y])
            {
                pNE[y] = x;
                setSizeNE[x] += setSizeNE[y];
            }
            else
            {
                pNE[x] = y;
                setSizeNE[y] += setSizeNE[x];
                if (rankNE[x] == rankNE[y])
                    rankNE[y]++;
            }
            return true;
        }

        public int numEqDisjointSets()
        {
            return numSetsE;
        }
        public int numNEDisjointSets()
        {
            return numSetsNE;
        }

        public int sizeOfEqSet(int i)
        {
            return setSizeE[findRepresentativeEq(i)];
        }
        public int sizeOfNESet(int i)
        {
            return setSizeNE[findRepresentativeNE(i)];
        }
    }
}

