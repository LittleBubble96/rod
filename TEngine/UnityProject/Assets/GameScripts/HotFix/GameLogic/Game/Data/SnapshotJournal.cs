using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GameLogic.Data
{
    /// <summary>平台无关双槽日志。仅覆写较旧槽；校验是损坏检测，不是反作弊。</summary>
    public sealed class SnapshotJournal
    {
        private const string Magic = "ROD-SNAPSHOT-1";
        private readonly Func<int, string> read;
        private readonly Action<int, string> write;
        private readonly Action<int> delete;
        private readonly object gate = new object();
        public SnapshotJournal(Func<int, string> read, Action<int, string> write, Action<int> delete)
        {
            this.read = read ?? throw new ArgumentNullException(nameof(read));
            this.write = write ?? throw new ArgumentNullException(nameof(write));
            this.delete = delete ?? throw new ArgumentNullException(nameof(delete));
        }

        private sealed class Entry { public long Revision; public string Payload; public int Slot; }
        private static string Hash(string text)
        {
            using (var hash = SHA256.Create())
                return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }

        private static Entry Parse(string raw, int slot)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            var fields = raw.Split('\n');
            if (fields.Length != 4 || fields[0] != Magic) return null;
            if (!long.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out var revision) || revision < 1) return null;
            if (fields[3] != Hash(fields[0] + "\n" + fields[1] + "\n" + fields[2])) return null;
            try
            {
                var payload = new UTF8Encoding(false, true).GetString(Convert.FromBase64String(fields[2]));
                if (string.IsNullOrWhiteSpace(payload)) return null;
                return new Entry { Revision = revision, Payload = payload, Slot = slot };
            }
            catch (FormatException) { return null; }
            catch (DecoderFallbackException) { return null; }
        }

        private Entry Latest()
        {
            // 读取异常不是“没有存档”，向上抛出，防止用新档覆盖未知旧数据。
            var a = read(0); var b = read(1);
            var x = Parse(a, 0); var y = Parse(b, 1);
            if (x == null && y == null && (!string.IsNullOrEmpty(a) || !string.IsNullOrEmpty(b)))
                throw new InvalidDataException("两个本地存档槽均损坏或版本不支持；原数据已保留。");
            return x == null ? y : y == null || x.Revision >= y.Revision ? x : y;
        }

        public string Read() { lock (gate) return Latest()?.Payload; }
        public void Write(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("不允许写入空快照。", nameof(payload));
            lock (gate)
            {
                var current = Latest();
                long revision = checked((current?.Revision ?? 0) + 1);
                int target = current == null ? 0 : 1 - current.Slot;
                string body = Magic + "\n" + revision.ToString(CultureInfo.InvariantCulture) + "\n" +
                              Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
                write(target, body + "\n" + Hash(body));
                var verified = Parse(read(target), target);
                if (verified == null || verified.Revision != revision || verified.Payload != payload)
                    throw new IOException("本地存档写后校验失败，上一有效槽仍保留。");
            }
        }

        /// <summary>仅供用户主动重置档案调用，不在损坏恢复时调用。</summary>
        public void DeleteAll() { lock (gate) { delete(0); delete(1); } }
    }
}
