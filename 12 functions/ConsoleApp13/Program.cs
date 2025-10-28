using System.Text;

// LinkedListTasks.cs
// Собственная реализация односвязного списка целых и 12 функций (1..12).
// Демонстрация каждой функции в Main.

using System;
using System.Collections.Generic;
using System.Text;

class Node
{
    public int Value;
    public Node Next;
    public Node(int v, Node next = null) { Value = v; Next = next; }
}

class SinglyLinkedList
{
    public Node Head;
    public SinglyLinkedList() { Head = null; }

    public static SinglyLinkedList FromArray(int[] arr)
    {
        var lst = new SinglyLinkedList();
        Node tail = null;
        foreach (var v in arr)
        {
            var n = new Node(v);
            if (lst.Head == null) { lst.Head = n; tail = n; }
            else { tail.Next = n; tail = n; }
        }
        return lst;
    }

    public int[] ToArray()
    {
        var tmp = new List<int>();
        var cur = Head;
        while (cur != null) { tmp.Add(cur.Value); cur = cur.Next; }
        return tmp.ToArray();
    }

    public SinglyLinkedList Clone()
    {
        return FromArray(this.ToArray());
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        var cur = Head;
        while (cur != null)
        {
            sb.Append(cur.Value);
            if (cur.Next != null) sb.Append(" -> ");
            cur = cur.Next;
        }
        return sb.ToString();
    }

    // 1) Reverse (in-place)
    public void Reverse()
    {
        Node prev = null, cur = Head;
        while (cur != null)
        {
            Node next = cur.Next;
            cur.Next = prev;
            prev = cur;
            cur = next;
        }
        Head = prev;
    }

    // 2a) Move last to front
    public void MoveLastToFront()
    {
        if (Head == null || Head.Next == null) return;
        Node prev = null, cur = Head;
        while (cur.Next != null) { prev = cur; cur = cur.Next; }
        prev.Next = null;
        cur.Next = Head;
        Head = cur;
    }

    // 2b) Move first to end
    public void MoveFirstToEnd()
    {
        if (Head == null || Head.Next == null) return;
        Node first = Head;
        Head = Head.Next;
        first.Next = null;
        Node tail = Head;
        while (tail.Next != null) tail = tail.Next;
        tail.Next = first;
    }

    // 3) Count distinct elements
    public int CountDistinct()
    {
        var set = new HashSet<int>();
        Node cur = Head;
        while (cur != null) { set.Add(cur.Value); cur = cur.Next; }
        return set.Count;
    }

    // 4) Remove non-unique (leave only elements that appear exactly once)
    public void RemoveNonUnique()
    {
        var counts = new Dictionary<int, int>();
        Node cur = Head;
        while (cur != null)
        {
            counts.TryGetValue(cur.Value, out int c);
            counts[cur.Value] = c + 1;
            cur = cur.Next;
        }
        Node dummy = new Node(0) { Next = Head };
        Node prev = dummy;
        cur = Head;
        while (cur != null)
        {
            if (counts[cur.Value] > 1)
            {
                prev.Next = cur.Next;
                cur = prev.Next;
            }
            else
            {
                prev = cur;
                cur = cur.Next;
            }
        }
        Head = dummy.Next;
    }

    // 5) Insert copy of list after first occurrence of x
    public void InsertSelfAfterFirst(int x)
    {
        Node pos = Head;
        while (pos != null && pos.Value != x) pos = pos.Next;
        if (pos == null) return;
        var clone = this.Clone();
        if (clone.Head == null) return;
        Node after = pos.Next;
        pos.Next = clone.Head;
        Node tail = clone.Head;
        while (tail.Next != null) tail = tail.Next;
        tail.Next = after;
    }

    // 6) Insert into sorted (non-decreasing) list


public void InsertIntoSorted(int e)
    {
        Node newNode = new Node(e);
        if (Head == null || e < Head.Value)
        {
            newNode.Next = Head; Head = newNode; return;
        }
        Node cur = Head;
        while (cur.Next != null && cur.Next.Value <= e) cur = cur.Next;
        newNode.Next = cur.Next;
        cur.Next = newNode;
    }

