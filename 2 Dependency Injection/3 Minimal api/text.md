# Минималистичный Api

Разработаем minimal api с помощью Asp .Net Core и Entity Framework Core.

Minimal Api - подход в Asp .Net Core, при котором мы используем методы `MapXXX` и `Results Api`. Общая идея - быстрое создание простого Api, где не требуется какая-либо сложная логика.

Для создания minimal api мы используем **пустой шаблон Asp .Net Core**.


## 1 Подключение EF Core

Установите три пакета:
- `Microsoft.EntityFrameworkCore`;
- `Microsoft.EntityFrameworkCore.Tools`;
- `Microsoft.EntityFrameworkCore.Sqlite`;

Создайте в проекте папку `Models`, в нее добавьте класс `City`:
```cs
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Population { get; set; }
    }
```

Также, добавьте класс `CitiesContext`:
```cs
public class CitiesContext : DbContext
{
    public CitiesContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<City> Cities { get; set; }

}
```

В контейнер `Services` добавьте `DbContext` с помощью метода `AddDbContext`:
```cs
builder.Services.AddDbContext<CitiesContext>(opt => opt.UseSqlite("data source = cities.db"));
```

После чего выполните команду в консоли диспетчера пакетов NuGet для создания миграции:
```
Add-Migration "Initial"
```

И обновите базу данных:
```
Update-Database
```

Если все было сделано правильно, то появится файл `cities.db`:
![](1.png)

Укажите для данного файла "Копировать более позднюю версию":
![](2.png)

## 2 Добавление записи и вывод записей

Реализуем метод для вывода записей. Для этого используем метод `MapGet`. Данный метод принимает первым параметром шаблон пути, а вторым параметром метод, который возвращает некоторое значение.

Попробуем сперва несколько вариантов вызова, чтобы понять принцип.

Сперва попробуем написать
```cs
app.MapGet("/hello", "Hello, World");
```
И обратимся по адресу `/hello`, чтобы увидеть вывод сообщения "Hello, World". Второй параметр - функция возвращаюшая `string`, все отобразится один в один.

Далее, напишем:
```cs
app.MapGet("/square", (int x) => x * x);
```

И обратимся по `/square?x=5`. Результатом будет `25`.

Изменим код:
```cs
app.MapGet("/square/{x}", (int x) => x * x);
```

Фигурные скобки в шаблоне позволяют задать месторасположение параметра. Так, теперь для получения квадрата 8 нужно будет написать `/square/8`.

Следующий шаг - попробуем объект:
```cs
app.MapGet("/ekaterinburg", () => new City { Country = "Россия",
    Name = "Екатеринбург",
    Population = 1539371,
    Id = 1
});
```

Обратимся к `/ekaterinburg`. Результатом будет вывод в формате JSON. Asp .Net автоматически выполнил сериализацию объекта.

И напоследок попробуем `Results`. Мы будем его использовать для изменения кодов ответа. Напишем:
```cs
app.MapGet("/moscow", () => Results.NotFound());
```

При попытке обращения вы увидите код ответа `404`.

Перейдем, наконец к коду вывода всех записей с помощью `EntityFramework`. Все предыдущие `MapGet` нам не нужны, и их можно удалить.

Для вывода мы просто используем:
```cs
app.MapGet("/cities", (CitiesContext context) => context.Cities.ToList());
```

Или, заменив на асинхронный вызов, получим:
```cs
app.MapGet("/cities", async (CitiesContext context) =>
    await context.Cities.ToListAsync());
```

Все готово! Однако база данных пустая, и мы ничего не увидим. Реализуем сразу же добавление города, вызвав метод `MapPost`:
```cs
app.MapPost("/city", async (CitiesContext context, City city) =>
{
    context.Add(city);
    await context.SaveChangesAsync();
});
```

Для простоты, мы опустим здесь валидацию данных и отлавливание исключений с помощью оператора `try`.

## 3 Тестирование методов добавления и вывода

Все бы ничего, но для отправки POST запроса к `/city` нам нужен какой-то инструмент. Используем **Postman Desktop**.

Нажмем в основном окне Postman на кнопку создания коллекции:
![](3.png)

