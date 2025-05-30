/**
 * C#でModel Context Protocol (MCP)を使用して道路施設情報を取得するツールのサンプルコード
 * 参考サイト:
 * https://modelcontextprotocol.io/quickstart/server#c
 */
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using System.Net.Http.Headers;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddSingleton(_ =>
{
  var client = new HttpClient() {
    BaseAddress = new Uri("https://road-structures-db.mlit.go.jp/xROAD/api/v1") 
  };
  client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("my-road-tools", "1.0"));
  return client;
});

var app = builder.Build();

await app.RunAsync();

