using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Unitask.Api.Services;

public class MomoSettings
{
    public string MomoApiUrl { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
    public string NotifyUrl { get; set; } = null!;
    public string PartnerCode { get; set; } = null!;
    public string RequestType { get; set; } = "payWithMethod";
    public int ExpireMinutes { get; set; } = 15;
}

public class MomoCreateResponse
{
    public int ResultCode { get; set; }
    public string? Message { get; set; }
    public string? PayUrl { get; set; }
    public string? OrderId { get; set; }
    public string? RequestId { get; set; }
}

public class MomoIpnRequest
{
    public string? PartnerCode { get; set; }
    public string? OrderId { get; set; }
    public string? RequestId { get; set; }
    public long Amount { get; set; }
    public string? OrderInfo { get; set; }
    public string? OrderType { get; set; }
    public long TransId { get; set; }
    public int ResultCode { get; set; }
    public string? Message { get; set; }
    public string? PayType { get; set; }
    public long ResponseTime { get; set; }
    public string? ExtraData { get; set; }
    public string? Signature { get; set; }
}

public class MomoService
{
    private readonly MomoSettings _settings;
    private readonly HttpClient _httpClient;

    public MomoService(MomoSettings settings, HttpClient httpClient)
    {
        _settings = settings;
        _httpClient = httpClient;
    }

    public async Task<MomoCreateResponse> CreatePaymentAsync(string orderId, long amount, string orderInfo, string extraData = "")
    {
        var requestId = Guid.NewGuid().ToString();

        var rawSignature = $"accessKey={_settings.AccessKey}" +
                           $"&amount={amount}" +
                           $"&extraData={extraData}" +
                           $"&ipnUrl={_settings.NotifyUrl}" +
                           $"&orderId={orderId}" +
                           $"&orderInfo={orderInfo}" +
                           $"&partnerCode={_settings.PartnerCode}" +
                           $"&redirectUrl={_settings.ReturnUrl}" +
                           $"&requestId={requestId}" +
                           $"&requestType={_settings.RequestType}";

        var signature = HmacSha256(rawSignature, _settings.SecretKey);

        var body = new
        {
            partnerCode = _settings.PartnerCode,
            partnerName = "UniTask",
            storeId = "UniTaskStore",
            requestId,
            amount,
            orderId,
            orderInfo,
            redirectUrl = _settings.ReturnUrl,
            ipnUrl = _settings.NotifyUrl,
            lang = "vi",
            requestType = _settings.RequestType,
            autoCapture = true,
            extraData,
            signature
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_settings.MomoApiUrl, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<MomoCreateResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? new MomoCreateResponse { ResultCode = -1, Message = "Deserialize failed" };
    }

    public bool VerifySignature(MomoIpnRequest ipn)
    {
        var rawSignature = $"accessKey={_settings.AccessKey}" +
                           $"&amount={ipn.Amount}" +
                           $"&extraData={ipn.ExtraData}" +
                           $"&message={ipn.Message}" +
                           $"&orderId={ipn.OrderId}" +
                           $"&orderInfo={ipn.OrderInfo}" +
                           $"&orderType={ipn.OrderType}" +
                           $"&partnerCode={ipn.PartnerCode}" +
                           $"&payType={ipn.PayType}" +
                           $"&requestId={ipn.RequestId}" +
                           $"&responseTime={ipn.ResponseTime}" +
                           $"&resultCode={ipn.ResultCode}" +
                           $"&transId={ipn.TransId}";

        var computed = HmacSha256(rawSignature, _settings.SecretKey);
        return string.Equals(computed, ipn.Signature, StringComparison.OrdinalIgnoreCase);
    }

    private static string HmacSha256(string message, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
