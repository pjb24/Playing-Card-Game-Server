var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 기본 파일 (index.html, test.html 등)을 루트로 자동 매핑
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "test.html" }
});

app.UseStaticFiles(); // 정적 파일 서빙

//app.MapGet("/", () => "Hello World!");

app.Run();
