using System;
using System.Collections.Generic;

namespace PolyBucket.Core.Exceptions
{
    /// <summary>
    /// Base exception for authentication-related errors
    /// </summary>
    public class AuthException : Exception
    {
        public AuthException(string message) : base(message)
        {
        }

        public AuthException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when validation errors occur during authentication operations
    /// </summary>
    public class ValidationException : AuthException
    {
        public Dictionary<string, List<string>> ValidationErrors { get; }

        public ValidationException(string message, Dictionary<string, List<string>> validationErrors) 
            : base(message)
        {
            ValidationErrors = validationErrors ?? new Dictionary<string, List<string>>();
        }
    }

    /// <summary>
    /// Exception thrown when authentication fails due to invalid credentials
    /// </summary>
    public class InvalidCredentialsException : AuthException
    {
        public InvalidCredentialsException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an account is locked
    /// </summary>
    public class AccountLockedException : AuthException
    {
        public DateTime? LockoutEnd { get; }

        public AccountLockedException(string message, DateTime? lockoutEnd = null) : base(message)
        {
            LockoutEnd = lockoutEnd;
        }
    }

    /// <summary>
    /// Exception thrown when a required email verification hasn't been completed
    /// </summary>
    public class EmailNotVerifiedException : AuthException
    {
        public EmailNotVerifiedException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a resource already exists (e.g., username, email)
    /// </summary>
    public class ResourceExistsException : AuthException
    {
        public string ResourceName { get; }
        public string ResourceValue { get; }

        public ResourceExistsException(string message, string resourceName, string resourceValue) 
            : base(message)
        {
            ResourceName = resourceName;
            ResourceValue = resourceValue;
        }
    }

    /// <summary>
    /// Exception thrown when a resource is not found
    /// </summary>
    public class ResourceNotFoundException : AuthException
    {
        public string ResourceName { get; }
        
        public ResourceNotFoundException(string message, string resourceName) : base(message)
        {
            ResourceName = resourceName;
        }
    }

    /// <summary>
    /// Exception thrown when a token is invalid or expired
    /// </summary>
    public class InvalidTokenException : AuthException
    {
        public string TokenType { get; }
        
        public InvalidTokenException(string message, string tokenType) : base(message)
        {
            TokenType = tokenType;
        }
    }
} 