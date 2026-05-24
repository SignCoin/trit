using Ternary.Encoding;

namespace Ternary.Tests.Encoding;

public class TwoBitStorageTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(4, 1)]
    [InlineData(5, 2)]
    [InlineData(8, 2)]
    [InlineData(9, 3)]
    public void BytesFor_rounds_up_per_four_trits(int tritCount, int expectedBytes)
    {
        var storage = default(TwoBitStorage);
        Assert.Equal(expectedBytes, storage.BytesFor(tritCount));
    }

    [Fact]
    public void BytesFor_throws_on_negative()
    {
        var storage = default(TwoBitStorage);
        Assert.Throws<ArgumentOutOfRangeException>(() => storage.BytesFor(-1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Round_trip_single_trit_at_each_slot(int value)
    {
        var storage = default(TwoBitStorage);
        // Use 8 trits = 2 bytes, exercise both byte and bit-shift indexing.
        Span<byte> buffer = stackalloc byte[storage.BytesFor(8)];
        for (var i = 0; i < 8; i++)
        {
            buffer.Clear();
            storage.WriteTrit(buffer, i, value);
            Assert.Equal(value, storage.ReadTrit(buffer, i));
        }
    }

    [Fact]
    public void Round_trip_full_sequence_independent_slots()
    {
        var storage = default(TwoBitStorage);
        int[] values = [-1, 0, 1, -1, 0, 1, -1, 0];
        Span<byte> buffer = stackalloc byte[storage.BytesFor(values.Length)];

        for (var i = 0; i < values.Length; i++)
        {
            storage.WriteTrit(buffer, i, values[i]);
        }

        for (var i = 0; i < values.Length; i++)
        {
            Assert.Equal(values[i], storage.ReadTrit(buffer, i));
        }
    }

    [Fact]
    public void WriteTrit_rejects_out_of_range_value()
    {
        var storage = default(TwoBitStorage);
        // Heap allocation here because Span<T> cannot escape a lambda body.
        byte[] buffer = new byte[1];
        Assert.Throws<ArgumentOutOfRangeException>(() => storage.WriteTrit(buffer, 0, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => storage.WriteTrit(buffer, 0, -2));
    }
}

public class Base243StorageTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(5, 1)]
    [InlineData(6, 2)]
    [InlineData(10, 2)]
    [InlineData(11, 3)]
    public void BytesFor_rounds_up_per_five_trits(int tritCount, int expectedBytes)
    {
        var storage = default(Base243Storage);
        Assert.Equal(expectedBytes, storage.BytesFor(tritCount));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Round_trip_single_trit_at_each_slot(int value)
    {
        var storage = default(Base243Storage);
        // 10 trits = 2 bytes, exercises slot 0..4 in both bytes.
        Span<byte> buffer = stackalloc byte[storage.BytesFor(10)];
        for (var i = 0; i < 10; i++)
        {
            buffer.Clear();
            storage.WriteTrit(buffer, i, value);
            Assert.Equal(value, storage.ReadTrit(buffer, i));
        }
    }

    [Fact]
    public void Round_trip_full_sequence_independent_slots()
    {
        var storage = default(Base243Storage);
        int[] values = [-1, 0, 1, -1, 0, 1, -1, 0, 1, -1];
        Span<byte> buffer = stackalloc byte[storage.BytesFor(values.Length)];

        for (var i = 0; i < values.Length; i++)
        {
            storage.WriteTrit(buffer, i, values[i]);
        }

        for (var i = 0; i < values.Length; i++)
        {
            Assert.Equal(values[i], storage.ReadTrit(buffer, i));
        }
    }

    [Fact]
    public void WriteTrit_rejects_out_of_range_value()
    {
        var storage = default(Base243Storage);
        // Heap allocation here because Span<T> cannot escape a lambda body.
        byte[] buffer = new byte[1];
        Assert.Throws<ArgumentOutOfRangeException>(() => storage.WriteTrit(buffer, 0, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => storage.WriteTrit(buffer, 0, -2));
    }
}
