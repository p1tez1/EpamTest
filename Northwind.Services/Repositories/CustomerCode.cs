using System.Diagnostics;

namespace Northwind.Services.Repositories;

[DebuggerDisplay("{Code}")]
public class CustomerCode
{
    public CustomerCode(string code)
    {
        if (code.Length != 5)
        {
            throw new ArgumentException("The length of code argument is not equal to 5.", nameof(code));
        }

        for (int i = 0; i < code.Length; i++)
        {
            if (!char.IsUpper(code[i]))
            {
                throw new ArgumentException($"The {i + 1} character in the customer code is not an uppercase letter.");
            }
        }

        this.Code = code;
    }

    public string Code { get; init; }
}
