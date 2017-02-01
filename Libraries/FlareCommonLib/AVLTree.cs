// AVLTree.cs - Class template for a height-balanced tree - September 11, 2009

// The original code for this file was stolen by Tom Weatherhead from Eric Lippert's blog:
// http://blogs.msdn.com/ericlippert/archive/2007/12/04/immutability-in-c-part-two-a-simple-immutable-stack.aspx
// http://blogs.msdn.com/ericlippert/archive/2007/12/18/immutability-in-c-part-six-a-simple-binary-tree.aspx
// http://blogs.msdn.com/ericlippert/archive/2007/12/19/immutability-in-c-part-seven-more-on-binary-trees.aspx
// http://blogs.msdn.com/ericlippert/archive/2008/01/18/immutability-in-c-part-eight-even-more-on-binary-trees.aspx
// http://blogs.msdn.com/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx

#define USE_C_SHARP_3_0_LANGUAGE_FEATURES
#define IN_ORDER_TRAVERSAL

using System;
using System.Collections;           // Needed for the non-generic IEnumerator and IEnumerable
using System.Collections.Generic;
using System.Linq;                  // The C# 3.0 code uses LINQ
using System.Text;

namespace SolarFlareCommon.Collections.Generic
{
    // From http://blogs.msdn.com/ericlippert/archive/2007/12/04/immutability-in-c-part-two-a-simple-immutable-stack.aspx :

    public interface IStack<T> : IEnumerable<T>
    {
        IStack<T> Push(T value);
        IStack<T> Pop();
        T Peek();
        bool IsEmpty { get; }
    }

    public sealed class Stack<T> : IStack<T>
    {
        private sealed class EmptyStack : IStack<T>
        {
            public bool IsEmpty
            {
                get
                {
                    return true;
                }
            }

            public T Peek()
            {
                throw new Exception(@"Empty stack");
            }

            public IStack<T> Push(T value)
            {
                return new Stack<T>(value, this);
            }

            public IStack<T> Pop()
            {
                throw new Exception(@"Empty stack");
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        } // class EmptyStack

        private static readonly EmptyStack empty = new EmptyStack();

        public static IStack<T> Empty
        {
            get
            {
                return empty;
            }
        }

        private readonly T head;
        private readonly IStack<T> tail;

        private Stack(T head, IStack<T> tail)
        {
            this.head = head;
            this.tail = tail;
        }

        public bool IsEmpty
        {
            get
            {
                return false;
            }
        }

        public T Peek()
        {
            return head;
        }

        public IStack<T> Pop()
        {
            return tail;
        }

        public IStack<T> Push(T value)
        {
            return new Stack<T>(value, this);
        }

