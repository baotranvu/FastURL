using System;

namespace FastUrl.API.Infrastructure;

/// <summary>
/// DEMO FILE ONLY - Injected deliberate security flaws to test CodeQL CI/CD detection.
/// </summary>
public class InsecureCodeDemo
{
    // 1. Hardcoded Secret Flaw
    private const string AWS_SECRET_KEY = "AKIAIOSFODNN7EXAMPLE_DUMMY_SECRET_KEY_999";

    public string BuildUnsafeQuery(string userInput)
    {
        // 2. SQL Injection Flaw (Raw string concatenation instead of parameterized query)
        string rawSqlQuery = "SELECT * FROM ShortUrls WHERE ShortCode = '" + userInput + "'";
        Console.WriteLine($"Generated query with secret {AWS_SECRET_KEY}");
        return rawSqlQuery;
    }
}
