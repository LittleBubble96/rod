using GameLogic.Data;
using WeChatWASM;
namespace GameLogic
{
    // 微信本地Storage双槽，同步提交用于关键节点及退出兜底。
    public class WeChatReadCacheFileSystem : IReadCacheFileSystem
    {
        private readonly string key;
        private readonly SnapshotJournal journal;
        public WeChatReadCacheFileSystem(string key = "gameCache")
        {
            this.key = key;
            journal = new SnapshotJournal(
                slot => WXBase.StorageGetStringSync(SlotKey(slot), ""),
                (slot, value) => WXBase.StorageSetStringSync(SlotKey(slot), value),
                slot => WX.RemoveStorageSync(SlotKey(slot)));
        }
        private string SlotKey(int slot) => key + (slot == 0 ? ".a" : ".b");
        public string ReadCache() => journal.Read() ?? WXBase.StorageGetStringSync(key, "");
        public void WriteCache(string json) => journal.Write(json);
        public void DeleteAll() { journal.DeleteAll(); WX.RemoveStorageSync(key); }
    }
}
