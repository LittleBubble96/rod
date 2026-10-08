using System.IO;
using System.Text;
using GameLogic.Data;
using UnityEngine;
namespace GameLogic
{
    // 桌面/移动本地双槽；保留旧单文件供首次迁移读取。
    public class WindowReadCacheFileSystem : IReadCacheFileSystem
    {
        private readonly string legacyPath;
        private readonly SnapshotJournal journal;
        public WindowReadCacheFileSystem(string fileName = "game_cache.json")
        {
            legacyPath = Path.Combine(Application.persistentDataPath, fileName);
            journal = new SnapshotJournal(ReadSlot, WriteSlot, DeleteSlot);
        }
        private string SlotPath(int slot) => legacyPath + (slot == 0 ? ".a" : ".b");
        private string ReadSlot(int slot) => File.Exists(SlotPath(slot)) ? File.ReadAllText(SlotPath(slot), Encoding.UTF8) : null;
        private void WriteSlot(int slot, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(legacyPath));
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (var stream = new FileStream(SlotPath(slot), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
        }
        private void DeleteSlot(int slot) { if (File.Exists(SlotPath(slot))) File.Delete(SlotPath(slot)); }
        public string ReadCache() => journal.Read() ?? (File.Exists(legacyPath) ? File.ReadAllText(legacyPath, Encoding.UTF8) : null);
        public void WriteCache(string json) => journal.Write(json);
        public void DeleteAll() { journal.DeleteAll(); if (File.Exists(legacyPath)) File.Delete(legacyPath); }
    }
}
