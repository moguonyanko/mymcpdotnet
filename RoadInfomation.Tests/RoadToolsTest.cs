using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using RoadInfomation.Tools; // テスト対象のクラスを参照
using System.Threading;
using Moq.Protected; // Protected メソッドをモックするために必要
using System.Net; // HttpStatusCode を使用するために必要
using System.Diagnostics; // Console.Error の代わりに Trace.WriteLine を使用する場合

// GetBridgesByAreaRequest の定義が異なる場合、この using も調整してください
// using RoadInfomation.Models; // 例えば Models フォルダに定義している場合

public class RoadToolsTests
{
    // ダミーのベースアドレス
    private const string BaseUrl = "https://road-structures-db.mlit.go.jp/xROAD/api/v1";

    // HttpClient のモックとそれを使用したインスタンスを生成するヘルパーメソッド
    private HttpClient CreateMockHttpClient(HttpResponseMessage mockResponse)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(mockResponse);

        var client = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(BaseUrl)
        };
        return client;
    }

    [Fact]
    public async Task GetBridgesByArea_ReturnsBridgeInfo_WhenDataExists()
    {
        // Arrange
        var expectedResultJson = "{\"result\": \"橋梁情報です\"}";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedResultJson, System.Text.Encoding.UTF8, "application/json")
        };
        var client = CreateMockHttpClient(httpResponseMessage);

        var request = new GetBridgesByAreaRequest(new double[] { 35.0, 36.0, 135.0, 136.0 });

        // Act
        var result = await RoadTools.GetBridgesByArea(client, request);

        // Assert
        // 結果が期待されるJSON文字列を含むことを検証
        Assert.Contains("橋梁情報です", result);
        Assert.False(result.Contains("\"code\":\"500\""), "エラーコードが含まれていません"); // エラーコードが含まれていないことを確認
    }

    [Fact]
    public async Task GetBridgesByArea_ReturnsEmptyResult_WhenDataIsNull()
    {
        // Arrange
        // APIがJSONの"null"を直接返す場合
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };
        var client = CreateMockHttpClient(httpResponseMessage);

        var request = new GetBridgesByAreaRequest(new double[] { 35.0, 36.0, 135.0, 136.0 });

        // Act
        var result = await RoadTools.GetBridgesByArea(client, request);

        // Assert
        var expectedEmptyResult = new
        {
            code = "404",
            message = "指定された範囲には橋梁情報が見つかりませんでした。"
        };
        Assert.Equal(JsonSerializer.Serialize(expectedEmptyResult), result);
    }

    [Fact]
    public async Task GetBridgesByArea_ReturnsEmptyResult_WhenResultIsNull()
    {
        // Arrange
        // APIが {"result": null} のようなJSONを返す場合
        var jsonResponse = "{\"result\": null}"; 
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };
        var client = CreateMockHttpClient(httpResponseMessage);

        var request = new GetBridgesByAreaRequest(new double[] { 35.0, 36.0, 135.0, 136.0 });

        // Act
        var result = await RoadTools.GetBridgesByArea(client, request);

        // Assert
        var expectedEmptyResult = new
        {
            code = "404",
            message = "指定された範囲には橋梁情報が見つかりませんでした。"
        };
        Assert.Equal(JsonSerializer.Serialize(expectedEmptyResult), result);
    }

    [Fact]
    public async Task GetBridgesByArea_ReturnsEmptyResult_WhenApiReturnsNotFound()
    {
        // Arrange
        // 外部APIが404 Not Foundを返す場合
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
             // 404の場合、APIが何かボディを返す可能性もあるので、空でないContentを設定することもできます
             Content = new StringContent("{\"error\":\"Not Found\"}", System.Text.Encoding.UTF8, "application/json")
        };
        var client = CreateMockHttpClient(httpResponseMessage);

        var request = new GetBridgesByAreaRequest(new double[] { 35.0, 36.0, 135.0, 136.0 });

        // Act
        var result = await RoadTools.GetBridgesByArea(client, request);

        // Assert
        var expectedEmptyResult = new
        {
            code = "404", // あなたのコードが404エラー時に返すコード
            message = "指定された範囲には橋梁情報が見つかりませんでした。"
        };
        Assert.Equal(JsonSerializer.Serialize(expectedEmptyResult), result);
    }

    [Fact]
    public async Task GetBridgesByArea_ReturnsErrorResult_WhenApiReturnsInternalServerError()
    {
        // Arrange
        // 外部APIが500 Internal Server Errorを返す場合
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("{\"error\":\"Internal Server Error on API\"}", System.Text.Encoding.UTF8, "application/json")
        };
        var client = CreateMockHttpClient(httpResponseMessage);

        var request = new GetBridgesByAreaRequest(new double[] { 35.0, 36.0, 135.0, 136.0 });

        // Act
        var result = await RoadTools.GetBridgesByArea(client, request);

        // Assert
        // HTTPエラー時のカスタムエラーメッセージが返されることを検証
        Assert.Contains("外部API呼び出しエラー (HTTP): Response status code does not indicate success: 500 (InternalServerError).", result);
        Assert.Contains("\"code\":\"InternalServerError\"", result); // HTTPステータスコードがコードに含まれることを確認
    }

    [Fact]
    public async Task GetBridgesByArea_ReturnsErrorResult_WhenApiReturnsInvalidJson()
    {
        // Arrange
        // 外部APIが不正なJSONを返す場合
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{invalid json", System.Text.Encoding.UTF8, "application/json") // 不正なJSON
        };
        var client = CreateMockHttpClient(httpResponseMessage);

        var request = new GetBridgesByAreaRequest(new double[] { 35.0, 36.0, 135.0, 136.0 });

        // Act
        var result = await RoadTools.GetBridgesByArea(client, request);

        // Assert
        // JSON解析エラー時のカスタムエラーメッセージが返されることを検証
        Assert.Contains("外部APIからの応答JSON解析エラー:", result);
        Assert.Contains("\"code\":\"500\"", result);
    }
}