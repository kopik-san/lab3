using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

// Полный однофайловый проект C# для лабораторной работы:
// - собственная связная структура списка и 12 функций для неё
// - стек на базе собственного связного списка
// - очередь на базе собственного связного списка + реализация с System.Queue
// - перевод инфиксной записи в постфикс (shunting-yard) и вычисление постфикса
// - чтение операций из input.txt для стека и очереди
// - измерения времени и запись результатов в CSV

namespace LabStructures
{
    #region Linked List
    public class Node<T>
    {
        public T Value;
        public Node<T> Next;
        public Node(T val) { Value = val; Next = null; }
    }

    public class LinkedListCustom<T>
    {
        public Node<T> Head;
        public LinkedListCustom() { Head = null; }

        public bool IsEmpty() => Head == null;

        public void AddFront(T val)
        {
            var n = new Node<T>(val) { Next = Head };
            Head = n;
        }

        public void AddBack(T val)
        {
            var n = new Node<T>(val);
            if (Head == null) { Head = n; return; }
            var cur = Head;
            while (cur.Next != null) cur = cur.Next;
            cur.Next = n;
        }

        public T RemoveFront()
        {
            if (Head == null) throw new InvalidOperationException("List empty");
            var v = Head.Value;
            Head = Head.Next;
            return v;
        }

        public T PeekFront()
        {
            if (Head == null) throw new InvalidOperationException("List empty");
            return Head.Value;
        }

        public void Print(TextWriter writer = null)
        {
            writer = writer ?? Console.Out;
            var cur = Head;
            var sb = new StringBuilder();
            while (cur != null) { sb.Append(cur.Value); if (cur.Next != null) sb.Append(" -> "); cur = cur.Next; }
            writer.WriteLine(sb.ToString());
        }

        // 1. Перевернуть список (изменить ссылки)
        public void Reverse()
        {
            Node<T> prev = null, cur = Head;
            while (cur != null)
            {
                var next = cur.Next;
                cur.Next = prev;
                prev = cur;
                cur = next;
            }
            Head = prev;
        }

        // 2. Перенос последнего в начало
        public void MoveLastToFront()
        {
            if (Head == null || Head.Next == null) return;
            Node<T> prev = null, cur = Head;
            while (cur.Next != null) { prev = cur; cur = cur.Next; }
            prev.Next = null;
            cur.Next = Head;
            Head = cur;
        }

        // Перенос первого в конец
        public void MoveFirstToLast()
        {
            if (Head == null || Head.Next == null) return;
            var first = Head;
            Head = Head.Next;
            first.Next = null;
            var cur = Head;
            while (cur.Next != null) cur = cur.Next;
            cur.Next = first;
        }

        // 3. Количество различных элементов (для int)
        public int CountDistinct()
        {
            var set = new HashSet<T>();
            var cur = Head;
            while (cur != null) { set.Add(cur.Value); cur = cur.Next; }
            return set.Count;
        }

        // 4. Удаляет неуникальные элементы (оставляет только элементы, встречающиеся 1 раз)
        public void RemoveNonUnique()
        {
            // подсчитать частоты
            var freq = new Dictionary<T, int>();
            var cur = Head;
            while (cur != null)
            {
                if (!freq.ContainsKey(cur.Value)) freq[cur.Value] = 0;
                freq[cur.Value]++;
                cur = cur.Next;
            }
            // построить новый список с элементами freq==1
            Node<T> dummy = new Node<T>(default(T));
            var tail = dummy;
            cur = Head;
            while (cur != null)
            {
                if (freq[cur.Value] == 1) { tail.Next = new Node<T>(cur.Value); tail = tail.Next; }
                cur = cur.Next;
            }
            Head = dummy.Next;
        }

        // 5. Вставка списка самого в себя вслед за первым вхождением x
        public void InsertListIntoItselfAfterFirstX(T x)
        {
            if (Head == null) return;
            var cur = Head;
            while (cur != null && !EqualityComparer<T>.Default.Equals(cur.Value, x)) cur = cur.Next;
            if (cur == null) return; // x не найден
            // клонируем текущий список
            var cloneHead = CloneList(Head);
            // вставляем clone после cur
            var cloneTail = cloneHead;
            while (cloneTail != null && cloneTail.Next != null) cloneTail = cloneTail.Next;
            cloneTail.Next = cur.Next;
            cur.Next = cloneHead;
        }

