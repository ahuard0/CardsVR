using System;
using System.Collections.Generic;

public class TupleList<T1, T2> : List<Tuple<T1, T2>> where T1 : IComparable
{
    public void Add(T1 item, T2 item2)
    {
        Add(new Tuple<T1, T2>(item, item2));
    }

    public new void Sort()
    {
        Comparison<Tuple<T1, T2>> c = (a, b) => a.Item1.CompareTo(b.Item1);
        base.Sort(c);
    }

    public T1[] Keys 
    { 
        get
        {
            Tuple<T1, T2>[] arr = base.ToArray();
            T1[] keys = new T1[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                keys[i] = arr[i].Item1;
            return keys;
        }
    }

    public T2[] Values
    {
        get
        {
            Tuple<T1, T2>[] arr = base.ToArray();
            T2[] values = new T2[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                values[i] = arr[i].Item2;
            return values;
        }
    }

}
