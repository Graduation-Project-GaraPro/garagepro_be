using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Project_final.Libraries
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        #region Add Request Data
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _requestData[key] = value;
        }
        #endregion

        #region Create Payment URL
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            // 1. Lấy rawData để ký (KHÔNG encode)
            string rawData = GetSigningString(_requestData);

            // 2. Tạo chữ ký
            string vnpSecureHash = Utils.HmacSHA512(vnpHashSecret, rawData);

            // 3. Build query string (encode để gửi đi)
            var query = new StringBuilder();
            foreach (var kv in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                string encodedKey = HttpUtility.UrlEncode(kv.Key, Encoding.UTF8);
                string encodedValue = HttpUtility.UrlEncode(kv.Value, Encoding.UTF8);
                query.Append($"{encodedKey}={encodedValue}&");
            }

            // 4. Thêm chữ ký
            query.Append("vnp_SecureHash=" + vnpSecureHash);

            return baseUrl + "?" + query.ToString();
        }

        // Ghép rawData để ký
        private string GetSigningString(SortedList<string, string> data)
        {
            var list = new List<string>();
            foreach (var kv in data.Where(kv => !string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType"))
            {
                list.Add($"{kv.Key}={kv.Value}"); // giá trị GỐC (không encode)
            }
            return string.Join("&", list);
        }
        #endregion

        #region Validate the payment response
        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            if (string.IsNullOrEmpty(inputHash))
                return false;

            // Build rawData từ response
            var rawData = GetSigningString(_responseData);

            var myChecksum = Utils.HmacSHA512(secretKey, rawData);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        public string GetResponseValue(string key)
        {
            return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        public void LoadResponseData(IQueryCollection query)
        {
            foreach (var kv in query)
            {
                AddResponseData(kv.Key, kv.Value);
            }
        }
        #endregion

        #region Helpers
        public static string GetIpAddress(HttpContext context)
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;

            if (remoteIpAddress != null)
            {
                var ipv4Address = Dns.GetHostEntry(remoteIpAddress)
                    .AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

                return remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6 && ipv4Address != null
                    ? ipv4Address.ToString()
                    : remoteIpAddress.ToString();
            }

            throw new InvalidOperationException("Không tìm thấy địa chỉ IP");
        }
        #endregion
    }

    public static class Utils
    {
        public static string HmacSHA512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using var hmac = new HMACSHA512(keyBytes);
            return BitConverter.ToString(hmac.ComputeHash(inputBytes)).Replace("-", string.Empty);
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
