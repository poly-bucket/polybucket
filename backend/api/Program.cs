using Api;

//var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactFrontend",
//        policy =>
//        {
//            policy.AllowAnyOrigin()
//                  .AllowAnyHeader()
//                  .AllowAnyMethod();
//        });
//});

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseCors("AllowReactFrontend");
//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

public class Program
{
    public static IHostBuilder CreateWebHostBuilder(string[] args) => Host
        .CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder => webBuilder
            .UseStartup<Startup>());

    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        await CreateWebHostBuilder(args).Build().RunAsync();
    }
}