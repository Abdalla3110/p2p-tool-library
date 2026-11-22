using BLL.Services;

namespace Core.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ProducesVerifiableHash()
    {
        var hash = PasswordHasher.Hash("MySecureP@ss1");
        Assert.True(PasswordHasher.Verify("MySecureP@ss1", hash));
    }

    [Fact]
    public void Verify_RejectsWrongPassword()
    {
        var hash = PasswordHasher.Hash("correct-horse");
        Assert.False(PasswordHasher.Verify("wrong-horse", hash));
    }

    [Fact]
    public void Hash_IsUniquePerCall()
    {
        var h1 = PasswordHasher.Hash("same-password");
        var h2 = PasswordHasher.Hash("same-password");
        Assert.NotEqual(h1, h2); // different salts
    }

    [Fact]
    public void Hash_HasExpectedFormat()
    {
        var hash = PasswordHasher.Hash("test");
        Assert.StartsWith("pbkdf2$", hash);
        var parts = hash.Split('$');
        Assert.Equal(4, parts.Length);
        Assert.Equal("100000", parts[1]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("x")]
    public void Verify_InvalidHash_ReturnsFalse(string bad)
    {
        Assert.False(PasswordHasher.Verify("anything", bad));
    }
}
