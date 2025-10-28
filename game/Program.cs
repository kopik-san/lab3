using System;
using System.Collections.Generic;

public class LevelNode
{
    public string Name;
    public LevelNode Next;
    public LevelNode(string name) { Name = name; }
}

public class LevelChain
{
    private LevelNode head;
    private LevelNode current;
    private Stack<LevelNode> historyBack = new Stack<LevelNode>(); // чтобы уметь вернуться назад

    public LevelChain(IEnumerable<string> initialLevels)
    {
        LevelNode tail = null;
        foreach (var lvl in initialLevels)
        {
            var n = new LevelNode(lvl);
            if (head == null) { head = n; tail = n; }
            else { tail.Next = n; tail = n; }
        }
        current = head; 
    }

    public string Current => current?.Name;


    public bool Next()
    {
        if (current?.Next == null) return false;
        historyBack.Push(current);
        current = current.Next;
        return true;
    }

    public bool Prev()
    {
        if (historyBack.Count == 0) return false;
        current = historyBack.Pop();
        return true;
    }

    public void InsertAfterCurrent(string newLevelName)
    {
        var n = new LevelNode(newLevelName);
        n.Next = current.Next;
        current.Next = n;
    }


    public override string ToString()
    {
        var cur = head;
        var s = "";
        while (cur != null)
        {
            s += (cur == current ? $"[{cur.Name}]" : cur.Name);
            if (cur.Next != null) s += " -> ";
            cur = cur.Next;
        }
        return s;
    }
}




class Program
{
    static void Main()
    {
        var chain = new LevelChain(new[] { "L1", "L2", "L3" });
        Console.WriteLine(chain); // [L1] -> L2 -> L3

        chain.Next();
        Console.WriteLine(chain); // L1 -> [L2] -> L3

        chain.InsertAfterCurrent("L2.5");
        Console.WriteLine(chain); // L1 -> [L2] -> L2.5 -> L3

        chain.Next();
        Console.WriteLine(chain); // L1 -> L2 -> [L2.5] -> L3

        chain.Next();
        Console.WriteLine(chain); // L1 -> L2 -> L2.5 -> [L3]

        chain.Prev();
        Console.WriteLine(chain); // L1 -> L2 -> [L2.5] -> L3
    }
}