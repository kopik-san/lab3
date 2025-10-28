using System;
using System.Collections.Generic;


public class FunctionCall
{
    public string FunctionName { get; }
    public int LineNumber { get; }
    public DateTime CallTime { get; }

    public FunctionCall(string name, int line)
    {
        FunctionName = name;
        LineNumber = line;
        CallTime = DateTime.Now;
    }

    public override string ToString()
    {
        return $"{FunctionName} (line {LineNumber}) at {CallTime:HH:mm:ss.fff}";
    }
}


public class CallStackManager
{
    private Stack<FunctionCall> _callStack;

    public CallStackManager()
    {
        _callStack = new Stack<FunctionCall>();
    }


    public void CallFunction(string functionName, int lineNumber)
    {
        var call = new FunctionCall(functionName, lineNumber);
        _callStack.Push(call);
        Console.WriteLine($"→ Вызвана функция: {call}");
        PrintCallStack();
    }


    public void ReturnFromFunction()
    {
        if (_callStack.Count > 0)
        {
            var returnedCall = _callStack.Pop();
            Console.WriteLine($"← Возврат из функции: {returnedCall.FunctionName}");
            PrintCallStack();
        }
        else
        {
            Console.WriteLine("Стек вызовов пуст!");
        }
    }


    public void PrintCallStack()
    {
        Console.WriteLine("Текущий стек вызовов (сверху вниз):");

        if (_callStack.Count == 0)
        {
            Console.WriteLine("  [пусто]");
            return;
        }


        var stackArray = _callStack.ToArray();
        for (int i = stackArray.Length - 1; i >= 0; i--)
        {
            Console.WriteLine($"  [{stackArray.Length - i}] {stackArray[i]}");
        }
        Console.WriteLine();
    }

    public int GetStackDepth()
    {
        return _callStack.Count;
    }
}


public class CallStackDemo
{
    public static void Main()
    {
        Console.WriteLine("=== ДЕМОНСТРАЦИЯ CALL STACK ===\n");

        var callStack = new CallStackManager();


        callStack.CallFunction("Main", 1);

        callStack.CallFunction("ProcessData", 15);

        callStack.CallFunction("ValidateInput", 32);

        callStack.CallFunction("SanitizeString", 47);


        callStack.ReturnFromFunction(); // Возврат из SanitizeString
        callStack.ReturnFromFunction(); // Возврат из ValidateInput

        callStack.CallFunction("SaveToDatabase", 25);

        callStack.ReturnFromFunction(); // Возврат из SaveToDatabase
        callStack.ReturnFromFunction(); // Возврат из ProcessData
        callStack.ReturnFromFunction(); // Возврат из Main

        // Попытка возврата из пустого стека
        callStack.ReturnFromFunction();
    }
}