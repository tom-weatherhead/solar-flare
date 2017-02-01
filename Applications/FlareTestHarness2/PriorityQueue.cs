// PriorityQueue.cs - By Tom Weatherhead - May 27, 2009

// The heap property for a list of prioritized elements is defined as:
// For each list element x that has a Parent(x), Priority(x) <= Priority(Parent(x));
// this guarantees that Priority(root) >= Priority(x) for all x.
// Also, if Index(x) = i and i > 0, then Index(Parent(x)) = floor((x - 1) / 2).
// Index(root) = 0; the root has no parent.

#define IMPLEMENT_ICOLLECTION

using System;
using System.Collections;           // Needed for the non-generic IEnumerator and IEnumerable
using System.Collections.Generic;
using System.Text;

namespace SolarFlareCommon
{
    namespace Collections
    {
        namespace Generic
        {
            public sealed class PriorityQueue<T> :
#if IMPLEMENT_ICOLLECTION
                ICollection<T>
#else
                IEnumerable<T>   // ThAW : We could implement ICollection<T> here instead.
#endif
                where T : IComparable<T>    // Type T is constrained; it must implement IComparable<>.
            {
                // Member variables.
                private readonly List<T> m_listQueuedElements = new List<T>();

                // Constructors.

                public PriorityQueue()
                {
                    // Default constructor: start with an empty list/heap/queue.
                }

                public PriorityQueue(IEnumerable<T> list)
                {
                    EnqueueAll(list);
                }

#if IMPLEMENT_ICOLLECTION
                // ICollection<> implementation.

                public int Count
                {
                    get
                    {
                        return m_listQueuedElements.Count;
                    }
                }

                public bool IsReadOnly
                {
                    get
                    {
                        return false;   // m_listQueuedElements.IsReadOnly;
                    }
                }

                public void Add(T item)
                {
                    Enqueue(item);
                }

                public void Clear()
                {
                    m_listQueuedElements.Clear();
                }

                public bool Contains(T item)
                {
                    return m_listQueuedElements.Contains(item);
                }

                public void CopyTo(T[] array, int arrayIndex)
                {
                    // Copy the queue's elements to the array in order of decreasing (or at least non-increasing) priority.
                    List<T> listSorted = DequeueAllToNewList();

                    EnqueueAll(listSorted);
                    listSorted.CopyTo(array, arrayIndex);
                }

                public bool Remove(T item)
                {

                    if (!Contains(item))
                    {
                        return false;
                    }

                    List<T> list = DequeueAllToNewList();
                    bool bSuccessfulRemoval = false;

                    try
                    {
                        bSuccessfulRemoval = list.Remove(item);
                    }
                    finally
                    {
                        // If an exception is thrown, don't let the contents of the queue be dumped permanently.
                        EnqueueAll(list);
                    }

                    return bSuccessfulRemoval;
                }
#else
                // IEnumerable<> implementation.
#endif
                // These two GetEnumerator overloads are common to both IEnumerable<> and ICollection<>.

                public IEnumerator<T> GetEnumerator()
                {
                    // ThAW 2009/05/27 : Caveat: This does *not* order the queue's elements by priority in any way.
                    return m_listQueuedElements.GetEnumerator();
                }
                
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                // Other methods (member functions).

                public bool IsEmpty()
                {
                    // ThAW 2009/05/27 : m_listQueuedElements will never be null; I'm just being paranoid.
                    return /* (m_listQueuedElements == null) || */ (m_listQueuedElements.Count <= 0);
                }

                private void BubbleUp()
                {
                    // This function operates on a list (m_listQueuedElements,
                    // where the heap property is true for every element
                    // except possibly the last element), and restores
                    // the heap property for the entire list.

                    if (IsEmpty())
                    {
                        throw new Exception("BubbleUp: priority queue is empty");
                    }

                    int nIndex = m_listQueuedElements.Count - 1;

                    while (nIndex > 0)
                    {
                        int nParentIndex = (nIndex - 1) / 2;    // This depends on integer division's truncation, not rounding.

                        if (m_listQueuedElements[nIndex].CompareTo(m_listQueuedElements[nParentIndex]) <= 0)
                        {
                            // nIndex's priority <= nParentIndex's priority; do not continue to bubble nIndex upwards.
                            break;
                        }

                        // Swap the list elements at indices nIndex and nParentIndex
                        T temp = m_listQueuedElements[nIndex];

                        m_listQueuedElements[nIndex] = m_listQueuedElements[nParentIndex];
                        m_listQueuedElements[nParentIndex] = temp;

                        nIndex = nParentIndex;
                    }
                }

                private void BubbleDown()
                {
                    // The heap property holds for the list, except for the fact that an element
                    // with unknown priority has been copied (not inserted) into the list at index 0.
                    // Now we will re-establish the heap property.

                    if (IsEmpty())
                    {
                        return;
                    }

                    int nIndex = 0;

                    for (; ; )
                    {
                        int nLeftChildIndex = 2 * nIndex + 1;
                        int nRightChildIndex = nLeftChildIndex + 1;
                        int nHigherPriorityChildIndex = nLeftChildIndex;      // By default

                        if (m_listQueuedElements.Count <= nLeftChildIndex)
                        {
                            // nIndex has bubbled all of the way down to the position of a leaf in the tree.
                            break;
                        }

                        if (m_listQueuedElements.Count > nRightChildIndex &&
                            m_listQueuedElements[nLeftChildIndex].CompareTo(m_listQueuedElements[nRightChildIndex]) < 0)
                        {
                            nHigherPriorityChildIndex = nRightChildIndex;
                        }

                        if (m_listQueuedElements[nIndex].CompareTo(m_listQueuedElements[nHigherPriorityChildIndex]) >= 0)
                        {
                            // nIndex's priority >= nHigherPriorityChildIndex's priority; do not continue to bubble nIndex downwards.
                            break;
                        }

                        // Swap the list elements at indices nIndex and nHigherPriorityChildIndex.
                        T temp = m_listQueuedElements[nIndex];

                        m_listQueuedElements[nIndex] = m_listQueuedElements[nHigherPriorityChildIndex];
                        m_listQueuedElements[nHigherPriorityChildIndex] = temp;

                        nIndex = nHigherPriorityChildIndex;
                    }
                }

                public void Enqueue(T elementToBePushed)
                {

                    lock (this)
                    {
                        m_listQueuedElements.Add(elementToBePushed);
                        BubbleUp();     // Re-establish the heap property.
                    }
                }

                private void EnqueueAll(IEnumerable<T> list)
                {
                    // ThAW 2009/05/27 : If we lock(this) here, will the lock inside Enqueue() hang the thread? : No.

                    // ThAW TODO 2009/05/27 : Provide an example of a scenario where a semantic error can occur
                    // if the object was not locked here:
                    // Assume that neither Refresh() nor EnqueueAll() lock this object.
                    // Thread 1: Begin refresh: Dequeue of all elements to new list L.  Queue is now empty.
                    // Thread 1: Inside refresh, EnqueueAll begins, and enqueues some (but not all) of elements in list L.
                    // Thread 2: Queue is cleared (Is this a valid queue operation?  If so, it should lock the queue object.)
                    // Thread 1: Inside refresh, EnqueueAll finishes, enqueueing the rest of the elements in list L.
                    // -> Queue has now been neither fully cleared or fully preserved; it is in an intermediate state.

                    lock (this)
                    {

                        foreach (T element in list)
                        {
                            Enqueue(element);
                        }
                    }
                }

                public T Peek()
                {

                    lock (this)     // Is this lock necessary?  This method does not modify the queue.
                    {

                        if (IsEmpty())
                        {
                            throw new Exception("Peek: priority queue is empty");
                        }

                        return m_listQueuedElements[0];
                    }
                }

                public T Dequeue()
                {

                    lock (this)
                    {

                        if (IsEmpty())
                        {
                            throw new Exception("Dequeue: priority queue is empty");
                        }

                        // Copy (the reference to) the first (i.e. the highest priority) element in the list,
                        // since we may be about to copy over that position in the list.
                        T highestPriorityElement = m_listQueuedElements[0];

                        int nIndexOfLastElement = m_listQueuedElements.Count - 1;

                        // If the list's last element is not also the first element (i.e. if their indices are distinct),
                        // then copy the last element onto the first element.
                        // As usual:
                        //   - If T is a value type, an actual value is copied;
                        //   - If T is a reference type, a reference to an object is copied; the object itself is not cloned.

                        if (nIndexOfLastElement > 0)
                        {
                            m_listQueuedElements[0] = m_listQueuedElements[nIndexOfLastElement];
                        }

                        // Remove the last element in the list.  This may result in an empty list.
                        m_listQueuedElements.RemoveAt(nIndexOfLastElement);

                        BubbleDown();   // Re-establish the heap property.
                        return highestPriorityElement;
                    }
                }

                public void DequeueAllToCollection(ICollection<T> collection, bool bClearCollection)
                {

                    if (collection == null)
                    {
                        throw new Exception("DequeueAllToCollection: cannot dequeue priority queue elements to a null collection reference");
                    }

                    if (bClearCollection)
                    {
                        collection.Clear();
                    }

                    // ThAW TODO 2009/05/27 : Provide an example of a scenario where a semantic error can occur
                    // if the object was not locked here.

                    lock (this)
                    {

                        while (!IsEmpty())
                        {
                            collection.Add(Dequeue());
                        }
                    }
                }

                public void DequeueAllToCollection(ICollection<T> collection)   // Overload, since C# does not support default parameters.
                {
                    // By default, clear the collection.
                    DequeueAllToCollection(collection, true);
                }

                // ThAW 2009/05/27 : Can we say:
                // public C<T> DequeueAllToNewCollection()<C> where C : ICollection<T> ???

                public List<T> DequeueAllToNewList()
                {
                    List<T> list = new List<T>();

                    DequeueAllToCollection(list, false);    // No need to Clear() because it's a newly created List<>.
                    return list;
                }

                // Refresh() can be useful if the relative priorities of the items in the queue can change
                // while they are enqueued (e.g. if the item is a hard drive, and the priority is the number of bytes free).

                public void Refresh()
                {
                    // ThAW TODO 2009/05/27 : Provide an example of a scenario where a semantic error can occur
                    // if the object was not locked here.
                    // Remember that two threads can be executing the same method at the same time.

                    lock (this)
                    {
                        EnqueueAll(DequeueAllToNewList());
                    }
                }
            }
        }
    }
}
