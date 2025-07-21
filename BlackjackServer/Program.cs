var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(); // SignalR 서비스 등록
builder.Services.AddSingleton<UserManager>(); // UserManager 서비스 등록
builder.Services.AddSingleton<GameRoomManager>(); // GameRoomManager 서비스 등록

var app = builder.Build();

// 기본 파일 (index.html, test.html 등)을 루트로 자동 매핑
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "test.html" }
});

app.UseStaticFiles(); // 정적 파일 서빙

//app.MapGet("/", () => "Hello World!");

// SignalR 허브 엔드포인트 설정
app.MapHub<BlackjackHub>("/blackjackHub");

app.Run();
