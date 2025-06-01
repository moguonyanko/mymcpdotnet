/**
 * 
 * 参考:
 * https://rirs.or.jp/tenken-db/pdf/api_specification.pdf
 */

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace RoadInfomation.Tools;

[McpServerToolType]
public static class RoadTools
{
  private static void DumpRequestJson(GetBridgesByAreaRequest requestBody)
  {
    var options = new JsonSerializerOptions { WriteIndented = true };
    Console.Error.WriteLine($"--- リクエストボディの内容 ---");
    Console.Error.WriteLine(JsonSerializer.Serialize(requestBody, options));
    Console.Error.WriteLine($"--------------------------");
  }

  private static void DumpRequestHeaders(HttpClient client)
  {
    Console.Error.WriteLine($"--- リクエストヘッダーの内容 ---");
    foreach (var header in client.DefaultRequestHeaders)
    {
      Console.Error.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
    }
    Console.Error.WriteLine($"--------------------------");
  }

  [McpServerTool, Description("指定された経緯度を元に橋梁の情報を取得します。")]
  public static async Task<string> GetBridgesByArea(
      HttpClient client,
      [Description("検索範囲を示す座標の配列です。書式は「北緯の下限,北緯の上限,東経の下限,東経の上限」です。")] GetBridgesByAreaRequest request)
  {
    DumpRequestJson(request);
    DumpRequestHeaders(client);

    var emptyResult = new
    {
      code = "404",
      message = "指定された範囲には橋梁情報が見つかりませんでした。"
    };
    
    JsonElement jsonElement;
    try
    {
      var areaQuery = $"{request.Area[0]},{request.Area[1]},{request.Area[2]},{request.Area[3]}";
      var requestPath = $"/bridges?area={areaQuery}";
      var requestUrl = $"{client.BaseAddress}{requestPath}";
      Console.Error.WriteLine($"外部API呼び出しURL: {requestUrl}");

      // requestUrlを渡すとGetBridgesByAreaメソッドが正しく動作しないため、requestPathを使用
      jsonElement = await client.GetFromJsonAsync<JsonElement>(requestPath);

      if (jsonElement.ValueKind == JsonValueKind.Null)
      {
        return JsonSerializer.Serialize(emptyResult);
      }
    }
    catch (HttpRequestException httpEx)
    {
      // HTTPリクエストに関するエラー (例: ネットワークエラー、4xx/5xx ステータスコード)
      Console.Error.WriteLine($"HttpRequestException: {httpEx.Message}");
      Console.Error.WriteLine($"Status Code: {httpEx.StatusCode}"); // HTTPステータスコードがあれば表示
      Console.Error.WriteLine($"Request URL: {client.BaseAddress}{httpEx.Data["RequestUri"]}"); // リクエストURLも表示
      // 詳細なエラーメッセージを返す
      return JsonSerializer.Serialize(new { code = $"{httpEx.StatusCode}", message = $"外部API呼び出しエラー (HTTP): {httpEx.Message}. HTTP Status: {httpEx.StatusCode?.ToString() ?? "N/A"}" });
    }
    catch (JsonException jsonEx)
    {
      // JSONのデシリアライズに関するエラー
      Console.Error.WriteLine($"JsonException: {jsonEx.Message}");
      // 詳細なエラーメッセージを返す
      return JsonSerializer.Serialize(new { code = "500", message = $"外部APIからの応答JSON解析エラー: {jsonEx.Message}" });
    }
    catch (Exception ex)
    {
      // その他の予期せぬエラー
      Console.Error.WriteLine($"予期せぬエラー: {ex.Message}");
      Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
      // 詳細なエラーメッセージを返す
      return JsonSerializer.Serialize(new { code = "500", message = $"不明なエラーが発生しました: {ex.Message}. StackTrace: {ex.StackTrace?.Substring(0, Math.Min(ex.StackTrace.Length, 200)) ?? "N/A"}" }); // StackTraceも一部返す
    }

    var result = jsonElement.GetProperty("result").GetString();

    return result ?? JsonSerializer.Serialize(emptyResult);
  }
}

