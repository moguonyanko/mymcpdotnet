using Xunit;
using Moq; // Moq を使用して HttpClient をモックします
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using RoadInfomation.Tools; // ★ここが変更されました
using System.Threading; // CancellationToken のために必要
using Moq.Protected; // Protected を使用するために必要

public class RoadToolsTests
{
    [Fact]
    public async Task GetBridgesByArea_ReturnsBridgeInfo_WhenDataExists()
    {
        // Arrange
        // モックされた HttpClient を設定します
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var jsonResponse = "{\"result\": \"橋梁情報です\"}";
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        mockHttpMessageHandler.Protected() // Protected メソッドをモックするために必要です
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        double[] area = { 35.0, 36.0, 135.0, 136.0 }; // テスト用の範囲

        // Act
        var result = await RoadTools.GetBridgesByArea(httpClient, area);

        // Assert
        Assert.Contains("橋梁情報です", result);
    }

    [Fact]
    public async Task GetBridgesByArea_ReturnsEmptyResult_WhenDataIsNull()
    {
        // Arrange
        // モックされた HttpClient を設定します (null レスポンスを返すように)
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("null") // null レスポンス
        };

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        double[] area = { 35.0, 36.0, 135.0, 136.0 };

        // Act
        var result = await RoadTools.GetBridgesByArea(httpClient, area);

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
        // モックされた HttpClient を設定します (result が null のレスポンスを返すように)
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var jsonResponse = "{\"result\": null}"; // result が null のレスポンス
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        double[] area = { 35.0, 36.0, 135.0, 136.0 };

        // Act
        var result = await RoadTools.GetBridgesByArea(httpClient, area);

        // Assert
        var expectedEmptyResult = new
        {
            code = "404",
            message = "指定された範囲には橋梁情報が見つかりませんでした。"
        };
        Assert.Equal(JsonSerializer.Serialize(expectedEmptyResult), result);
    }
}