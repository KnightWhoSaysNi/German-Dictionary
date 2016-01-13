using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GermanDictionary.Helpers
{
    /// <summary>
    /// Observable collection that sorts items that are added (only) with Add method.
    /// INotifyCollectionChanged and INotifyPropertyChanged inherited from ObservableCollection. 
    /// Duplicates allowed in collection.
    /// </summary>
    /// <typeparam name="T">A class that implements IComparable interface.</typeparam>
    [Serializable]
    public class SortedObservableCollection<T> : ObservableCollection<T> where T : IComparable<T>
    {
        /// <summary>
        /// Adds item in the collection and sorts it in ascending order.
        /// </summary>
        /// <param name="item">IComparable item to be added in the collection.</param>
        public new void Add(T item)
        {
            int lower = 0;
            int upper = base.Count - 1;
            int mid = 0;
            while (lower <= upper)
            {
                mid = lower + (upper - lower) / 2;
                switch (item.CompareTo(this[mid]))
                {
                    case 0:
                        base.InsertItem(mid, item);
                        return;
                    case 1:
                        lower = mid + 1;                        
                        break;
                    case -1:
                        upper = mid - 1;
                        break;
                }
            }
                        
            base.InsertItem(lower, item);
        }

        /// <summary>
        /// Gets the item at the specified index. Set is not supported in a sorted collection.
        /// </summary>
        /// <param name="index">Index of the item of a collection. </param>        
        public new T this[int index]
        {
            get { return base[index]; }
        }

        #region Some of the reasons why this class shouldn't be derived from Collection.

        /// <summary>
        /// Not supported in a sorted collection. Throws InvalidOperationException if used.
        /// </summary>     
        /// <exception cref="InvalidOperationException" />
        public new void Insert(int index, T item)
        {
            throw new InvalidOperationException("Cannot manually choose an index for a sorted collection.");            
        }
        
        /// <summary>
        /// Not supported in a sorted collection. Throws InvalidOperationException if used.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public new void Move(int oldIndex, int newIndex)
        {
            throw new InvalidOperationException("Cannot manually change the index of items in a sorted collection");
        }

        /// <summary>
        /// Not supported in sorted collections. Throws InvalidOperationException if used.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <remarks>
        /// Each item from the second collection could be added individually to the collection through 
        /// Add method, but that wouldn't be concatenation.
        /// </remarks>
        public IEnumerable<T> Concat<K>(IEnumerable<T> secondCollection)
        {
            throw new InvalidOperationException("Cannot concatenate another collection to a sorted collection.");
        }

        #endregion
    }
}
