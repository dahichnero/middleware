# Внедрение зависимостей

**Внедрение зависимостей** подразумевает, что поля объекта должны быть настроены некоторой внешней сущностью. Рассмотрим пример.

Пусть есть класс для отправки сообщения:
```cs
    class EmailSender
    {
        private string senderAddress; // адрес отправителя

        public EmailSender(string senderAddress)
        {
            this.senderAddress = senderAddress;
        }

        public void Send(string message, string address)
        {
            Console.WriteLine($"|{senderAddress}| Сообщение <{message}> oтправлено по почте <{address}>");
        }
    }
```

И класс для генерации некоторого секретного кода:
```cs
    class CodeGenerator
    {
        private EmailSender sender = new EmailSender("main@test.ru");

        public CodeGenerator() { }

        public void GenerateCodeAndSend(string address)
        {
            int code = new Random().Next(10000);
            Console.WriteLine($"Сгенерирован код: {code}");
            sender.Send($"Ваш код: {code}", address);
        }
    }
```

```cs
// main
new CodeGenerator().GenerateCodeAndSend("test@test.ru");
```

В данном примере, можно сказать, что `CodeGenerator` зависит от `EmailSender`. Однако, данную **зависимость** класс разрешает самостоятельно:
```cs
private EmailSender sender = new EmailSender("main@test.ru"); // самостоятельная настройка
```

С этим могут быть связаны проблемы:
- невозможно задать свойства "внутреннего" объекта для `CodeGenerator`, то есть задать адрес отправителя;
- невозможно изменить реализацию отправителя, поскольку она жестко "зашита" в код;

Как следствие, код в дальнейшем будет сложно тестировать и поддерживать.

Первую проблему можно разрешить, если использовать конструктор, метод или свойство. Зачастую, для таких целей используют конструктор:
```cs
    class CodeGenerator
    {
        private EmailSender sender;

        public CodeGenerator(string senderAddress)
        {
            sender = new EmailSender(senderAddress);
        }

        public void GenerateCodeAndSend(string address)
        {
            int code = new Random().Next(10000);
            Console.WriteLine($"Сгенерирован код: {code}");
            sender.Send($"Ваш код: {code}", address);
        }
    }
```

Или, мы можем внедрить уже готовый класс:
```cs
    class CodeGenerator
    {
        private EmailSender sender;

        public CodeGenerator(EmailSender sender) 
        {
            this.sender = sender;
        }

        public void GenerateCodeAndSend(string address)
        {
            int code = new Random().Next(10000);
            Console.WriteLine($"Сгенерирован код: {code}");
            sender.Send($"Ваш код: {code}", address);
        }
    }
```
```cs
// main
new CodeGenerator(new EmailSender("main@test.ru")).GenerateCodeAndSend("test@test.ru");
```

В последнем примере мы произвели **внедрение зависимости**. Один из вариантов, который чаще всего используется - внедрение через конструктор. Однако, похожим образом можно внедрить зависимость через метод или свойство.

Вторая проблема - использование конкретной реализации. Чтобы решить данную проблему, классы должны зависеть от абстракции. Попросту, мы должны использовать интерфейс:
```cs
    interface ISender
    {
        void Send(string message, string address);
    }

    class EmailSender: ISender
    {
        private string senderAddress;

        public EmailSender(string senderAddress)
        {
            this.senderAddress = senderAddress;
        }

        public void Send(string message, string address)
        {
            Console.WriteLine($"|{senderAddress}| Сообщение <{message}> oтправлено по почте <{address}>");
        }
    }
```
```cs
    class CodeGenerator
    {
        private ISender sender; // мы не привязаны к конкретной реализации ISender

        public CodeGenerator(ISender sender) 
        {
            this.sender = sender;
        }

        public void GenerateCodeAndSend(string address)
        {
            int code = new Random().Next(10000);
            Console.WriteLine($"Сгенерирован код: {code}");
            sender.Send($"Ваш код: {code}", address);
        }
    }
```
```cs
       static void Main(string[] args)
        {
		// реализация ISender легко может быть изменена
            var generator1 = new CodeGenerator(new EmailSender("main@test.ru"));
            var generator2 = new CodeGenerator(new SmsSender("+79009009000"));
        }
```

Обычно, **зависимости внедряют через интерфейс или с использованием абстрактного класса**.

## Контейнер внедрения зависимостей

В сложных проектах, удобно использовать специальные **контейнеры внедрения зависимостей (DI Container)**. В задачи таких контейнеров обычно входит **управление зависимостями** и управление **временем жизни объектов**.

Существуют различные реализации DI контейнеров, которые легко могут быть получены с помощью менеджера пакетов `NuGet`. Мы будем использовать стандартную реализацию от Майкрософт.

Пусть есть классы из прошлого примера:
```cs
    interface ISender
    {
        void Send(string message, string address);
    }


    class EmailSender: ISender
    {
        private string senderAddress;

        public EmailSender(string senderAddress)
        {
            this.senderAddress = senderAddress;
        }

        public void Send(string message, string address)
        {
            Console.WriteLine($"|{senderAddress}| Сообщение <{message}> oтправлено по почте <{address}>");
        }
    }

    class CodeGenerator
    {
        private ISender sender;

        public CodeGenerator(ISender sender) 
        {
            this.sender = sender;
        }

        public void GenerateCodeAndSend(string address)
        {
            int code = new Random().Next(10000);
            Console.WriteLine($"Сгенерирован код: {code}");
            sender.Send($"Ваш код: {code}", address);
        }
    }
```

Добавим сборки (исп. NuGet):
```cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
```

Основной код:
```cs
var builder = Host.CreateDefaultBuilder();
builder.ConfigureServices((_, services) =>
{
	services.AddScoped<CodeGenerator>();
	services.AddScoped<ISender, EmailSender>(sender => new EmailSender("main@addr.ru"));
});

var host = builder.Build();
host.Services.GetService<CodeGenerator>().GenerateCodeAndSend("test@example.com");
```

Здесь, сперва создаем `IHostBulder builder`. Далее, с помощью метода `AddScoped` происходит внедрение зависимых служб (Services). Во втором случае мы указываем явный способ создания экземпляра `EmailSender`. Обычно, классы разрабатываются таким образом, чтобы в этом не было необходимости, однако наш пример является упрощенным.

Далее, вызываем `Build()` чтобы получить экземпляр `IHost`. Ниже используем свойство `Services` в сочетании с `GetService`, чтобы получить ссылку на `CodeGenerator` и вызвать `GenerateCodeAndSend`.

Обратите внимание, что внедрение `ISender` в `CodeGenerator` произошло автоматически.

## Самостоятельная работа

Метод `AddScoped` - это не единственный способ внедрения зависимостей. Помимо него, существуют методы `AddTransient` и `AddScoped`. Данные методы определяют **время жизни созданных объектов**.

Выполните следующее:
- выполните руководство из официальной документации
https://docs.microsoft.com/ru-ru/dotnet/core/extensions/dependency-injection-usage
- ответьте на вопрос "Какое время жизни объетов задает каждый из методов `AddScoped`, `AddTransient` и `AddScoped`?";


## Подробная информация

Данный вопрос является достаточно сложным. Вы можете более детально изучить контейнеры внедрения зависимостей:
- майрософт: https://docs.microsoft.com/ru-ru/dotnet/core/extensions/dependency-injection
в частности, стоит дополнительно посмотреть **"Несколько правил обнаружения конструктора"** и "Время существования служб" 
- (незаконченный?) цикл статей на хабре. Статья https://habr.com/ru/post/350068/ и https://habr.com/ru/post/350708/ (неплохо написано, но примеры на Java).

