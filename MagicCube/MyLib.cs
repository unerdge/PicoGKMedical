namespace MyLib
{
    using static System.Math;
    public static class Math
    {
        public static IntVector2 Nomalize(this IntVector2 Vec)
        {
            double dis=Sqrt(Vec.x*Vec.x+Vec.y*Vec.y);
            return new IntVector2((int)Round(Vec.x/dis),(int)Round(Vec.y/dis));
        }
        public static bool InRange<T>(this T a,T min,T max) where T:IComparable<T>
        {
            if(a.CompareTo(min)<0) return false;
            if(a.CompareTo(max)>0) return false;
            return true;
        }
    }
}