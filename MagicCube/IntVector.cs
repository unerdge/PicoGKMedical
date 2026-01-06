using System.Security.Cryptography.X509Certificates;

namespace MyLib
{
    public struct IntVector2
    {
        public int x,y;
        public IntVector2(int X,int Y)
        {
            x=X;
            y=Y;
        }
        // public static bool operator ==(IntVector2 a,IntVector2 b)=>a.x==b.x&&a.y==b.y;
        // public static bool operator !=(IntVector2 a,IntVector2 b)=>!(a.x==b.x&&a.y==b.y);
        public static IntVector2 operator +(IntVector2 a)=>a;
        public static IntVector2 operator -(IntVector2 a)=>new IntVector2(-a.x,-a.y);
        public static IntVector2 operator +(IntVector2 a,IntVector2 b)=>new IntVector2(a.x+b.x,a.y+b.y);
        public static IntVector2 operator -(IntVector2 a,IntVector2 b)=>a+(-b);

        public static int operator *(IntVector2 a,IntVector2 b)=>a.x*b.x+a.y*b.y;
    }
}