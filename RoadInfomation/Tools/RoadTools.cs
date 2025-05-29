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
      [Description("検索範囲を示す座標の配列です。書式は「北緯の下限,北緯の上限,東経の下限,東経の上限」です。")] double[] area)
  {
    var jsonElement = await client.GetFromJsonAsync<JsonElement>($"/bridges/{area[0]},{area[1]},{area[2]},{area[3]}");
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

