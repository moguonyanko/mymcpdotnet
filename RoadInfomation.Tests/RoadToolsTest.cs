using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using Xunit;
using Moq;
using Moq.Protected;
using RoadInfomation.Tools;

public class RoadToolsTests
{
    private const string BaseUrl = "https://road-structures-db.mlit.go.jp/xROAD/api/v1";

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
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("my-road-tools-test", "1.0"));
        return client;
    }

    /**
     * TODO: AIクライアントやブラウザでAPIを呼び出した時には発生しない500エラーが発生して失敗する。
     */
    [Fact]
    public async Task GetBridgesByArea_ReturnsBridgeInfo_WhenDataExists()
    {
        // TODO: expectedResultJsonには実際のAPIレスポンスに合わせて期待されるJSONレスポンスを設定する。
        var expectedResultJson = "DUMMY_JSON_RESPONSE";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedResultJson, System.Text.Encoding.UTF8, "application/json")
        };
        var client = CreateMockHttpClient(httpResponseMessage);
        var request = new GetBridgesByAreaRequest(new double[] { 35.4631,35.4685,139.6174,139.6228 });

        var result = await RoadTools.GetBridgesByArea(client, request);

        using var json = JsonDocument.Parse(result);
        var resultJsonRoot = json.RootElement;
        Console.WriteLine($"RESULT JSON: {resultJsonRoot}");
        Assert.NotNull(resultJsonRoot.GetProperty("result"));
    }

}