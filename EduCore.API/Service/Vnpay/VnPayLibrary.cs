using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace EduCore.API.Service.Vnpay
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value) => _requestData.Add(key, value);
        public void AddResponseData(string key, string value) => _responseData.Add(key, value);
        public string GetResponseData(string key) => _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
            string querystring = data.ToString().TrimEnd('&');
            string vnpSecureHash = HmacSha512(vnpHashSecret, querystring);
            return baseUrl + "?" + querystring + "&vnp_SecureHash=" + vnpSecureHash;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            string rspRaw = GetRawResponseData();
            string myChecksum = HmacSha512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetRawResponseData()
        {
            StringBuilder data = new StringBuilder();
            _responseData.Remove("vnp_SecureHashType");
            _responseData.Remove("vnp_SecureHash");
            foreach (var kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
            return data.ToString().TrimEnd('&');
        }

        private string HmacSha512(string key, string inputData)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using var hmac = new HMACSHA512(keyBytes);
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }

        public string GetIpAddress(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            return (string.IsNullOrEmpty(ip) || ip == "::1") ? "127.0.0.1" : ip;
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y) => string.Compare(x, y, StringComparison.Ordinal);
    }
}