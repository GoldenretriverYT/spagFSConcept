using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept {
    internal static class IEnumerableExtensions {
        public static T[] PadRight<T>(this T[] ie, int size) {
            if (ie.Count() >= size) return ie;

            T[] arr = new T[size];
            
            for (int i = 0; i < size; i++) {
                if (ie.Length > i)
                    arr[i] = ie[i];
                else
                    arr[i] = default(T);
            }

            return arr;
        }
    }
}