        private Node<T> CloneList(Node<T> start)
        {
            if (start == null) return null;
            Node<T> newHead = new Node<T>(start.Value);
            var curNew = newHead;
            var curOld = start.Next;
            while (curOld != null) { curNew.Next = new Node<T>(curOld.Value); curNew = curNew.Next; curOld = curOld.Next; }
            return newHead;
        }

        // 6. Вставка в упорядоченный по неубыванию список
        public void InsertSorted(T val)
        {
            var n = new Node<T>(val);
            var comparer = Comparer<T>.Default;

            if (Head == null || comparer.Compare(Head.Value, val) > 0)
            {
                n.Next = Head;
                Head = n;
                return;
            }

            var cur = Head;
            while (cur.Next != null && comparer.Compare(cur.Next.Value, val) <= 0)
                cur = cur.Next;

            n.Next = cur.Next;
            cur.Next = n;
        }

        // 7. Удаляет все элементы E
        public void RemoveAll(T e)
        {
            Node<T> dummy = new Node<T>(default(T)) { Next = Head };
            var cur = dummy;
            while (cur.Next != null)
            {
                if (EqualityComparer<T>.Default.Equals(cur.Next.Value, e)) cur.Next = cur.Next.Next;
                else cur = cur.Next;
            }
            Head = dummy.Next;
        }

        // 8. Вставляет F перед первым вхождением E
        public void InsertBeforeFirst(T f, T e)
        {
            if (Head == null) return;
            if (EqualityComparer<T>.Default.Equals(Head.Value, e)) { AddFront(f); return; }
            var cur = Head;
            while (cur.Next != null && !EqualityComparer<T>.Default.Equals(cur.Next.Value, e)) cur = cur.Next;
            if (cur.Next == null) return; // E не найден
            var n = new Node<T>(f) { Next = cur.Next };
            cur.Next = n;
        }

        // 9. Дописывает к списку L список E
        public void AppendList(LinkedListCustom<T> other)
        {
            if (other == null || other.Head == null) return;
            if (Head == null) { Head = CloneList(other.Head); return; }
            var cur = Head;
            while (cur.Next != null) cur = cur.Next;
            cur.Next = CloneList(other.Head);
        }

        // 10. Разбивает по первому вхождению x: возвращает второй список
        public LinkedListCustom<T> SplitByFirstX(T x)
        {
            var second = new LinkedListCustom<T>();
            if (Head == null) return second;
            if (EqualityComparer<T>.Default.Equals(Head.Value, x))
            {
                second.Head = Head.Next;
                Head.Next = null;
                return second;
            }
            var cur = Head;
            while (cur.Next != null && !EqualityComparer<T>.Default.Equals(cur.Next.Value, x)) cur = cur.Next;
            if (cur.Next == null) return second; // x не найден
            second.Head = cur.Next.Next;
            cur.Next = null;
            return second;
        }

        // 11. Удваивает список (приписывает к концу копию самого себя)
        public void DuplicateList()
        {
            if (Head == null) return;
            var clone = CloneList(Head);
            var cur = Head;
            while (cur.Next != null) cur = cur.Next;
            cur.Next = clone;
        }

        // 12. Меняет местами два элемента по индексам (i и j, 0-based). Меняются значения.
        public void SwapElementsByIndices(int i, int j)
        {
            if (i == j) return;
            Node<T> a = null, b = null, cur = Head; int idx = 0;
            while (cur != null)
            {
                if (idx == i) a = cur;
                if (idx == j) b = cur;
                cur = cur.Next; idx++;
            }
            if (a == null || b == null) throw new ArgumentOutOfRangeException("Индекс выходит за пределы списка");
            var tmp = a.Value; a.Value = b.Value; b.Value = tmp;
        }

        // Утилита: возвращает коллекцию значений
        public IEnumerable<T> ToEnumerable()
        {
            var cur = Head;
            while (cur != null) { yield return cur.Value; cur = cur.Next; }
        }
    }
    #endregion

    #region Stack
    public class StackCustom<T>
    {
        private LinkedListCustom<T> list = new LinkedListCustom<T>();
        public void Push(T e) => list.AddFront(e);
        public T Pop()
        {
            return list.RemoveFront();
        }
        public T Top() => list.PeekFront();
        public bool IsEmpty() => list.IsEmpty();
        public void Print() => list.Print();
    }
    #endregion

