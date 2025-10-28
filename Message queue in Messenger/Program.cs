using System;
using System.Collections.Generic;
using System.Threading;


public class Message
{
    public string Id { get; }
    public string Sender { get; }
    public string Recipient { get; }
    public string Content { get; }
    public DateTime Timestamp { get; }
    public MessageStatus Status { get; set; }

    public Message(string sender, string recipient, string content)
    {
        Id = Guid.NewGuid().ToString();
        Sender = sender;
        Recipient = recipient;
        Content = content;
        Timestamp = DateTime.Now;
        Status = MessageStatus.Pending;
    }

    public override string ToString()
    {
        return $"[{Timestamp:HH:mm:ss}] {Sender} → {Recipient}: {Content} ({Status})";
    }
}

// Статусы сообщения
public enum MessageStatus
{
    Pending,    // Ожидает доставки
    Delivered,  // Доставлено
    Failed      // Ошибка доставки
}


public class MessageQueueManager
{
    private Queue<Message> _messageQueue;
    private object _lockObject;
    private bool _isProcessing;

    public event Action<Message> OnMessageDelivered;
    public event Action<Message> OnMessageFailed;

    public MessageQueueManager()
    {
        _messageQueue = new Queue<Message>();
        _lockObject = new object();
        _isProcessing = false;
    }


    public void SendMessage(string sender, string recipient, string content)
    {
        var message = new Message(sender, recipient, content);

        lock (_lockObject)
        {
            _messageQueue.Enqueue(message);
            Console.WriteLine($"✉️ Сообщение добавлено в очередь: {message}");
        }


        if (!_isProcessing)
        {
            StartProcessing();
        }
    }

    
    private void StartProcessing()
    {
        _isProcessing = true;
        ThreadPool.QueueUserWorkItem(_ => ProcessQueue());
    }

  
    private void ProcessQueue()
    {
        while (true)
        {
            Message message = null;

            lock (_lockObject)
            {
                if (_messageQueue.Count > 0)
                {
                    message = _messageQueue.Dequeue();
                }
                else
                {
                    _isProcessing = false;
                    break; 
                }
            }

            if (message != null)
            {
                DeliverMessage(message);
            }

          
            Thread.Sleep(1000);
        }
    }

    private void DeliverMessage(Message message)
    {
        Console.WriteLine($"🔄 Доставляем сообщение: {message.Content}");

       
        var random = new Random();
        var success = random.Next(0, 10) > 2; 

        if (success)
        {
            message.Status = MessageStatus.Delivered;
            Console.WriteLine($"✅ Сообщение доставлено: {message}");
            OnMessageDelivered?.Invoke(message);
        }
        else
        {
            message.Status = MessageStatus.Failed;
            Console.WriteLine($"❌ Ошибка доставки: {message}");
            OnMessageFailed?.Invoke(message);
        }
    }

  
    public void PrintQueueStatus()
    {
        lock (_lockObject)
        {
            Console.WriteLine($"\n📊 Статус очереди: {_messageQueue.Count} сообщений в ожидании");
            if (_messageQueue.Count > 0)
            {
                Console.WriteLine("Сообщения в очереди:");
                foreach (var msg in _messageQueue)
                {
                    Console.WriteLine($"  - {msg}");
                }
            }
            Console.WriteLine();
        }
    }
}


public class MessageQueueDemo
{
    public static void Main()
    {
        Console.WriteLine("=== ДЕМОНСТРАЦИЯ ОЧЕРЕДИ СООБЩЕНИЙ ===\n");

        var messageQueue = new MessageQueueManager();

        
        messageQueue.OnMessageDelivered += msg =>
            Console.WriteLine($"🎉 Уведомление: Сообщение для {msg.Recipient} доставлено!");

        messageQueue.OnMessageFailed += msg =>
            Console.WriteLine($"⚠️ Уведомление: Не удалось доставить сообщение для {msg.Recipient}!");

     
        Console.WriteLine("Отправка сообщений...\n");

        messageQueue.SendMessage("Alice", "Bob", "Привет! Как дела?");
        messageQueue.SendMessage("Bob", "Alice", "Отлично! Спасибо!");
        messageQueue.SendMessage("Charlie", "Alice", "Встречаемся в 15:00");
        messageQueue.SendMessage("Alice", "Charlie", "Хорошо, буду вовремя!");
        messageQueue.SendMessage("Bob", "Charlie", "Отправь мне документы");

        Thread.Sleep(2000); 

        messageQueue.SendMessage("David", "Alice", "Новое предложение по проекту");
        messageQueue.SendMessage("Alice", "David", "Интересно, расскажи подробнее");

       
        messageQueue.PrintQueueStatus();

       
        Console.WriteLine("Ожидание завершения обработки сообщений...");
        Thread.Sleep(10000);

        messageQueue.PrintQueueStatus();
    }
}