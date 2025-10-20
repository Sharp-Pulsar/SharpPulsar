
namespace SharpPulsar.Table
{
    public record TableData
    {
        public static readonly TableData Instance = new TableData();
    }
    public record TableDataSize
    {
        public static readonly TableDataSize Instance = new TableDataSize();
    }
    public record TableDataEmpty
    {
        public static readonly TableDataEmpty Instance = new TableDataEmpty();
    }
    public record TableDataKey(string Key);
    public record TableDataGet(string Key);
    public record TableDataEntrySet
    {
        public static readonly TableDataEntrySet Instance = new TableDataEntrySet();
    }
    public record TableDataKeySet
    {
        public static readonly TableDataKeySet Instance = new TableDataKeySet();
    }
    public record TableDataValues
    {
        public static readonly TableDataValues Instance = new TableDataValues();
    }
    public record RefeshData
    {

        public static readonly RefeshData Instance = new RefeshData();
    }
    
}