    #region Queue
    public class QueueCustom<T>
    {
        private LinkedListCustom<T> list = new LinkedListCustom<T>();
        public void Enqueue(T e) => list.AddBack(e);
        public T Dequeue() => list.RemoveFront();
        public bool IsEmpty() => list.IsEmpty();
        public T Front() => list.PeekFront();
        public void Print() => list.Print();
    }
    #endregion

    #region Postfix / Infix
    public static class ExpressionUtils
    {
        static bool IsNumber(string s) => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _);

        public static double EvaluatePostfix(string[] tokens)
        {
            var st = new Stack<double>();
            foreach (var tk in tokens)
            {
                var t = tk.Trim();
                if (t == "") continue;
                if (IsNumber(t)) st.Push(double.Parse(t, CultureInfo.InvariantCulture));
                else if (t == "+" || t == "-" || t == "*" || t == ":" || t == "/" || t == "^")
                {
                    if (st.Count < 2) throw new InvalidOperationException("Недостаточно операндов");
                    var b = st.Pop(); var a = st.Pop();
                    switch (t)
                    {
                        case "+": st.Push(a + b); break;
                        case "-": st.Push(a - b); break;
                        case "*": st.Push(a * b); break;
                        case ":":
                        case "/": st.Push(a / b); break;
                        case "^": st.Push(Math.Pow(a, b)); break;
                    }
                }
                else // функции унарные: ln, cos, sin, sqrt
                {
                    if (st.Count < 1) throw new InvalidOperationException("Недостаточно операндов для функции");
                    var a = st.Pop();
                    switch (t.ToLower())
                    {
                        case "ln": st.Push(Math.Log(a)); break;
                        case "cos": st.Push(Math.Cos(a)); break;
                        case "sin": st.Push(Math.Sin(a)); break;
                        case "sqrt": st.Push(Math.Sqrt(a)); break;
                        default: throw new InvalidOperationException($"Неизвестный токен {t}");
                    }
                }
            }
            if (st.Count != 1) throw new InvalidOperationException("Неверный постфиксный формат");
            return st.Pop();
        }

        // Shunting-yard: перевод инфикса -> постфикс
        private static int Precedence(string op)
        {
            switch (op)
            {
                case "+": case "-": return 1;
                case "*": case ":": case "/": return 2;
                case "^": return 3;
                default: return 0;
            }
        }

        private static bool IsLeftAssoc(string op)
        {
            if (op == "^") return false; // ^ - правый ассоциативный
            return true;
        }

        public static string[] InfixToPostfix(string expr)
        {
            // простой токенайзер: числа, функции/идентификаторы, операторы, скобки
            var tokens = Tokenize(expr);
            var output = new List<string>();
            var ops = new Stack<string>();
            foreach (var tk in tokens)
            {
                if (IsNumber(tk)) output.Add(tk);
                else if (IsFunction(tk)) ops.Push(tk);
                else if (IsOperator(tk))
                {
                    while (ops.Count > 0 && IsOperator(ops.Peek()) &&
                        ((IsLeftAssoc(tk) && Precedence(tk) <= Precedence(ops.Peek())) || (!IsLeftAssoc(tk) && Precedence(tk) < Precedence(ops.Peek()))))
                    {
                        output.Add(ops.Pop());
                    }
                    ops.Push(tk);
                }
                else if (tk == "(") ops.Push(tk);
                else if (tk == ")")
                {
                    while (ops.Count > 0 && ops.Peek() != "(") output.Add(ops.Pop());
                    if (ops.Count == 0) throw new InvalidOperationException("Mismatched parentheses");
                    ops.Pop();
                    if (ops.Count > 0 && IsFunction(ops.Peek())) output.Add(ops.Pop());
                }
                else // идентификатор/функция
                {
                    // если неизвестно — считаем функцией
                    ops.Push(tk);
                }
            }
            while (ops.Count > 0)
            {
                var o = ops.Pop();
                if (o == "(" || o == ")") throw new InvalidOperationException("Mismatched parentheses");
                output.Add(o);
            }
            return output.ToArray();
        }

