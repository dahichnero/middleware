# Внедрение зависимостей 2


## 1 Службы по умолчанию

Asp .Net использует для своей работы контейнер внедрения зависимостей. Мы можем:
- внедрить необходимые в проекте зависимости;
- задавать время жизни создаваемым объектам (`Transient`, `Scoped`, `Singleton`);
- использовать внедренные службы там, где нам это необходимо;

Для начала, получим сервисы, внедренные по умолчанию в проект Asp. Создайте пустой проект Asp Net. Выполните код:
```cs
var builder = WebApplication.CreateBuilder(args);
var allServices = builder.Services;
var app = builder.Build();

app.Run(async ctx => {
    ctx.Response.Headers.ContentType = "text/html; charset=UTF-8";
    await ctx.Response.WriteAsync("<h1>Все сервисы</h1>");
    await ctx.Response.WriteAsync("<table><tr><th>Тип</th><th>Время жизни</th><th>Реализация</th></tr>");
    foreach (var service in allServices)
    {
        await ctx.Response.WriteAsync($"<tr><td>{service.ImplementationType}</td><td>{service.Lifetime}</td><td>{service.ImplementationInstance}</td></tr>");
    }
    await ctx.Response.WriteAsync("</table>");
});

app.Run();
```

В виде списка будут выведены все внедренные службы. Как видно, этот список далеко не пуст. Каждая из служб может быть использована в приложении.

## 2 Внедрение собственной службы

В проектах мы можем использовать один из нескольких вариантов внедрения дополнительных зависимостей:
- внедрение собственной службы;
- использование встроенных служб;
- использование сторонних служб (подкачка дополнительных пакетов с `NuGet`).

Создадим собственную службу и внедрим ее. Для этого определим: а) интерфейс; б) реализацию интерфейса в виде класса. 

Пусть наша служба выдает текущий день недели. Интерфейс будет иметь вид:
```cs
interface IDayOfWeek
{
    string GetDayOfWeek();
}
```

Реализация может иметь вид:
```cs
class RusDayOfWeek : IDayOfWeek
{
    public string GetDayOfWeek()
    {
        string[] names = new string[]
        {
            "воскресенье", "понедельник", "вторник",
            "среда", "четверг", "пятница", "суббота"
        };
        var dayIndex = (int)DateTime.Today.DayOfWeek;
        return names[dayIndex];
    }
}
```

Внедрим службу с помощью свойства `Services`:
```cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IDayOfWeek, RusDayOfWeek>(); // внедрение службы
var app = builder.Build();

var service = app.Services.GetService<IDayOfWeek>(); // получение службы
app.Run(async ctx => {
    ctx.Response.Headers.ContentType = "text/html; charset=utf-8";
    await ctx.Response.WriteAsync(service.GetDayOfWeek());
});
app.Run();
```

В целом, ничего сложного. Внедрить или получить службу мы можем с помощью свойства `Services`. Однако, получение зависимости через `Services` является не рекомендуемым подходом. Рекомендуется внедрять зависимости через конструкторы классов.

### Внедрение через конструктор

Продемонстрируем автоматическое внедрение службы через конструктор другого класса. Для этого создадим middleware:
```cs
class DayOfWeekMiddleware
{
    private RequestDelegate next;
    private IDayOfWeek dayOfWeekService;

    // обратите внимание на параметр типа IDayOfWeek:
    // его разрешение контейнер зависимостей произведет автоматически
    public DayOfWeekMiddleware(RequestDelegate next, IDayOfWeek dayOfWeekService)
    {
        this.next = next;
        this.dayOfWeekService = dayOfWeekService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.ContentType = "text/html; charset=utf8";
        await context.Response.WriteAsync(dayOfWeekService.GetDayOfWeek());
    }

}
```

Важно, что теперь нет необходимости брать службу вручную с помощью `app.Services.GetService<T>`, внедрение произойдет автоматически. Код `program.cs`
```cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IDayOfWeek, RusDayOfWeek>(); // внедрение службы
var app = builder.Build();
// не нужно получать сервис вручную, 
// внедрение через конструктор DayOfWeekMiddleware
// произойдет автоматически
app.UseMiddleware<DayOfWeekMiddleware>();
app.Run();
```

Контейнер внедрения зависимостей самостоятельно может разрешить параметр `IDayOfWeek`, подставить туда нужную реализацию и проследить, чтобы время жизни объекта соответствовало заданному нами значению (`Transient`).

### Замена реализации

Внедрение зависимостей через интерфейс позволяет достаточно легко заменить реализацию. Пусть, теперь нам нужно выводить завтрашний день недели, день недели на 1 мая или день недели на другом языке. Мы с легкостью можем внедрить другую реализацию, не изменив при этом логики остального приложения. Например:
```cs
// новая реализация
class EngDayOfWeek : IDayOfWeek
{
    public string GetDayOfWeek() => 
        Enum.GetName<DayOfWeek>(DateTime.Today.DayOfWeek);
}
```

```cs
// основной код
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IDayOfWeek, EngDayOfWeek>(); // изменилось только название класса
var app = builder.Build();
app.UseMiddleware<DayOfWeekMiddleware>();
app.Run();
```

## 3 Пятница 13

Выполните следующее:
- разработайте службу;
- добавьте ее в контейнер зависимостей через интерфейс;
- создайте `middleware`, внедрите службу через конструктор;
- используйте `HttpContext` и `WriteAsync` для вывода результата;

##### Интерфейс
Интерфейс будет следующим:
```cs
interface ISpecialDateGetter
{
	string GetSpecialDate();
}
```
##### Класс

Опишите класс `ThirteenthFridayGetter` который бы реализовывал интерфейс `ISpecialDateGetter`. Метод `GetSpecialDate` должен находить и возвращать ближайшую к текущей дате пятницу 13. Используйте для решения задачи встренный тип `DateTime`.

##### Внедрение зависимости
Примерный вид:
```cs
builder.Services.AddTransient<ISpecialDateGetter, ThirteenthFridayGetter>();
```

## 4 Какое сейчас августа?

Добавьте еще одну реализацию `ISpecialDateGetter` с названием `AugustDayGetter`. Метод `GetSpecialDate` должен возвращать дату `N августа`, где вместо `N` подставляется число. Считаем, что при этом: август не заканчивается 31 августа; 1 августа наступает в тот день, когда должно было бы наступить в реальности (после 31 июля).

1 сентября будет `32 августа`, 2 сентября будет `33 августа` и т. д.;


