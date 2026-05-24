namespace Ternary.Encoding;

/// <summary>
/// Base-243 encoding: five trits packed into each byte (3⁵ = 243 &lt; 256).
/// </summary>
/// <remarks>
/// <para>
/// Trit <c>i</c> within a byte occupies the slot <c>i mod 5</c>; its value
/// is recovered by computing <c>(packed / 3ⁱ) mod 3</c>, which yields
/// <c>0, 1, 2</c> mapping to <c>-1, 0, +1</c>.
/// </para>
/// <para>
/// Compared to <see cref="TwoBitStorage"/>:
/// <list type="bullet">
///   <item><description>~5% bit waste (vs 33%) — better for serialization / on-wire formats.</description></item>
///   <item><description>Read/write requires modular arithmetic — slower per-trit than the 2-bit path.</description></item>
///   <item><description>Less SIMD-friendly because trits within a byte are not aligned to bit boundaries.</description></item>
/// </list>
/// Choose this strategy for storage-heavy workloads; choose <see cref="TwoBitStorage"/>
/// for compute-heavy workloads.
/// </para>
/// </remarks>
public readonly struct Base243Storage : ITritStorage
{
    // 3^0..3^4 — the divisors for the five trit slots within a byte.
    private static ReadOnlySpan<byte> Pow3 => [1, 3, 9, 27, 81];

    /// <inheritdoc/>
    public int BitsPerTrit => 2; // 8/5 ≈ 1.6, rounded up for size-hint purposes.

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int BytesFor(int tritCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tritCount);
        return (tritCount + 4) / 5;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadTrit(ReadOnlySpan<byte> buffer, int tritIndex)
    {
        (int byteIndex, int slot) = Math.DivRem(tritIndex, 5);
        uint packed = buffer[byteIndex];
        uint encoded = (packed / Pow3[slot]) % 3;
        return (int)encoded - 1;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteTrit(Span<byte> buffer, int tritIndex, int value)
    {
        if ((uint)(value + 1) > 2u)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value,
                "Trit values must be -1, 0, or +1.");
        }

        (int byteIndex, int slot) = Math.DivRem(tritIndex, 5);
        uint packed = buffer[byteIndex];
        uint divisor = Pow3[slot];
        uint current = (packed / divisor) % 3;

        packed -= current * divisor;
        packed += (uint)(value + 1) * divisor;

        buffer[byteIndex] = (byte)packed;
    }
}
