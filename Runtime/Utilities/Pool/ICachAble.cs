namespace Universe
{
    public interface ICacheAble
    {
        bool IsInCache { get; set; }
        void Reset();
    }
}