namespace Ternary.Numerics;

/// <summary>
/// A single balanced ternary digit. Valid values are <c>-1</c>, <c>0</c>, and <c>+1</c>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Trit"/> is a <see langword="readonly struct"/> backed by a single
/// <see cref="sbyte"/> — zero allocations, value-type semantics, suitable for
/// dense storage in arrays and spans.
/// </para>
/// <para>
/// In balanced ternary, the natural "logic" operations are:
/// <list type="bullet">
///   <item><description><b>AND</b> = minimum of the two inputs.</description></item>
///   <item><description><b>OR</b> = maximum of the two inputs.</description></item>
///   <item><description><b>NOT</b> = arithmetic negation (sign flip).</description></item>
/// </list>
/// Arithmetic operators (<c>+ - * /</c>) are intentionally not defined at the trit
/// level because a single-trit addition produces a (carry, sum) pair. See the
/// fixed-width <c>TritWord</c> family for carry-aware arithmetic.
/// </para>
/// </remarks>
[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Trit
    : IEquatable<Trit>, IComparable<Trit>, IComparable
{
    private readonly sbyte _value;

    /// <summary>The negative trit: <c>-1</c>.</summary>
    public static Trit Negative { get; } = new(-1);

    /// <summary>The zero trit: <c>0</c>.</summary>
    public static Trit Zero { get; } = new(0);

    /// <summary>The positive trit: <c>+1</c>.</summary>
    public static Trit Positive { get; } = new(1);

    /// <summary>Equivalent to <see cref="Negative"/>.</summary>
    public static Trit MinValue => Negative;

    /// <summary>Equivalent to <see cref="Positive"/>.</summary>
    public static Trit MaxValue => Positive;

    /// <summary>The signed integer value of this trit: <c>-1</c>, <c>0</c>, or <c>+1</c>.</summary>
    public sbyte Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Trit(sbyte value)
    {
        Debug.Assert(value is -1 or 0 or 1, $"Invariant violation: Trit constructed with {value}.");
        _value = value;
    }

    /// <summary>Construct a <see cref="Trit"/> from a 32-bit signed integer.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value"/> is not <c>-1</c>, <c>0</c>, or <c>+1</c>.
    /// </exception>
    public static Trit FromInt32(int value) => value switch
    {
        -1 => Negative,
        0 => Zero,
        1 => Positive,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value,
            "Trit values must be -1, 0, or +1."),
    };

    /// <summary>Explicit cast from <see cref="int"/>; validates range.</summary>
    public static explicit operator Trit(int value) => FromInt32(value);

    /// <summary>Implicit widening to <see cref="int"/>.</summary>
    public static implicit operator int(Trit trit) => trit._value;

    // ----- Ternary logic ---------------------------------------------------

    /// <summary>Ternary AND: minimum of two trits.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Trit And(Trit a, Trit b) =>
        new(a._value <= b._value ? a._value : b._value);

    /// <summary>Ternary OR: maximum of two trits.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Trit Or(Trit a, Trit b) =>
        new(a._value >= b._value ? a._value : b._value);

    /// <summary>Ternary NOT: sign flip.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Trit Not(Trit t) => new((sbyte)(-t._value));

    /// <summary>Equivalent to <see cref="And"/>.</summary>
    public static Trit operator &(Trit a, Trit b) => And(a, b);

    /// <summary>Equivalent to <see cref="Or"/>.</summary>
    public static Trit operator |(Trit a, Trit b) => Or(a, b);

    /// <summary>Unary negation; equivalent to <see cref="Not"/>.</summary>
    public static Trit operator -(Trit t) => Not(t);

    // ----- Equality & ordering --------------------------------------------

    public bool Equals(Trit other) => _value == other._value;

    public override bool Equals(object? obj) => obj is Trit other && Equals(other);

    public override int GetHashCode() => _value;

    public int CompareTo(Trit other) => _value.CompareTo(other._value);

    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        Trit other => CompareTo(other),
        _ => throw new ArgumentException($"Object must be of type {nameof(Trit)}.", nameof(obj)),
    };

    public static bool operator ==(Trit left, Trit right) => left.Equals(right);
    public static bool operator !=(Trit left, Trit right) => !left.Equals(right);
    public static bool operator <(Trit left, Trit right) => left._value < right._value;
    public static bool operator <=(Trit left, Trit right) => left._value <= right._value;
    public static bool operator >(Trit left, Trit right) => left._value > right._value;
    public static bool operator >=(Trit left, Trit right) => left._value >= right._value;

    // ----- Formatting -----------------------------------------------------

    /// <summary>
    /// Returns <c>"-"</c>, <c>"0"</c>, or <c>"+"</c> per Knuth's balanced ternary
    /// convention from <i>TAOCP</i> Vol 2 §4.1.
    /// </summary>
    public override string ToString() => _value switch
    {
        -1 => "-",
        0 => "0",
        1 => "+",
        _ => throw new UnreachableException($"Trit invariant violated: {_value}."),
    };
}
