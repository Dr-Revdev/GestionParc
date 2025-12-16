using System;
using Xunit;

namespace GestiParc.Tests.TestUtils;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiresEnvironmentVariableFactAttribute : FactAttribute
{
    public RequiresEnvironmentVariableFactAttribute(string envVarName)
    {
        if (string.IsNullOrWhiteSpace(envVarName))
            throw new ArgumentException("Le nom de la variable d'environnement ne peut pas être vide.", nameof(envVarName));

        var value = Environment.GetEnvironmentVariable(envVarName);
        if (string.IsNullOrWhiteSpace(value))
        {
            Skip = $"Test ignoré. Définis la variable d'environnement '{envVarName}' pour exécuter ce test.";
        }
    }
}
