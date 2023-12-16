using DITwo;

//var builder = WebApplication.CreateBuilder(args);
////var allServices = builder.Services;//встроенные службы
//builder.Services.AddTransient<IDayOfWeek, EngDayOfWeek>();//внедрение собственной службы //моно заменять реализацию создавая новый класс и наследуя его от интерфейса
//var app = builder.Build();

//app.Run(async ctx =>//запуск встроенных служб
//{
//    ctx.Response.Headers.ContentType = "text/html; charset=UTF-8";
//    await ctx.Response.WriteAsync("<h1>Все сервисы</h1>");
//    await ctx.Response.WriteAsync("<table><tr><th>Тип</th><th>Время жизни</th><th>Реализация</th></tr>");
//    foreach (var service in allServices)
//    {
//        await ctx.Response.WriteAsync($"<tr><td>{service.ImplementationType}</td><td>{service.Lifetime}</td><td>{service.ImplementationInstance}</td></tr>");
//    }
//    await ctx.Response.WriteAsync("</table>");
//});
//app.Run();

//var service = app.Services.GetService<IDayOfWeek>();//собственная служба
//app.Run(async ctx =>
//{
//    ctx.Response.Headers.ContentType = "text/html; charset=utf-8";
//    await ctx.Response.WriteAsync(service.GetDayOfWeek());
//});
//app.Run();

//app.UseMiddleware<DayOfWeekMiddleware>();//внедрение через конструктор
//app.Run();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<ISpecialDateGetter, ThirteenFridayGetter>();
var app = builder.Build();
app.UseMiddleware<SpecialDateGetterMiddleware>();
app.Run();