        public IEnumerator<T> GetEnumerator()
        {

            for (IStack<T> stack = this; !stack.IsEmpty; stack = stack.Pop())
            {
                yield return stack.Peek();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
    } // class Stack<T>

    // From http://blogs.msdn.com/ericlippert/archive/2007/12/18/immutability-in-c-part-six-a-simple-binary-tree.aspx :

    public interface IBinaryTree<V>
    {
        bool IsEmpty { get; }
        V Value { get; }
        IBinaryTree<V> Left { get; }
        IBinaryTree<V> Right { get; }
    }

    public sealed class BinaryTree<V> : IBinaryTree<V>
    {
        private sealed class EmptyBinaryTree : IBinaryTree<V>
        {
            public bool IsEmpty
            {
                get
                {
                    return true;
                }
            }

            public IBinaryTree<V> Left
            {
                get
                {
                    throw new Exception(@"Empty tree");
                }
            }

            public IBinaryTree<V> Right
            {
                get
                {
                    throw new Exception(@"Empty tree");
                }
            }

            public V Value
            {
                get
                {
                    throw new Exception(@"Empty tree");
                }
            }
        } // class EmptyBinaryTree

        private static readonly EmptyBinaryTree empty = new EmptyBinaryTree();

        public static IBinaryTree<V> Empty
        {
            get
            {
                return empty;
            }
        }

        private readonly V value;
        private readonly IBinaryTree<V> left;
        private readonly IBinaryTree<V> right;

        public bool IsEmpty
        {
            get
            {
                return false;
            }
        }

        public V Value
        {
            get
            {
                return value;
            }
        }

        public IBinaryTree<V> Left
        {
            get
            {
                return left;
            }
        }

        public IBinaryTree<V> Right
        {
            get
            {
                return right;
            }
        }

        public BinaryTree(V value, IBinaryTree<V> left, IBinaryTree<V> right)
        {
            this.value = value;
            this.left = left ?? Empty;      // ... = (left != null) ? left : Empty;
            this.right = right ?? Empty;
        }

#if IN_ORDER_TRAVERSAL
#if !USE_C_SHARP_3_0_LANGUAGE_FEATURES
        // Originally from http://blogs.msdn.com/ericlippert/archive/2007/12/19/immutability-in-c-part-seven-more-on-binary-trees.aspx ;
        // modified to work in C# 2.0

        public static IEnumerable<T> InOrder<T>(IBinaryTree<T> tree)
        {
            IStack<IBinaryTree<T>> stack = Stack<IBinaryTree<T>>.Empty;
            for (IBinaryTree<T> current = tree; !current.IsEmpty || !stack.IsEmpty; current = current.Right)
            {
                while (!current.IsEmpty)
                {
                    stack = stack.Push(current);
                    current = current.Left;
                }
                current = stack.Peek();
                stack = stack.Pop();
                yield return current.Value;
            }
        }
#endif
#endif
    } // class BinaryTree<V>

#if IN_ORDER_TRAVERSAL
#if USE_C_SHARP_3_0_LANGUAGE_FEATURES
    public static class BinaryTreeExtensionMethods  // ThAW 2009/09/26 : This class was not specified in the article, but it is necessary.
    {
        // From http://blogs.msdn.com/ericlippert/archive/2007/12/19/immutability-in-c-part-seven-more-on-binary-trees.aspx :

        // C# 3.0 : Extension method (note the "this" keyword before the type of the first parameter)

        public static IEnumerable<T> InOrder<T>(this IBinaryTree<T> tree)
        {
            IStack<IBinaryTree<T>> stack = Stack<IBinaryTree<T>>.Empty;

            for (IBinaryTree<T> current = tree; !current.IsEmpty || !stack.IsEmpty; current = current.Right)
            {

                while (!current.IsEmpty)
                {
                    stack = stack.Push(current);
                    current = current.Left;
                }

                current = stack.Peek();
                stack = stack.Pop();
                yield return current.Value;
            }
        }
    } // class BinaryTreeExtensionMethods
#endif
#endif

    // From http://blogs.msdn.com/ericlippert/archive/2008/01/18/immutability-in-c-part-eight-even-more-on-binary-trees.aspx :

    public interface IMap<K, V> where K : IComparable<K>
    {
        bool Contains(K key);
        V Lookup(K key);
        IMap<K, V> Add(K key, V value);
        IMap<K, V> Remove(K key);
#if USE_C_SHARP_3_0_LANGUAGE_FEATURES
        IEnumerable<K> Keys { get; }
        IEnumerable<V> Values { get; }
        IEnumerable<KeyValuePair<K, V>> Pairs { get; }
#endif
    }

    public interface IBinarySearchTree<K, V> :
        IBinaryTree<V>,
        IMap<K, V>
        where K : IComparable<K>
    {
        K Key { get; }
        new IBinarySearchTree<K, V> Left { get; }
        new IBinarySearchTree<K, V> Right { get; }
        new IBinarySearchTree<K, V> Add(K key, V value);
        new IBinarySearchTree<K, V> Remove(K key);
        IBinarySearchTree<K, V> Search(K key);
    }

    // From http://blogs.msdn.com/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx :

    public sealed class AVLTree<K, V> : IBinarySearchTree<K, V> where K : IComparable<K>
    {
        private sealed class EmptyAVLTree : IBinarySearchTree<K, V>
        {
            // IBinaryTree

            public bool IsEmpty
            {
                get
                {
                    return true;
                }
            }

            public V Value
            {
                get
                {
                    throw new Exception(@"Empty tree");
                }
            }

            IBinaryTree<V> IBinaryTree<V>.Left
            {
                get
                {
                    throw new Exception(@"Empty tree");
                }
            }

            IBinaryTree<V> IBinaryTree<V>.Right
            {
                get
                {
                    throw new Exception(@"Empty tree");
                }
            }

            // IBinarySearchTree

            public IBinarySearchTree<K, V> Left
            {
                get
                {
                    throw new Exception(@"Empty tree");
                }
            }

            public IBinarySearchTree<K, V> Right
            {
                get
                {
                    throw new Exception(@"Empty tree");
                }
            }

            public IBinarySearchTree<K, V> Search(K key)
            {
                return this;
            }

            public K Key
            {
                get
                {
                    throw new Exception(@"Empty tree");
                }
            }

            public IBinarySearchTree<K, V> Add(K key, V value)
            {
                return new AVLTree<K, V>(key, value, this, this);
            }

            public IBinarySearchTree<K, V> Remove(K key)
            {
                throw new Exception(@"Cannot remove item that is not in tree.");
            }

            // IMap

            public bool Contains(K key)
            {
                return false;
            }

            public V Lookup(K key)
            {
                throw new Exception(@"Not found");
            }

            IMap<K, V> IMap<K, V>.Add(K key, V value)
            {
                return this.Add(key, value);
            }

            IMap<K, V> IMap<K, V>.Remove(K key)
            {
                return this.Remove(key);
            }

#if USE_C_SHARP_3_0_LANGUAGE_FEATURES
            public IEnumerable<K> Keys
            {
                get
                {
                    yield break;
                }
            }

            public IEnumerable<V> Values
            {
                get
                {
                    yield break;
                }
            }

            public IEnumerable<KeyValuePair<K,V>> Pairs
            {
                get
                {
                    yield break;
                }
            }
#endif
        } // class EmptyAVLTree

        private static readonly EmptyAVLTree empty = new EmptyAVLTree();

        public static IBinarySearchTree<K, V> Empty
        {
            get
            {
                return empty;
            }
        }

        private readonly K key;
        private readonly V value;
        private readonly IBinarySearchTree<K, V> left;
        private readonly IBinarySearchTree<K, V> right;
        private readonly int height;

        private AVLTree(K key, V value, IBinarySearchTree<K, V> left, IBinarySearchTree<K, V> right)
        {
            this.key = key;
            this.value = value;
            this.left = left;
            this.right = right;
            this.height = 1 + Math.Max(Height(left), Height(right));
        }

        // IBinaryTree

        public bool IsEmpty
        {
            get
            {
                return false;
            }
        }

        public V Value
        {
            get
            {
                return value;
            }
        }

        IBinaryTree<V> IBinaryTree<V>.Left
        {
            get
            {
                return left;
            }
        }

        IBinaryTree<V> IBinaryTree<V>.Right
        {
            get
            {
                return right;
            }
        }

        // IBinarySearchTree

        public IBinarySearchTree<K, V> Left
        {
            get
            {
                return left;
            }
        }

        public IBinarySearchTree<K, V> Right
        {
            get
            {
                return right;
            }
        }

        public IBinarySearchTree<K, V> Search(K key)
        {
            int compare = key.CompareTo(Key);

            if (compare == 0)
            {
                return this;
            }
            else if (compare > 0)
            {
                return Right.Search(key);
            }
            else
            {
                return Left.Search(key);
            }
        }

        public K Key
        {
            get
            {
                return key;
            }
        }

        public IBinarySearchTree<K, V> Add(K key, V value)
        {
            AVLTree<K, V> result;

            if (key.CompareTo(Key) > 0)
            {
                result = new AVLTree<K, V>(Key, Value, Left, Right.Add(key, value));
            }
            else
            {
                result = new AVLTree<K, V>(Key, Value, Left.Add(key, value), Right);
            }

            return MakeBalanced(result);
        }

        public IBinarySearchTree<K, V> Remove(K key)
        {
            IBinarySearchTree<K, V> result;
            int compare = key.CompareTo(Key);

            if (compare == 0)
            {
                // We have a match. If this is a leaf, just remove it
                // by returning Empty.  If we have only one child,
                // replace the node with the child.

                if (Right.IsEmpty && Left.IsEmpty)
                {
                    result = Empty;
                }
                else if (Right.IsEmpty && !Left.IsEmpty)
                {
                    result = Left;
                }
                else if (!Right.IsEmpty && Left.IsEmpty)
                {
                    result = Right;
                }
                else
                {
                    // We have two children. Remove the next-highest node and replace
                    // this node with it.
                    IBinarySearchTree<K, V> successor = Right;

                    while (!successor.Left.IsEmpty)
                    {
                        successor = successor.Left;
                    }

                    result = new AVLTree<K, V>(successor.Key, successor.Value, Left, Right.Remove(successor.Key));
                }
            }
            else if (compare < 0)
            {
                result = new AVLTree<K, V>(Key, Value, Left.Remove(key), Right);
            }
            else
            {
                result = new AVLTree<K, V>(Key, Value, Left, Right.Remove(key));
            }

            return MakeBalanced(result);
        }

        // IMap

        public bool Contains(K key)
        {
            return !Search(key).IsEmpty;
        }

        IMap<K, V> IMap<K, V>.Add(K key, V value)
        {
            return this.Add(key, value);
        }

        IMap<K, V> IMap<K, V>.Remove(K key)
        {
            return this.Remove(key);
        }

        public V Lookup(K key)
        {
            IBinarySearchTree<K, V> tree = Search(key);

            if (tree.IsEmpty)
            {
                throw new Exception(@"Not found");
            }

            return tree.Value;
        }

#if USE_C_SHARP_3_0_LANGUAGE_FEATURES
        // ThAW : "select..." : Is this LINQ?  Yes.

        public IEnumerable<K> Keys
        {
            get
            {
                return from t in Enumerate() select t.Key;
            }
        }

        public IEnumerable<V> Values
        {
            get
            {
                return from t in Enumerate() select t.Value;
            }
        }

        public IEnumerable<KeyValuePair<K,V>> Pairs
        {
            get
            {
                return from t in Enumerate() select new KeyValuePair<K, V>(t.Key, t.Value);
            }
        }
#endif

        private IEnumerable<IBinarySearchTree<K, V>> Enumerate()
        {
#if USE_C_SHARP_3_0_LANGUAGE_FEATURES
            // Implicitly typed variable declaration.
            var stack = Stack<IBinarySearchTree<K,V>>.Empty;
#else
            IStack<IBinarySearchTree<K, V>> stack = Stack<IBinarySearchTree<K, V>>.Empty;
#endif

            for (IBinarySearchTree<K, V> current = this; !current.IsEmpty || !stack.IsEmpty; current = current.Right)
            {

                while (!current.IsEmpty)
                {
                    stack = stack.Push(current);
                    current = current.Left;
                }

                current = stack.Peek();
                stack = stack.Pop();
                yield return current;
            }
        }

        // Static helpers for tree balancing

        private static int Height(IBinarySearchTree<K, V> tree)
        {

            if (tree.IsEmpty)
            {
                return 0;
            }

            return ((AVLTree<K, V>)tree).height;
        }

        private static IBinarySearchTree<K, V> RotateLeft(IBinarySearchTree<K, V> tree)
        {

            if (tree.Right.IsEmpty)
            {
                return tree;
            }

            return new AVLTree<K, V>(tree.Right.Key, tree.Right.Value,
                new AVLTree<K, V>(tree.Key, tree.Value, tree.Left, tree.Right.Left),
                tree.Right.Right);
        }

        private static IBinarySearchTree<K, V> RotateRight(IBinarySearchTree<K, V> tree)
        {

            if (tree.Left.IsEmpty)
            {
                return tree;
            }

            return new AVLTree<K, V>(tree.Left.Key, tree.Left.Value, tree.Left.Left,
                new AVLTree<K, V>(tree.Key, tree.Value, tree.Left.Right, tree.Right));
        }

        private static IBinarySearchTree<K, V> DoubleLeft(IBinarySearchTree<K, V> tree)
        {

            if (tree.Right.IsEmpty)
            {
                return tree;
            }

            AVLTree<K, V> rotatedRightChild = new AVLTree<K, V>(tree.Key, tree.Value, tree.Left, RotateRight(tree.Right));
            return RotateLeft(rotatedRightChild);
        }

        private static IBinarySearchTree<K, V> DoubleRight(IBinarySearchTree<K, V> tree)
        {

            if (tree.Left.IsEmpty)
            {
                return tree;
            }

            AVLTree<K, V> rotatedLeftChild = new AVLTree<K, V>(tree.Key, tree.Value, RotateLeft(tree.Left), tree.Right);
            return RotateRight(rotatedLeftChild);
        }

        private static int Balance(IBinarySearchTree<K, V> tree)
        {

            if (tree.IsEmpty)
            {
                return 0;
            }

            return Height(tree.Right) - Height(tree.Left);
        }

        private static bool IsRightHeavy(IBinarySearchTree<K, V> tree)
        {
            return Balance(tree) >= 2;
        }

        private static bool IsLeftHeavy(IBinarySearchTree<K, V> tree)
        {
            return Balance(tree) <= -2;
        }

        private static IBinarySearchTree<K, V> MakeBalanced(IBinarySearchTree<K, V> tree)
        {
            IBinarySearchTree<K, V> result;

            if (IsRightHeavy(tree))
            {

                if (IsLeftHeavy(tree.Right))
                {
                    result = DoubleLeft(tree);
                }
                else
                {
                    result = RotateLeft(tree);
                }
            }
            else if (IsLeftHeavy(tree))
            {

                if (IsRightHeavy(tree.Left))
                {
                    result = DoubleRight(tree);
                }
                else
                {
                    result = RotateRight(tree);
                }
            }
            else
            {
                result = tree;
            }

            return result;
        }
    } // class AVLTree<K, V>
} // namespace SolarFlareCommon.Collections.Generic

// **** End of File ****
