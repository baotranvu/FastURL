using System;

namespace FastUrl.Domain.Common;

/// <summary>
/// Bộ sinh ID 64-bit chuẩn Twitter Snowflake ID (Tùy chỉnh cho dự án: 3-bits Worker, 19-bits Sequence):
/// Bit Layout: [1-bit Sign (0)] [41-bits Timestamp (ms)] [3-bits Worker ID] [19-bits Sequence]
/// Supported Workers: 8 Nodes (0..7)
/// Supported Sequence: 524,288 IDs / ms / node
/// </summary>
public class SnowflakeIdGenerator
{
    // Custom Epoch: 2026-01-01T00:00:00Z (Giúp tiết kiệm bit timestamp trong 69 năm)
    private static readonly long CustomEpoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

    private const int WorkerIdBits = 3;   // 3 bits = 8 Workers (0..7)
    private const int SequenceBits = 19;  // 19 bits = 524,288 IDs/ms

    private const long MaxWorkerId = (1L << WorkerIdBits) - 1; // Max 7
    private const long MaxSequence = (1L << SequenceBits) - 1; // Max 524,287

    private const int WorkerIdShift = SequenceBits; // 19 bits
    private const int TimestampShift = SequenceBits + WorkerIdBits; // 22 bits

    private readonly long _workerId;
    private readonly object _lock = new();

    private long _lastTimestamp = -1L;
    private long _sequence = 0L;

    public SnowflakeIdGenerator(long workerId = 1)
    {
        if (workerId < 0 || workerId > MaxWorkerId)
        {
            throw new ArgumentOutOfRangeException(nameof(workerId), $"WorkerId must be between 0 and {MaxWorkerId}.");
        }

        _workerId = workerId;
    }

    public long NextId()
    {
        lock (_lock)
        {
            long currentTimestamp = GetCurrentTimestamp();

            if (currentTimestamp < _lastTimestamp)
            {
                throw new InvalidOperationException($"Clock moved backwards. Refusing to generate id for {_lastTimestamp - currentTimestamp} milliseconds.");
            }

            if (_lastTimestamp == currentTimestamp)
            {
                _sequence = (_sequence + 1) & MaxSequence;
                if (_sequence == 0)
                {
                    // Trượt chuỗi vượt quá 524,287 ID trong 1ms -> Đợi tới ms tiếp theo
                    currentTimestamp = WaitNextMillisecond(_lastTimestamp);
                }
            }
            else
            {
                _sequence = 0L;
            }

            _lastTimestamp = currentTimestamp;

            return ((currentTimestamp - CustomEpoch) << TimestampShift)
                 | (_workerId << WorkerIdShift)
                 | _sequence;
        }
    }

    private static long GetCurrentTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private static long WaitNextMillisecond(long lastTimestamp)
    {
        long timestamp = GetCurrentTimestamp();
        while (timestamp <= lastTimestamp)
        {
            timestamp = GetCurrentTimestamp();
        }
        return timestamp;
    }
}
