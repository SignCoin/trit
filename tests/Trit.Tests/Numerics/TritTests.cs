using Ternary.Numerics;

namespace Ternary.Tests.Numerics;

public class TritTests
{
    // ----- Construction & states -----------------------------------------

    [Fact]
    public void Negative_value_is_minus_one()
    {
        Assert.Equal(-1, (int)Trit.Negative);
    }

    [Fact]
    public void Zero_value_is_zero()
    {
        Assert.Equal(0, (int)Trit.Zero);
    }

    [Fact]
    public void Positive_value_is_plus_one()
    {
        Assert.Equal(1, (int)Trit.Positive);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void FromInt32_accepts_valid_values(int value)
    {
        var trit = Trit.FromInt32(value);
        Assert.Equal(value, (int)trit);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void FromInt32_rejects_out_of_range(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Trit.FromInt32(value));
    }

    // ----- Logic ----------------------------------------------------------

    [Theory]
    [InlineData(-1, -1, -1)]
    [InlineData(-1, 0, -1)]
    [InlineData(-1, 1, -1)]
    [InlineData(0, -1, -1)]
    [InlineData(0, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(1, -1, -1)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 1, 1)]
    public void And_is_minimum(int a, int b, int expected)
    {
        var ta = Trit.FromInt32(a);
        var tb = Trit.FromInt32(b);
        Assert.Equal(expected, (int)Trit.And(ta, tb));
        Assert.Equal(expected, (int)(ta & tb));
    }

    [Theory]
    [InlineData(-1, -1, -1)]
    [InlineData(-1, 0, 0)]
    [InlineData(-1, 1, 1)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, 0)]
    [InlineData(0, 1, 1)]
    [InlineData(1, -1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 1)]
    public void Or_is_maximum(int a, int b, int expected)
    {
        var ta = Trit.FromInt32(a);
        var tb = Trit.FromInt32(b);
        Assert.Equal(expected, (int)Trit.Or(ta, tb));
        Assert.Equal(expected, (int)(ta | tb));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 0)]
    [InlineData(1, -1)]
    public void Not_flips_sign(int input, int expected)
    {
        var t = Trit.FromInt32(input);
        Assert.Equal(expected, (int)Trit.Not(t));
        Assert.Equal(expected, (int)(-t));
    }

    // ----- Equality & ordering -------------------------------------------

    [Fact]
    public void Equal_trits_are_equal_via_operator_and_method()
    {
        var a = Trit.Zero;
        var b = Trit.FromInt32(0);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object)b));
    }

    [Fact]
    public void Distinct_trits_are_not_equal()
    {
        Assert.True(Trit.Negative != Trit.Positive);
        Assert.False(Trit.Negative == Trit.Positive);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(-1, 1)]
    [InlineData(0, 1)]
    public void CompareTo_respects_balanced_ordering(int a, int b)
    {
        var ta = Trit.FromInt32(a);
        var tb = Trit.FromInt32(b);
        Assert.True(ta.CompareTo(tb) < 0);
        Assert.True(ta < tb);
        Assert.True(tb > ta);
    }

    // ----- Formatting -----------------------------------------------------

    [Theory]
    [InlineData(-1, "-")]
    [InlineData(0, "0")]
    [InlineData(1, "+")]
    public void ToString_uses_Knuth_balanced_glyphs(int value, string expected)
    {
        var trit = Trit.FromInt32(value);
        Assert.Equal(expected, trit.ToString());
    }

    // ----- Property-based: arithmetic identities -------------------------

    /// <summary>Double-NOT is the identity function on <see cref="Trit"/>.</summary>
    [Property]
    public bool Not_is_involution(int seed)
    {
        var trit = TritFromSeed(seed);
        return Trit.Not(Trit.Not(trit)) == trit;
    }

    /// <summary>AND is commutative.</summary>
    [Property]
    public bool And_is_commutative(int seedA, int seedB)
    {
        var a = TritFromSeed(seedA);
        var b = TritFromSeed(seedB);
        return Trit.And(a, b) == Trit.And(b, a);
    }

    /// <summary>OR is commutative.</summary>
    [Property]
    public bool Or_is_commutative(int seedA, int seedB)
    {
        var a = TritFromSeed(seedA);
        var b = TritFromSeed(seedB);
        return Trit.Or(a, b) == Trit.Or(b, a);
    }

    /// <summary>De Morgan's law for balanced ternary: ¬(a ∧ b) = (¬a) ∨ (¬b).</summary>
    [Property]
    public bool DeMorgan_AndOr(int seedA, int seedB)
    {
        var a = TritFromSeed(seedA);
        var b = TritFromSeed(seedB);
        return Trit.Not(Trit.And(a, b)) == Trit.Or(Trit.Not(a), Trit.Not(b));
    }

    private static Trit TritFromSeed(int seed) =>
        Trit.FromInt32(((seed % 3) + 3) % 3 - 1);
}