        private static IEnumerable<string> Tokenize(string s)
        {
            s = s.Replace('\t', ' ');
            int i = 0; var n = s.Length;
            while (i < n)
            {
                if (char.IsWhiteSpace(s[i])) { i++; continue; }
                if (char.IsDigit(s[i]) || (s[i] == '.' && i+1<n && char.IsDigit(s[i+1])))
                {
                    int j = i+1; while (j<n && (char.IsDigit(s[j]) || s[j]=='.' || s[j]=='e' || s[j]=='E' || (s[j]=='-' && j>i+1 && (s[j-1]=='e' || s[j-1]=='E')))) j++;
                    yield return s.Substring(i, j-i);
                    i = j; continue;
                }
                if (char.IsLetter(s[i]))
                {
                    int j = i+1; while (j<n && char.IsLetter(s[j])) j++;
                    yield return s.Substring(i, j-i);
                    i = j; continue;
                }
                // операторы и скобки
                yield return s[i].ToString(); i++;
            }
        }

        private static bool IsFunction(string tok) => new[] { "ln", "cos", "sin", "sqrt" }.Contains(tok.ToLower());
        private static bool IsOperator(string tok) => new[] { "+","-","*",":","/","^" }.Contains(tok);
    }
    #endregion


    class Program
    {
        private static string GetProjectRootDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo directory = new DirectoryInfo(baseDir);
            for (int i = 0; i < 3; i++)
            {
                if (directory.Parent != null)
                    directory = directory.Parent;
            }
            return directory.FullName;
        }

        private static string GetFilePathInProject(string fileName)
        {
            return Path.Combine(GetProjectRootDirectory(), fileName);
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Lab: LinkedList / Stack / Queue / Expressions (C#)\n");

            string inputFile = GetFilePathInProject("input.txt");

            if (File.Exists(inputFile))
            {
                Console.WriteLine($"Найден input.txt в {inputFile}");
                Console.WriteLine("Выполняю команды для стека и для очереди (Custom).\n");
                var content = File.ReadAllText(inputFile);
                RunStackCommands(content);
                Console.WriteLine();
                RunQueueCommands(content);
            }
            else
            {
                Console.WriteLine("input.txt не найден в папке проекта.");
                Console.WriteLine($"Искал по пути: {inputFile}");
            }

            // Демонстрация инфикс -> постфикс и вычисление
            var expr = "3 + 4 * 2 / ( 1 - 5 ) ^ 2 ^ 3";
            Console.WriteLine("\nПример инфиксной записи:\n  " + expr);
            var postfix = ExpressionUtils.InfixToPostfix(expr);
            Console.WriteLine("Постфиксная запись: " + string.Join(" ", postfix));
            try
            {
                var val = ExpressionUtils.EvaluatePostfix(postfix);
                Console.WriteLine("Результат вычисления: " + val);
            }
            catch (Exception ex) { Console.WriteLine("Ошибка вычисления постфикса: " + ex.Message); }

            // Генерация тестовых наборов для замеров
            GenerateSampleInputs();
            Console.WriteLine("\nГенерация sample_input_*.txt и timings.csv завершена.");

            Console.WriteLine("\nГотово. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static void RunStackCommands(string content)
        {
            var tokens = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var stack = new StackCustom<string>();
            var sw = Stopwatch.StartNew();

            foreach (var tk in tokens)
            {
                if (tk.StartsWith("1,"))
                {
                    var val = tk.Substring(2);
                    stack.Push(val);
                    Console.WriteLine($"Push({val})");
                }
                else if (tk == "2")
                {
                    if (!stack.IsEmpty()) { var p = stack.Pop(); Console.WriteLine($"Pop -> {p}"); }
                    else Console.WriteLine("Pop -> Стек пуст");
                }
                else if (tk == "3")
                {
                    if (!stack.IsEmpty()) Console.WriteLine("Top -> " + stack.Top());
                    else Console.WriteLine("Top -> Стек пуст");
                }
                else if (tk == "4") Console.WriteLine("isEmpty -> " + stack.IsEmpty());
                else if (tk == "5") { Console.Write("Print -> "); stack.Print(); }
                else Console.WriteLine("Неизвестная команда: " + tk);
            }

            sw.Stop();
            string csvPath = GetFilePathInProject("timings.csv");
            // Изменено: разделяем название времени и значение на разные столбцы
            File.AppendAllText(csvPath, $"stack,{tokens.Length},Время выполнения (мс),{sw.ElapsedMilliseconds}\n");
            Console.WriteLine($"Время выполнения операций стека: {sw.ElapsedMilliseconds} ms");
        }

        static void RunQueueCommands(string content)
        {
            var tokens = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var queue = new QueueCustom<string>();
            var sw = Stopwatch.StartNew();

            foreach (var tk in tokens)
            {
                if (tk.StartsWith("1,"))
                {
                    var val = tk.Substring(2);
                    queue.Enqueue(val);
                    Console.WriteLine($"Enqueue({val})");
                }
                else if (tk == "2")
                {
                    if (!queue.IsEmpty()) { var p = queue.Dequeue(); Console.WriteLine($"Dequeue -> {p}"); }
                    else Console.WriteLine("Dequeue -> Очередь пуста");
                }
                else if (tk == "3")
                {
                    if (!queue.IsEmpty()) Console.WriteLine("Front -> " + queue.Front());
                    else Console.WriteLine("Front -> Очередь пуста");
                }
                else if (tk == "4") Console.WriteLine("isEmpty -> " + queue.IsEmpty());
                else if (tk == "5") { Console.Write("Print -> "); queue.Print(); }
                else Console.WriteLine("Неизвестная команда: " + tk);
            }

            sw.Stop();
            string csvPath = GetFilePathInProject("timings.csv");
            // Изменено: разделяем название времени и значение на разные столбцы
            File.AppendAllText(csvPath, $"queue_custom,{tokens.Length},Время выполнения (мс),{sw.ElapsedMilliseconds}\n");
            Console.WriteLine($"Время выполнения операций очереди (custom): {sw.ElapsedMilliseconds} ms");

            // Также тест стандартной очереди
            var q2 = new Queue<string>();
            sw.Restart();
            foreach (var tk in tokens)
            {
                if (tk.StartsWith("1,")) q2.Enqueue(tk.Substring(2));
                else if (tk == "2") { if (q2.Count > 0) q2.Dequeue(); }
                else if (tk == "3") { var _ = q2.Count > 0 ? q2.Peek() : null; }
                else if (tk == "4") { var _ = q2.Count == 0; }
                else if (tk == "5") { var _ = string.Join("->", q2.ToArray()); }
            }
            sw.Stop();
            // Изменено: разделяем название времени и значение на разные столбцы
            File.AppendAllText(csvPath, $"queue_dotnet,{tokens.Length},Время выполнения (мс),{sw.ElapsedMilliseconds}\n");
            Console.WriteLine($"Время выполнения операций очереди (System.Queue): {sw.ElapsedMilliseconds} ms");
        }

        static void GenerateSampleInputs()
        {
            string csvPath = GetFilePathInProject("timings.csv");
            if (!File.Exists(csvPath))
                // Изменено: новый заголовок с отдельными колонками для названия времени и значения
                File.WriteAllText(csvPath, "Структура,Количество операций,Метрика времени,Значение (мс)\n");

            var rand = new Random(123);

            // Наборы: маленький, средний, большой
            var small = GenerateCommands(rand, 20);
            var medium = GenerateCommands(rand, 200);
            var large = GenerateCommands(rand, 2000);

            File.WriteAllText(GetFilePathInProject("sample_input_small.txt"), small);
            File.WriteAllText(GetFilePathInProject("sample_input_medium.txt"), medium);
            File.WriteAllText(GetFilePathInProject("sample_input_large.txt"), large);

            // Наборы с разным соотношением операций
            var manyPushes = GenerateCommandsBiased(rand, 2000, pushBias: 0.9);
            var manyPops = GenerateCommandsBiased(rand, 2000, pushBias: 0.1);

            File.WriteAllText(GetFilePathInProject("sample_input_manypushes.txt"), manyPushes);
            File.WriteAllText(GetFilePathInProject("sample_input_manypops.txt"), manyPops);
        }

        static string GenerateCommands(Random rand, int n)
        {
            var ops = new List<string>();
            for (int i = 0; i < n; i++)
            {
                int op = rand.Next(1, 6);
                if (op == 1) ops.Add($"1,{rand.Next(0, 1000)}");
                else ops.Add(op.ToString());
            }
            return string.Join(" ", ops);
        }

        static string GenerateCommandsBiased(Random rand, int n, double pushBias = 0.5)
        {
            var ops = new List<string>();
            for (int i = 0; i < n; i++)
            {
                double r = rand.NextDouble();
                if (r < pushBias) ops.Add($"1,{rand.Next(0, 1000)}");
                else ops.Add("2");
            }
            return string.Join(" ", ops);
        }
    }
}