    // 7) Remove all elements equal to E
    public void RemoveAll(int e)
    {
        Node dummy = new Node(0) { Next = Head };
        Node prev = dummy, cur = Head;
        while (cur != null)
        {
            if (cur.Value == e) { prev.Next = cur.Next; cur = prev.Next; }
            else { prev = cur; cur = cur.Next; }
        }
        Head = dummy.Next;
    }

    // 8) Insert F before first occurrence of E
    public void InsertBeforeFirst(int e, int f)
    {
        if (Head == null) return;
        if (Head.Value == e) { Head = new Node(f, Head); return; }
        Node prev = Head, cur = Head.Next;
        while (cur != null && cur.Value != e) { prev = cur; cur = cur.Next; }
        if (cur == null) return;
        var node = new Node(f);
        prev.Next = node;
        node.Next = cur;
    }

    // 9) Append other list to this one (use clone to avoid aliasing)
    public void AppendList(SinglyLinkedList other)
    {
        if (other == null || other.Head == null) return;
        if (Head == null) { Head = other.Clone().Head; return; }
        Node tail = Head;
        while (tail.Next != null) tail = tail.Next;
        tail.Next = other.Clone().Head;
    }

    // 10) Split by first occurrence of x:
    // this becomes elements up to and including first x; method returns the second list (elements after x).
    public SinglyLinkedList SplitAfterFirst(int x)
    {
        Node cur = Head;
        while (cur != null && cur.Value != x) cur = cur.Next;
        if (cur == null) return new SinglyLinkedList(); // x not found => second is empty, this unchanged
        var second = new SinglyLinkedList();
        second.Head = cur.Next;
        cur.Next = null;
        return second;
    }

    // 11) Duplicate list (append copy of self to end)
    public void Duplicate()
    {
        var clone = this.Clone();
        AppendList(clone);
    }

    // 12) Swap by indices i, j (0-based). If invalid indices => no-op.
    public void SwapByIndices(int i, int j)
    {
        if (i == j) return;
        if (i > j) { int t = i; i = j; j = t; }
        Node prevI = null, nodeI = Head;
        for (int k = 0; k < i && nodeI != null; k++) { prevI = nodeI; nodeI = nodeI.Next; }
        Node prevJ = null, nodeJ = Head;
        for (int k = 0; k < j && nodeJ != null; k++) { prevJ = nodeJ; nodeJ = nodeJ.Next; }
        if (nodeI == null || nodeJ == null) return;

        if (prevI != null) prevI.Next = nodeJ; else Head = nodeJ;
        if (prevJ != null) prevJ.Next = nodeI; else Head = nodeI;

        Node tmp = nodeI.Next;
        nodeI.Next = nodeJ.Next;
        nodeJ.Next = tmp;
    }

    // Дополнительно: swap by values (первые вхождения)
    public void SwapByValues(int v1, int v2)
    {
        if (v1 == v2) return;
        Node prev1 = null, n1 = Head;
        while (n1 != null && n1.Value != v1) { prev1 = n1; n1 = n1.Next; }
        Node prev2 = null, n2 = Head;
        while (n2 != null && n2.Value != v2) { prev2 = n2; n2 = n2.Next; }
        if (n1 == null || n2 == null) return;

        if (prev1 != null) prev1.Next = n2; else Head = n2;
        if (prev2 != null) prev2.Next = n1; else Head = n1;

        Node tmp = n1.Next;
        n1.Next = n2.Next;
        n2.Next = tmp;
    }
}

class LinkedListTasksDemo
{
    static void PrintHeader(string h) { Console.WriteLine($"\n--- {h} ---"); }

