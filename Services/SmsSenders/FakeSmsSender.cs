using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Services.SmsSenders
{
    public class FakeSmsSender : ISmsSender
    {
        private readonly ILogger<FakeSmsSender> _logger;
        private static readonly ConcurrentDictionary<string, string> _otpStore = new();
        private static readonly ConcurrentDictionary<string, bool> _verifiedStore = new();

        public FakeSmsSender(ILogger<FakeSmsSender> logger)
        {
            _logger = logger;
        }

        public Task SendSmsAsync(string number, string message)
        {
            _otpStore[number] = message;
            _verifiedStore[number] = false;

            _logger.LogInformation("FakeSmsSender: to={Number}, message={Message}", number, message);
            Console.WriteLine($"[FakeSmsSender] To: {number} - OTP: {message}");
            return Task.CompletedTask;
        }

        public static bool TryGetLastOtp(string number, out string otp)
            => _otpStore.TryGetValue(number, out otp);

        public static bool VerifyOtp(string number, string otp)
        {
            if (_otpStore.TryGetValue(number, out var storedOtp) && storedOtp == otp)
            {
                _verifiedStore[number] = true;
                _otpStore.TryRemove(number, out _);
                return true;
            }
            return false;
        }

        public static bool IsVerified(string number)
            => _verifiedStore.TryGetValue(number, out var verified) && verified;
    }
}
