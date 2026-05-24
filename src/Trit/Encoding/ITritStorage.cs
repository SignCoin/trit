namespace Ternary.Encoding;

/// <summary>
/// Strategy for packing balanced ternary digits into a byte buffer.
/// </summary>
/// <remarks>
/// <para>
/// Implementations <b>must</b> be value types (<see langword="struct"/>). The
/// type is consumed via generic-struct constraints (e.g. <c>where TStorage : struct, ITritStorage</c>)
/// so that the JIT generates a separate specialization for each storage type
/// and devirtualizes every call to <see cref="ReadTrit"/> / <see cref="WriteTrit"/>.
/// A reference-type implementation would force virtual dispatch and defeat
/// the perf strategy described in ADR 0001.
/// </para>
/// <para>
/// Implementations <b>must</b> be stateless. Generic-struct specialization uses
/// <c>default(TStorage)</c> as the receiver of every call, so any per-instance
/// state would be silently discarded.
/// </para>
/// </remarks>
public interface ITritStorage
{
    /// <summary>Number of bits an encoding consumes per trit, used for size hints.</summary>
    /// <remarks>
    /// For non-power-of-two encodings (e.g. base-243 at <c>8/5</c> bits per trit)
    /// this is the rounded-up integer value.
    /// </remarks>
    int BitsPerTrit { get; }

    /// <summary>Minimum number of bytes required to hold <paramref name="tritCount"/> trits.</summary>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="tritCount"/> is negative.</exception>
    int BytesFor(int tritCount);

    /// <summary>Read the trit at <paramref name="tritIndex"/>; result is one of <c>{-1, 0, +1}</c>.</summary>
    int ReadTrit(ReadOnlySpan<byte> buffer, int tritIndex);

    /// <summary>Write <paramref name="value"/> (one of <c>{-1, 0, +1}</c>) into <paramref name="buffer"/> at <paramref name="tritIndex"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in <c>{-1, 0, +1}</c>.</exception>
    void WriteTrit(Span<byte> buffer, int tritIndex, int value);
}
