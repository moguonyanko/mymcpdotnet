using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace RoadInfomation.Tools;

[McpServerToolType]
public static class RoadTools
{
  [McpServerTool, Description("指定された経緯度を元に橋梁の情報を取得します。")]
  public static async Task<string> GetBridgesByArea(
      HttpClient client,
      // ★変更: 引数をリクエストボディの型に変更
      // McpServerTool が自動的にリクエストボディをこの型にデシリアライズすることを想定
      [Description("検索範囲を示す座標の配列です。書式は「北緯の下限,北緯の上限,東経の下限,東経の上限」です。")] GetBridgesByAreaRequest request)
  {
    // ★変更: request.Area から値を取得
    var jsonElement = await client.GetFromJsonAsync<JsonElement>($"/bridges/{request.Area[0]},{request.Area[1]},{request.Area[2]},{request.Area[3]}");

    var emptyResult = new
    {
      code = "404",
      message = "指定された範囲には橋梁情報が見つかりませんでした。"
    };
    if (jsonElement.ValueKind == JsonValueKind.Null)
    {
      return JsonSerializer.Serialize(emptyResult);
    }

    var result = jsonElement.GetProperty("result").GetString();

    return result ?? JsonSerializer.Serialize(emptyResult);
  }
}

