namespace Ternary.Encoding;

/// <summary>
/// Two-bit-per-trit encoding. Each byte holds four trits.
/// </summary>
/// <remarks>
/// <para>
/// Bit layout (LSB-first within a byte):
/// <code>
///   bit 7  6 | 5  4 | 3  2 | 1  0
///   ---------+------+------+------
///   trit  3  | trit 2 | trit 1 | trit 0
/// </code>
/// </para>
/// <para>
/// Each two-bit slot encodes:
/// <list type="bullet">
///   <item><description><c>0b00</c> → <c>-1</c></description></item>
///   <item><description><c>0b01</c> → <c>0</c></description></item>
///   <item><description><c>0b10</c> → <c>+1</c></description></item>
///   <item><description><c>0b11</c> → reserved (invalid; never produced by <see cref="WriteTrit"/>).</description></item>
/// </list>
/// 33% bit waste vs. <see cref="Base243Storage"/>, but every read/write is a
/// trivial shift/mask sequence — SIMD-friendly and branch-free in the hot path.
/// </para>
/// </remarks>
public readonly struct TwoBitStorage : ITritStorage
{
    /// <inheritdoc/>
    public int BitsPerTrit => 2;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int BytesFor(int tritCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tritCount);
        return (tritCount + 3) >> 2;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadTrit(ReadOnlySpan<byte> buffer, int tritIndex)
    {
        int byteIndex = tritIndex >> 2;
        int bitShift = (tritIndex & 0b11) << 1;
        int raw = (buffer[byteIndex] >> bitShift) & 0b11;
        return raw - 1; // 0→-1, 1→0, 2→+1
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

        int byteIndex = tritIndex >> 2;
        int bitShift = (tritIndex & 0b11) << 1;
        int slotMask = 0b11 << bitShift;
        int slotBits = (value + 1) << bitShift;

        buffer[byteIndex] = (byte)((buffer[byteIndex] & ~slotMask) | slotBits);
    }
}