    static void Main()
    {
        Console.WriteLine("=== LinkedListTasks Demo (1..12) ===");

        // 1 Reverse
        var l1 = SinglyLinkedList.FromArray(new[] { 1, 2, 3, 4, 5 });
        PrintHeader("1) Reverse");
        Console.WriteLine("Before: " + l1);
        l1.Reverse();
        Console.WriteLine("After:  " + l1);


// 2 MoveLastToFront / MoveFirstToEnd
        var l2 = SinglyLinkedList.FromArray(new[] { 10, 20, 30, 40 });
        PrintHeader("2) MoveLastToFront / MoveFirstToEnd");
        Console.WriteLine("Start: " + l2);
        l2.MoveLastToFront();
        Console.WriteLine("After MoveLastToFront: " + l2);
        l2.MoveFirstToEnd();
        Console.WriteLine("After MoveFirstToEnd:  " + l2);

        // 3 CountDistinct
        var l3 = SinglyLinkedList.FromArray(new[] { 1, 2, 2, 3, 1, 4 });
        PrintHeader("3) CountDistinct");
        Console.WriteLine(l3 + " -> distinct count = " + l3.CountDistinct());

        // 4 RemoveNonUnique
        var l4 = SinglyLinkedList.FromArray(new[] { 1, 2, 2, 3, 1, 4, 5 });
        PrintHeader("4) RemoveNonUnique");
        Console.WriteLine("Before: " + l4);
        l4.RemoveNonUnique();
        Console.WriteLine("After:  " + l4);

        // 5 InsertSelfAfterFirst
        var l5 = SinglyLinkedList.FromArray(new[] { 7, 8, 9 });
        PrintHeader("5) InsertSelfAfterFirst(8)");
        Console.WriteLine("Before: " + l5);
        l5.InsertSelfAfterFirst(8);
        Console.WriteLine("After:  " + l5);

        // 6 InsertIntoSorted
        var l6 = SinglyLinkedList.FromArray(new[] { 1, 2, 4, 5 });
        PrintHeader("6) InsertIntoSorted");
        Console.WriteLine("Before: " + l6);
        l6.InsertIntoSorted(3);
        Console.WriteLine("After insert 3: " + l6);
        l6.InsertIntoSorted(0);
        Console.WriteLine("After insert 0: " + l6);
        l6.InsertIntoSorted(6);
        Console.WriteLine("After insert 6: " + l6);

        // 7 RemoveAll
        var l7 = SinglyLinkedList.FromArray(new[] { 2, 3, 2, 4, 2 });
        PrintHeader("7) RemoveAll(2)");
        Console.WriteLine("Before: " + l7);
        l7.RemoveAll(2);
        Console.WriteLine("After:  " + l7);

        // 8 InsertBeforeFirst
        var l8 = SinglyLinkedList.FromArray(new[] { 5, 6, 7, 6 });
        PrintHeader("8) InsertBeforeFirst(E=6, F=99)");
        Console.WriteLine("Before: " + l8);
        l8.InsertBeforeFirst(6, 99);
        Console.WriteLine("After:  " + l8);

        // 9 AppendList
        var la = SinglyLinkedList.FromArray(new[] { 1, 2, 3 });
        var lb = SinglyLinkedList.FromArray(new[] { 4, 5, 6 });
        PrintHeader("9) AppendList");
        Console.WriteLine("A: " + la);
        Console.WriteLine("B: " + lb);
        la.AppendList(lb);
        Console.WriteLine("A after append B: " + la);

        // 10 SplitAfterFirst
        var l10 = SinglyLinkedList.FromArray(new[] { 1, 2, 3, 4, 3, 5 });
        PrintHeader("10) SplitAfterFirst(3)");
        Console.WriteLine("Before: " + l10);
        var second = l10.SplitAfterFirst(3);
        Console.WriteLine("First after split:  " + l10);
        Console.WriteLine("Second returned:    " + second);

        // 11 Duplicate
        var l11 = SinglyLinkedList.FromArray(new[] { 9, 8, 7 });
        PrintHeader("11) Duplicate");
        Console.WriteLine("Before: " + l11);
        l11.Duplicate();
        Console.WriteLine("After:  " + l11);

        // 12 SwapByIndices / SwapByValues
        var l12 = SinglyLinkedList.FromArray(new[] { 1, 2, 3, 4, 5 });
        PrintHeader("12) SwapByIndices(1,3) and SwapByValues(5,1)");
        Console.WriteLine("Before: " + l12);
        l12.SwapByIndices(1, 3);
        Console.WriteLine("After SwapByIndices(1,3): " + l12);
        l12.SwapByValues(5, 1);
        Console.WriteLine("After SwapByValues(5,1):  " + l12);

        Console.WriteLine("\n=== End LinkedListTasks Demo ===");
    }
}