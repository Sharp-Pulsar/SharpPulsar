
namespace SharpPulsar.Table
{
    public readonly record struct TableData
    {
        public static readonly TableData Instance = new TableData();
    }
    public readonly record struct TableDataSize
    {
        public static readonly TableDataSize Instance = new TableDataSize();
    }
    public readonly record struct TableDataEmpty
    {
        public static readonly TableDataEmpty Instance = new TableDataEmpty();
    }
    public readonly record struct TableDataKey(string Key);
    public readonly record struct TableDataGet(string Key);
    public readonly record struct TableDataEntrySet
    {
        public static readonly TableDataEntrySet Instance = new TableDataEntrySet();
    }
    public readonly record struct TableDataKeySet
    {
        public static readonly TableDataKeySet Instance = new TableDataKeySet();
    }
    public readonly record struct TableDataValues
    {
        public static readonly TableDataValues Instance = new TableDataValues();
    }
    public readonly record struct RefeshData
    {

        public static readonly RefeshData Instance = new RefeshData();
    }
    
}
