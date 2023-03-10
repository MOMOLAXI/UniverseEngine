namespace Universe
{
    public enum VarType : byte
    {
        None, // 空类型
        Bool, // 布尔
        Int32, // 32位整数
        Int64, // 64位整数
        Float, // 单精度浮点数
        Double, // 双精度浮点数
        String, // 字符串
        WideStr, // 宽字符串
        Object, // 对象号
        UserData, // 用户自定义数据，对象引用
        Binary, // 二进制数据

        Max,
    }

    internal enum VarFlag : byte
    {
        None,
        StringInContainer,
    }
}