# Введение в ASP .Net

Изучить материалы о ПО промежуточного слоя (middleware)
https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0

Рассмотрите работу методов `Use`, `Run` и `Map`.

## Внедрение простого middleware

Простым способом внедрения middleware является использование методов `Run` и `Use`. Исследуем работу данных методов на примерах.

Создайте пустой проект Asp .Net. Проверьте работоспособность, выполнив запуск приложения.

Измените код программы следующим образом:

```cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async context =>
{
    int y = 10;
    y++;
    await context.Response.WriteAsync($"y = {y}");
});

app.Run();

```

Запустите проект, проверьте результат. Изменится ли результат, если обновить страницу?

Добавим еще одну переменную:
```cs
            int x = 20;

            app.Run(async context =>
            {
                int y = 10;
                x++;
                y++;
                await context.Response.WriteAsync($"x = {x}; y = {y}");
            });

```

Запустите и проверьте результат. Изменится ли результат, если обновить страницу? Попробуйте обратиться к странице из другого браузера. Каким будет результат при первом запросе?
> Примечание
> Счетчик может изменяться на 2, поскольку браузер может производить сразу два запроса, один из которых к favicon (иконке)



Теперь попробуем вызвать несколько middleware:
```cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

int x = 20;

app.Use(async (context, next) =>
{
    x = 1000;
    await next.Invoke();
    await context.Response.WriteAsync($"app.Use: x = {x}\n");
});

app.Run(async context =>
{
    int y = 10;
    x++;
    y++;
    await context.Response.WriteAsync($"first app.Run: x = {x}; y = {y}\n");
});

app.Run(async context =>
{
    await context.Response.WriteAsync("Second app.Run");
});

app.Run();
```

Запустите и проверьте результат. Определите порядок выполнения операций. Почему мы не видим строку `"Second app.Run"`?

Данный проект можно не сохранять.

## Внедрение middleware с помощью UseMiddleware&lt;T>

Рассмотрим создание собственного компонента. Чтобы внедрить свой компонент, необходимо сперва создать свой класс, который будет удовлетворять двум условиям:
- класс должен иметь конструктор, принимающий в качестве параметра `RequestDelegate`;
- класс должен иметь метод `Invoke`/`InvokeAsync`, который принимает в качестве параметра `HttpContext`

Разработаем такой класс. Пусть наш компонент показывает текущее время на серверной машине.

Сперва следует создать в проекте новый класс `TimeMiddleware` (отд. файл TimeMiddleware.cs).

Далее, добавим в класс конструктор:
```cs
    public class TimeMiddleware
    {
        private RequestDelegate next;

        public TimeMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
    }
```

И метод `InvokeAsync`:
```cs
public class TimeMiddleware
    {
        private RequestDelegate next;

        public TimeMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
        	await next.Invoke(context);
            await context.Response.WriteAsync(DateTime.Now.ToShortTimeString());
        }

    }
```

Далее, мы можем легко внедрить наш middleware внутри метода `Configure`:
```cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<TimeMiddleware>();
app.Run();
```

Проверьте работоспособность.

### RandomMiddleware

Используйте для создания middleware следующее:
![](rnd1.png)
![](rnd2.png)

Реализуйте внутри метода `invoke` вывод случайного числа.

Сгенерированный файл содержит строки:
```cs
	public static class RandomMiddlewareExtensions
    {
        public static IApplicationBuilder UseRandomMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RandomMiddleware>();
        }
    }
```

Каково назначение данных строк? Используйте для внедрения middleware метод расширения.


## UseRouting

Создадим приложение. Изменим его код:
```cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Hello World!");
    });

    endpoints.MapGet("/connection", async context =>
    {
        await context.Response.WriteAsync("Remote ip: " +
            context.Connection.RemoteIpAddress.ToString());
        await context.Response.WriteAsync("\nLocal ip: " +
            context.Connection.LocalIpAddress.ToString());
// или context.Connection.LocalIpAddress.MapToIPv4().ToString(), чтобы явно указать ipv4 адрес
    });
});

app.Run();
```