Переименуем коллекцию (ПКМ, Rename). Если вы не выполняли вход в систему, то стоит указать в названии коллекции свою фамилию, чтобы не перепутать ее с другими подобными коллекциями:
![](4.pNg)

Далее, создадим запрос (`Add a request`) и дадим ему название `Добавить Екатиринбург`. Изменим тип запроса на POST и укажем адрес сервера (он должен быть запущен!):
![](5.png)

Перейдем во вкладку Body, выберем там Raw и зададим формат JSON:
![](6.pNg)

Пропишем объект:
```json
{
    "name" : "Екатеринбург",
    "country" : "Россия",
    "population" : "1539371"
}
```

И отправим его нажатием Send:
![](7.png)

Получим ответ `200 OK`
![](8.png)

Сохраним запрос нажатием на кнопку Save.

Далее, создадим еще один запрос для получения всех записей:
1. укажем метод GET;
2. зададим URL запроса;
3. выполним отправку;
4. сохраним запрос.

![](9.png)

С помощью возможности дублирования запросов (ПКМ->Dublicate) создайте еще два запроса для добавления других городов. Проверьте, что все работает.

## 4 Получение города по Id

Реализуем получение города по Id:
```cs
app.MapGet("/city/{id}", async (CitiesContext context, int id) =>
    await context.Cities.FindAsync(id));
```

Проверьте работоспособность с помощью POSTMAN. Используйте как существующий, так и не сущесвтующий Id. Какими будут ответы?

Перепишем код с использованием `Results`:
```cs
app.MapGet("/city/{id}", async (CitiesContext context, int id) =>
{
    var found = await context.Cities.FindAsync(id);
    if (found is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(found);
});
```
Что изменилось в ответе, когда id города не был найден?

## 5 Изменение города

Создайте метод для изменения города:
```cs
app.MapPut("/city/{id}", async (CitiesContext context, City city, int id) =>
{
    City? foundCity = await context.Cities.FindAsync(id);
    if (foundCity is null)
    {
        return Results.NotFound();
    }
    foundCity.Name = city.Name;
    foundCity.Population = city.Population;
    await context.SaveChangesAsync();
    return Results.Ok(foundCity);
});
```

Проверьте его работоспособность в Postman.  Не забудьте изменить тип запроса на PUT и передать телом запроса JSON-объект.

## 6 Удаление города

Реализуйте самостоятельно:
```cs
app.MapDelete("/city/{id}", ???)
```

Метод получает Id города и удаляет его. Если город не найден, то будет возвращен код `404`. После успешного удаления используйте `Results.NoContent()`, чтобы отдать код `204` (успешно, но нет контента). 

Проверьте работоспособность с помощью Postman.

## Самостоятельная работа

Добавьте в api точку:
```
GET /cities/{country}
```
Которая бы позволила получить список всех городов из определенной страны.


## 7 Валидация данных

Реализуем проверку данных с помощью популярной библиотеки `FluentValidation`. Для начала вам нужно будет установить ее с помощью Nuget:
![](10.png)

Далее, создайте в проекте папку `Validators` и добавьте в нее класс `CityValidator`:
```cs
public class CityValidator : AbstractValidator<City>
{

}
```

Опишем правила для полей `City` с помощью вызова `RuleFor` внутри конструктора (доп. сведения смотрите в документации к FluentValidation):
```cs
public class CityValidator : AbstractValidator<City>
{
    public CityValidator()
    {
        RuleFor(city => city.Name).MaximumLength(100);
        RuleFor(city => city.Name).NotEmpty();
        RuleFor(city => city.Population).ExclusiveBetween(1, 1000000000);
    }
}
```

Добавим валидатор в контейнер:
```cs
builder.Services.AddScoped<AbstractValidator<City>, CityValidator>();
```

Внедрим зависимость и выполним проверку:
```cs
app.MapPost("/city", async (
        CitiesContext context, 
        AbstractValidator<City> validator, 
        City city) =>
{
    ValidationResult results = validator.Validate(city);
    if (!results.IsValid)
    {
        return Results.ValidationProblem(results.ToDictionary());
    }
    context.Add(city);
    await context.SaveChangesAsync();
    return Results.Ok(city);
});
```

Добавьте аналогичным образом проверку в метод изменения города.

Проверьте работоспособность с помощью Postman.