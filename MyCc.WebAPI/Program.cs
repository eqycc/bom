using MyCc.WebAPI.Extensions;
using MyCc.WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// 加载配置文件
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 配置数据库服务
builder.Services.ConfigureServicesDatabase(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation(); // 启用Swagger
builder.Logging.ClearProviders(); // 清除默认的日志提供程序
builder.Logging.AddConsole(); // 添加控制台日志提供程序

// 注册应用服务和仓储
builder.Services.AddApiServices(builder.Configuration);

// 添加CORS服务
builder.Services.AddCors(options =>
{
    // ReSharper disable once VariableHidesOuterVariable
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin() // 允许所有来源
            .AllowAnyMethod() // 允许所有HTTP方法
            .AllowAnyHeader(); // 允许所有请求头
        // .AllowCredentials(); // 允许携带凭据
        // .DisallowCredentials() // 不允许携带凭据
    });
});

// 添加OpenIddict服务
builder.Services.AddMyCcOpenIddictServer();

builder.Services.AddJwtAuthentication(builder.Configuration); // 添加JWT认证
builder.Services.AddAuthorization(); // 添加授权服务
var app = builder.Build();

// 调用扩展方法以初始化应用程序
await app.InitializeApplicationAsync();

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();// 启用Swagger
    app.UseSwaggerDocumentation(); // 启用Swagger
}

//所有的未处理异常都会被中间件捕获并记录日志，并返回统一的错误响应
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseDefaultFiles(); // 使用默认文件中间件
app.UseStaticFiles(); // 使用静态文件中间件

app.UseAuthentication(); // 启用身份验证中间件
app.UseAuthorization(); // 启用授权中间件
app.MapControllers(); // 启用控制器
app.UseRouting(); // 启用路由
app.UseCors("AllowAll"); // 启用跨域
app.UseHttpsRedirection(); // 启用HTTPS重定向
app.Run(); // 启动应用程序