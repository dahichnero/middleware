using DITwo;

//var builder = WebApplication.CreateBuilder(args);
////var allServices = builder.Services;//���������� ������
//builder.Services.AddTransient<IDayOfWeek, EngDayOfWeek>();//��������� ����������� ������ //���� �������� ���������� �������� ����� ����� � �������� ��� �� ����������
//var app = builder.Build();

//app.Run(async ctx =>//������ ���������� �����
//{
//    ctx.Response.Headers.ContentType = "text/html; charset=UTF-8";
//    await ctx.Response.WriteAsync("<h1>��� �������</h1>");
//    await ctx.Response.WriteAsync("<table><tr><th>���</th><th>����� �����</th><th>����������</th></tr>");
//    foreach (var service in allServices)
//    {
//        await ctx.Response.WriteAsync($"<tr><td>{service.ImplementationType}</td><td>{service.Lifetime}</td><td>{service.ImplementationInstance}</td></tr>");
//    }
//    await ctx.Response.WriteAsync("</table>");
//});
//app.Run();

//var service = app.Services.GetService<IDayOfWeek>();//����������� ������
//app.Run(async ctx =>
//{
//    ctx.Response.Headers.ContentType = "text/html; charset=utf-8";
//    await ctx.Response.WriteAsync(service.GetDayOfWeek());
//});
//app.Run();

//app.UseMiddleware<DayOfWeekMiddleware>();//��������� ����� �����������
//app.Run();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<ISpecialDateGetter, ThirteenFridayGetter>();
var app = builder.Build();
app.UseMiddleware<SpecialDateGetterMiddleware>();
app.Run();
