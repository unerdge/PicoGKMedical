using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;

namespace TestHello
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Reflection.Emit;
    using System.Text.RegularExpressions;
    using static System.Math;
    using MyLib;
    // using static MyLib.Math;
    // using static MyLib.IntVector2;
    class MagicCube
    {
        public enum ScanOrder{DiagParellel,CenterFirst,LineFirst};
        private ScanOrder scanOrder;
        public int DIM;
        int average,posibleCubeCount=0;
        private bool [,] UsedPos;
        private bool [] UsedNum;
        private int? [,] Cube;
        public MagicCube(int dim,ScanOrder _scanOrder=ScanOrder.CenterFirst)
        {
            scanOrder=_scanOrder;
            if(!(dim<3||dim%2==0)) DIM=dim;
            else DIM=3;
            UsedPos=new bool [DIM,DIM];
            UsedNum=new bool [DIM*DIM];
            Cube=new int? [DIM,DIM];//null还没被填，
            average=(DIM*DIM+1)*DIM/2;
            for(int i = 0; i < DIM; i++)
            {
                for(int j = 0; j < DIM; j++)
                {
                    UsedPos[i,j]=false;
                    UsedNum[i+j*DIM]=false;
                    Cube[i,j]=null;
                }
            }
        }
        private (int x,int y) CenterFirstNextPos(int x,int y)
        {
            if(x==DIM/2&&y<=DIM/2) return (x-1,y-1);
            else
            {
                int dis=Max(Abs(x - DIM / 2),Abs(y - DIM / 2));
                IntVector2 dir=new IntVector2(Sign((x - DIM / 2)/dis),Sign((y - DIM / 2)/dis));
                IntVector2 rotated=new IntVector2(dir.y,-dir.x);//anticlockwise rotate right angle
                if (dir.x != 0 && dir.y != 0)
                {
                    IntVector2 NomalizedD = (rotated - dir).Nomalize();
                    if((rotated-dir).x==0) return (x,y+ NomalizedD.y);
                    else return (x+ NomalizedD.x,y);
                }
                IntVector2 Nomalized=rotated.Nomalize();
                return (x+ Nomalized.x,y+ Nomalized.y);
            }
        }
        private (int x,int y) DiagParellelNextPos(int x,int y)
        {
            if (x == DIM - 1) return (x+y+1-(DIM-1),DIM-1);
            else if (y == 0) return (0,x+y+1-0);
            else return (x+1,y-1);
        }
        private (int x,int y) LineFirstNextPos(int x,int y)
        {
            if(x==DIM-1) return (0,y+1);
            else return (x+1,y);
        }
        public (int x,int y) NextPos(int x,int y,ScanOrder? s=null)
        {
            if(s==null) s=scanOrder;
            switch (s)
            {
                case ScanOrder.CenterFirst:
                return CenterFirstNextPos(x,y);
                case ScanOrder.DiagParellel:
                return DiagParellelNextPos(x,y);
                case ScanOrder.LineFirst:
                default:
                return LineFirstNextPos(x,y);
            }
        }
        void Print(bool WetherCount=true)
        {
            if(WetherCount==true) posibleCubeCount++;
            Console.WriteLine($"{posibleCubeCount}:");
            for(int i = 0; i < DIM; i++)
            {
                for(int j = 0; j < DIM; j++)
                {
                    if(Cube[i,j]!=null) Console.Write($"{Cube[i,j]} ");
                    else Console.Write($"n ");
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }
        private delegate (int x,int y)Index(int i);
        private (int x,int y)Diag(int i)
        {
            return (i,i);
        }
        private (int x,int y)AntiDiag(int i)
        {
            return (i,DIM-1-i);
        }
        private (int x,int y)Row(int i)
        {
            return (0,i);
        }
        private (int x,int y)Line(int i)
        {
            return (i,0);
        }
        private bool CheckSum(Index index,int? x=null,int? y=null)
        {
            int? Sum=0;
            for(int i = 0; i < DIM; i++)
            {
                Sum+=Cube[x==null?index(i).x:(int)x,y==null?index(i).y:(int)y];
            }
            return Sum==average||Sum==null;
        }
        private bool CenterFirstCheckSum(int x,int y)
        {
            bool flag=true;
            int dis=Max(Abs(x - DIM / 2),Abs(y - DIM / 2));
            if(dis==DIM / 2)
            {
                flag&=CheckSum(Line,y:y);
                flag&=CheckSum(Row,x:x);
            }
            if (x == DIM - 1 && y == DIM - 1)
            {
                flag&=CheckSum(Diag);
            }
            if (x == DIM - 1 && y == 0)
            {
                flag&=CheckSum(AntiDiag);
            }
            return flag;
        }
        private bool DiagParellelCheckSum(int x,int y)
        {
            bool flag=true;
            if (x == DIM - 1 && y == DIM - 1)
            {
                flag&=CheckSum(Diag);
            }
            else if (x == DIM-1 && y == 0)
            {
                flag&=CheckSum(AntiDiag);
            }
            if (x == DIM - 1)
            {
                flag&=CheckSum(Line,y:y);
            }
            if (y == DIM - 1)
            {
                flag&=CheckSum(Row,x:x);
            }
            return flag;
        }
        private bool LineFirstCheckSum(int x,int y)
        {
            bool flag=true;
            if (x == DIM - 1)
            {
                flag&=CheckSum(Line,y:y);
            }
            if (y == DIM - 1)
            {
                flag&=CheckSum(Row,x:x);
                if (x == 0)
                {
                    flag&=CheckSum(AntiDiag);
                }
                else if (x == DIM - 1)
                {
                    flag&=CheckSum(Diag);
                }
            }
            return flag;
        }
        private bool CheckSum(int x,int y)
        {
            switch (scanOrder)
            {
                case ScanOrder.CenterFirst:
                return CenterFirstCheckSum(x,y);
                case ScanOrder.DiagParellel:
                return DiagParellelCheckSum(x,y);
                case ScanOrder.LineFirst:
                default:
                return LineFirstCheckSum(x,y);
            }
        }
        public void Search(int x,int y)//深度优先搜索
        {
            if (!(x.InRange(0,DIM-1) && y.InRange(0, DIM - 1)))
            {
                Print();
                return;
            }
            for(int num = 0; num < DIM * DIM; num++)
            {
                if (UsedNum[num] == false)
                {
                    UsedNum[num]=true;
                    Cube[x,y]=num+1;
                    if(CheckSum(x,y)) Search(NextPos(x,y).x,NextPos(x,y).y);
                    Cube[x,y]=null;
                    UsedNum[num]=false;
                }
            }
        }
        public void Search()
        {
            switch (scanOrder){
                case ScanOrder.CenterFirst:
                Search(DIM/2,DIM/2);
                break;
                case ScanOrder.DiagParellel:
                case ScanOrder.LineFirst:
                default:
                Search(0,0);
                break;
            }
        }
    }
}