Запустите программу, обратитесь по адресу `<АДРЕС:ПОРТ>/connection`. В результате вы увидите информацию об ip-адресах. Примечание: возможен вывод адреса localhost (127.0.0.1) в формате ipv6. Для ipv6 "петля" записывается как :::1

Добавим удобный переход со стартовой страницы:
```cs
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("<a href='/connection'>connection info</a>");
                });
```

Выведем информацию о запросе, добавив еще одну точку:
```cs
                endpoints.MapGet("/request", async context =>
                {
                    StringBuilder result = new StringBuilder();
                    result.AppendLine($"Path: {context.Request.Path.Value}");
                    result.AppendLine($"Method: {context.Request.Method}");
                    result.AppendLine($"Status Code: {context.Response.StatusCode}");
                    result.AppendLine("Headers:");

                    foreach (var header in context.Request.Headers)
                    {
                        result.AppendLine($"{header.Key} : {header.Value}");
                    }

                    await context.Response.WriteAsync(result.ToString());

                });
```

### Строка запроса

Добавим еще одну точку:
```cs
                endpoints.MapGet("/params", async context =>
                {
                    await context.Response.WriteAsync(
                        $"Query String: {context.Request.QueryString.Value}\n");

                    foreach (var param in context.Request.Query)
                    {
                        await context.Response.WriteAsync(
                            $"{param.Key} = {param.Value}\n");
                    }
                });
```

Загрузим страницу:
![](params1.png)

Как видно, никаких параметров нет, поэтому страница пустая. Изменим строку запроса, добавив `?id=123&value=secret`

![](params2.png)

Параметры могут быть использованы при выполнении запроса. Добавьте еще одну точку `/square`:
```cs
endpoints.MapGet("/square", async context =>
                {
                    int value = 0;

                    if (context.Request.Query.ContainsKey("value"))
                    {
                        if (int.TryParse(context.Request.Query["value"], out value))
                        {
                            await context.Response
                                .WriteAsync($"{value}^2 = {value * value}");
                        }
                        else
                        {
                            await context.Response.WriteAsync($"Invalid value");
                        }
                    }
                    else
                    {
                        await context.Response.WriteAsync($"Invalid request");
                    }


                });
```

Проверьте работоспособность, протестировав все возможные исходы.

## Самостоятельная работа: возведение в степень

Добавьте в последний проект еще одну точку `/power`. Предполагается, что через строку запроса передается два параметра `a` и `b`. На странице выводится результат возведения числа `a` в степень `b`, или иное сообщение, если операция невозможна.

## Самостоятельная работа: HelloWorldMiddleware

Создайте новый проект.
Разработайте отдельный компонент HelloWorldMiddleware. Данный компонент работает следующим образом:
- из строки запроса берется параметр `name` (пример: ?name=vasya);
- выводится сообщение "Hello, {name}!", где {name} - переданное имя;
- если имя не задано, то выводится сообщение "Hello, World!"

Внедрите данный компонент в новый проект.

# Статические файлы

Добавим `Middleware`, отвечающий за обработку статических файлов. Общая идея такова:
- в проекте добавляется каталог `wwwroot`, в котором содержаться файлы;
- если, например url будет `https://oursite.ru/index.html`, то в папке `wwwroot` будет попытка найти файл `index.html`, для случая `https://oursite.ru/images/logo.png` будет попытка найти файл `images/logo.png` и т. п. Общая идея - путь до ресурса будет просканивован в каталоге `wwwroot`, если там будет найден файл, то его содержимое будет отдано клиенту;
- если файл не был найден, то работа конвейера будет продолжена;

## Создание приложения

Создадим пустой проект Asp .Net.

В нем создадим папку `wwwroot`. Признак того, что все успешно - для данного каталога будет задан специальный значок (это обычный каталог, но в asp он имеет особое назначение).

![](static1.png)



