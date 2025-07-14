var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true); // 모든 origin 허용 (개발용)
    });
});

var app = builder.Build();

app.UseCors();

// 기본 파일 (index.html, test.html 등)을 루트로 자동 매핑
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "test.html" }
});

app.UseStaticFiles(); // 정적 파일 서빙

app.MapHub<ChatHub>("/chathub");

//app.MapGet("/", () => "Hello World!");

app.Run();
