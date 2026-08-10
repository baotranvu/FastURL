using System;
using System.Text;

namespace FastUrl.Domain.Common
{
    public class Base62Codec : IShortCodeCodec
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly int Base = Alphabet.Length; // 62

        // TỐI ƯU 1: Bảng tra cứu O(1) ASCII Direct-Address Map
        private static readonly int[] AsciiMap = InitializeAsciiMap();

        private static int[] InitializeAsciiMap()
        {
            var map = new int[128];
            Array.Fill(map, -1);

            for (int i = 0; i < Alphabet.Length; i++)
            {
                map[Alphabet[i]] = i;
            }

            return map;
        }

        /// <summary>
        /// Phương thức Encode chính của Interface IShortCodeCodec (Public Entry Point)
        /// </summary>
        public string Encode(long id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "ID must be a non-negative integer.");
            }

            if (id == 0)
            {
                return "0";
            }

            // Mặc định sử dụng phương thức High-Performance
            return EncodeStackAlloc(id);
        }


        /// <summary>
        /// PHIÊN BẢN 2 (Private): Mã hóa dùng stackalloc + Span (Tối ưu Zero-Allocation High Performance)
        /// </summary>
        private static string EncodeStackAlloc(long id)
        {
            Span<char> buffer = stackalloc char[11];
            int pos = 11;
            var tempId = id;

            while (tempId > 0)
            {
                int remainder = (int)(tempId % Base);
                buffer[--pos] = Alphabet[remainder];
                tempId /= Base;
            }

            return new string(buffer.Slice(pos));
        }

        /// <summary>
        /// Giải mã chuỗi Base62 thành số nguyên long ID (Áp dụng Quy tắc Horner & Bảng tra O(1) ASCII Map)
        /// </summary>
        public long Decode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            long result = 0;
            foreach (char c in code)
            {
                // TỐI ƯU 1: Tra cứu O(1) tức thì bằng mảng ASCII 128 phần tử thay vì IndexOf O(62)
                int value = (c < 128) ? AsciiMap[c] : -1;
                if (value == -1)
                {
                    throw new ArgumentException($"Invalid character '{c}' in base62 code.", nameof(code));
                }

                // TỐI ƯU 2: Phòng vệ tràn số long.MaxValue trước khi thực hiện nhân lũy thừa Horner
                if (result > (long.MaxValue - value) / Base)
                {
                    throw new OverflowException("The base62 code is too large and overflows long.MaxValue.");
                }

                // Quy tắc Horner nhân tích lũy (Tận dụng CPU instruction)
                result = result * Base + value;
            }

            return result;
        }
    }
}
