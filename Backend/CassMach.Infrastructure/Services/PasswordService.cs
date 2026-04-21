using CassMach.Application.Interfaces;
using BCrypt.Net;
using CassMach.Domain.Common.Interfaces;
using CassMach.Domain.Common.Interfaces.Repositories;
using CassMach.Domain.Entities;
using CassMach.Application.Common.Results;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CassMach.Domain.Models;
using CassMach.Domain.Common.Enums;
using Microsoft.Extensions.Logging;

namespace CassMach.Infrastructure.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(ILogger<PasswordService> logger)
        {
            _logger = logger;
        }

    public Result<string> HashPassword(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
                return Result<string>.Failure(Error.Failure(
                    ErrorCode.ValidationFailed,
                    "Password cannot be empty"));

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
            return Result<string>.Success(hashedPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error hashing password");
            return Result<string>.Failure(Error.Failure(
                ErrorCode.InternalError,
                "An unexpected error occurred"));
        }
    }

    public Result<bool> VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return Result<bool>.Success(false);

            var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            return Result<bool>.Success(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error verifying password");
            return Result<bool>.Failure(Error.Failure(
                ErrorCode.InternalError,
                "An unexpected error occurred"));
        }
    }

    public Result<string> GenerateRandomPassword(int length = 12)
        {
            try
            {
                if (length < 8)
                return Result<string>.Failure(
            Error.Failure(ErrorCode.ValidationFailed, "Password must be at least 8 characters long"));

            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
                const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string digits = "0123456789";
                const string special = "!@#$%^&*";

                var password = new StringBuilder();

                // Ensure at least one character from each category
                password.Append(lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)]);
                password.Append(uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)]);
                password.Append(digits[RandomNumberGenerator.GetInt32(digits.Length)]);
                password.Append(special[RandomNumberGenerator.GetInt32(special.Length)]);

                // Fill the rest randomly
                string allChars = lowercase + uppercase + digits + special;
                for (int i = 4; i < length; i++)
                {
                    password.Append(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);
                }

                // Shuffle the password
                var chars = password.ToString().ToCharArray();
                for (int i = chars.Length - 1; i > 0; i--)
                {
                    int j = RandomNumberGenerator.GetInt32(i + 1);
                    (chars[i], chars[j]) = (chars[j], chars[i]);
                }

            return Result<string>.Success(new string(chars));
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating random password");
                return Result<string>.Failure(
                    Error.Failure(ErrorCode.InternalError, "An unexpected error occurred"));
            }
        }

    public Result<bool> ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, "Password cannot be empty"));

        if (password.Length < 8)
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, "Password must be at least 8 characters long"));

        if (!Regex.IsMatch(password, @"[a-z]"))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, "Password must contain at least one lowercase letter"));

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, "Password must contain at least one uppercase letter"));

        if (!Regex.IsMatch(password, @"\d"))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, "Password must contain at least one digit"));

        return Result<bool>.Success(true);
    }

}
}
