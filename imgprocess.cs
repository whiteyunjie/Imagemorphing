using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;


namespace Imagemorphing
{
    class imgprocess
    {
        Bitmap bmp;
        Bitmap orig;    //原图
        double centralx;
        double centraly;
        double availabler;
        double hei;
        double wid;
        double[,] K;    //原控制点矩阵
        double[,] P;
        double[,] YY;
        double[,] XX;    //系数解
        double[,] L;
        public double[,] LT;
        int kpn;          //关键点坐标数
        double lambda = 100; //正则项
        public void creatimage(string path)
        {
            bmp = new Bitmap(path);
            orig = new Bitmap(bmp);
            centralx = (bmp.Width - 1) / 2;
            centraly = (bmp.Height - 1) / 2;
            availabler = Math.Min(centralx, centraly);
            hei = bmp.Height;
            wid = bmp.Width; 
        }

        public double[] rotatept(int x, int y, double angle, double radius)//这里必须转化成弧度制
        {
            double[] tfpt = new double[2];
            double x0 = x - centralx;
            double y0 = centraly - y;
            double a;
            double dis = Math.Sqrt(Math.Pow((double)x - centralx, 2) + Math.Pow((double)y - centraly, 2));
            a = angle * (radius - dis)/radius;
            tfpt[0] = x0 * Math.Cos(a) - y0 * Math.Sin(a) + centralx;
            tfpt[1] = centraly - (x0 * Math.Sin(a) + y0 * Math.Cos(a));
            return tfpt;
        }
        public double[] distf1(int x, int y, double radius)//图像凹变(枕形畸变)
        {
            double[] tfpt = new double[2];
            double x0 = x - centralx;
            double y0 = y - centraly;
            double dis = Math.Sqrt(Math.Pow(x0, 2) + Math.Pow(y0, 2));
            tfpt[0] = (Math.Asin(dis / radius) * 2 * radius / (Math.PI * dis)) * x0 + centralx;
            tfpt[1] = (Math.Asin(dis / radius) * 2 * radius / (Math.PI * dis)) * y0 + centralx;
            return tfpt;
        }
        public double[] distf2(int x, int y, double radius)//图像凸变(桶形畸变)
        {
            double[] tfpt = new double[2];
            double x0 = x - centralx;
            double y0 = y - centraly;
            double dis = Math.Sqrt(Math.Pow(x0, 2) + Math.Pow(y0, 2));
            tfpt[0] = x0 / (Math.Asin(dis / radius) *  radius / dis) + centralx;
            tfpt[1] = y0 / (Math.Asin(dis / radius) *  radius / dis) + centralx;
            return tfpt;
        }

        //最近邻插值
        public Color nearestneighbour(double x, double y)//转换到原图上的坐标
        {
            //加0.5显式转换为整型得到的坐标就是离该坐标最近的
            int nnx = (int)(x + 0.5);
            int nny = (int)(y + 0.5);
            Color c = new Color();
            if (nnx < 0 || nny < 0 || nnx >= wid || nny >= hei)
                c = Color.FromArgb(0, 0, 0);
            else
            {
                c = orig.GetPixel(nnx, nny);
            }
            return c;
        }
        //双线性插值
        public Color bilinear(double x, double y)
        {
            int x1 = (int)x;
            int y1 = (int)y;
            int x2 = (int)(x + 1);
            int y2 = (int)(y + 1);
            int R, G, B;
            Color f11, f12, f21, f22;
            int f1, f2;
            if (x1 < 0 || x1 >= wid || y1 < 0 || y1 >= hei) f11 = Color.FromArgb(0, 0, 0);
            else
            {
                f11 = orig.GetPixel(x1, y1);
            }
            if (x1 < 0 || x1 >= wid || y2 < 0 || y2 >= hei) f12 = Color.FromArgb(0, 0, 0);
            else
            {
                f12 = orig.GetPixel(x1, y2);
            }
            if (x2 < 0 || x2 >= wid || y1 < 0 || y1 >= hei) f21 = Color.FromArgb(0, 0, 0);
            else
            {
                f21 = orig.GetPixel(x2, y1);
            }
            if (x2 < 0 || x2 >= wid || y2 < 0 || y2 >= hei) f22 = Color.FromArgb(0, 0, 0);
            else
            {
                f22 = orig.GetPixel(x2, y2);
            }
            //R
            f1 = (int)((x - x1) * f21.R / (x2 - x1) + (x - x2) * f11.R / (x1 - x2));
            f2 = (int)((x - x1) * f22.R / (x2 - x1) + (x - x2) * f12.R / (x1 - x2));
            R = (int)((y - y1) * f2 / (y2 - y1) + (y - y2) * f1 / (y1 - y2));
            //G
            f1 = (int)((x - x1) * f21.G / (x2 - x1) + (x - x2) * f11.G / (x1 - x2));
            f2 = (int)((x - x1) * f22.G / (x2 - x1) + (x - x2) * f12.G / (x1 - x2));
            G = (int)((y - y1) * f2 / (y2 - y1) + (y - y2) * f1 / (y1 - y2));
            //B
            f1 = (int)((x - x1) * f21.B / (x2 - x1) + (x - x2) * f11.B / (x1 - x2));
            f2 = (int)((x - x1) * f22.B / (x2 - x1) + (x - x2) * f12.B / (x1 - x2));
            B = (int)((y - y1) * f2 / (y2 - y1) + (y - y2) * f1 / (y1 - y2));
            if (R > 255) R = 255;
            if (R < 0) R = 0;
            if (G > 255) G = 255;
            if (G < 0) G = 0;
            if (B > 255) B = 255;
            if (B < 0) B = 0;
            Color c = Color.FromArgb(R, G, B);
            return c;
        }
        //双三次插值
        public Color bicubic(double x, double y)
        {
            //对于边界情况的考虑
            int m = (int)x;
            int n = (int)y;
            double r = 0, b = 0, g = 0;
            double u = x - m;
            double v = y - n;
            double[] A = new double[4];
            double[] B = new double[4];
            double[] rer = new double[4];
            double[] reg = new double[4];
            double[] reb = new double[4];
            for (int i = 0; i < 4; i++)
            {
                A[i] = Sval(u - i + 1);
                B[i] = Sval(v - i + 1);
            }
            //另一种思路，或者将C先写成矩阵
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (m + i - 1 < 0 || n + j - 1 < 0 || m + i - 1 >= wid || n + j - 1 >= hei)
                    {
                        rer[i] = rer[i] + 0;   //边界问题处理
                    }
                    else
                    {
                        rer[i] = rer[i] + (A[j] * (int)orig.GetPixel(m + i - 1, n + j - 1).R);
                    }
                }
            for (int i = 0; i < 4; i++)
            {
                r += rer[i] * B[i];
            }
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (m + i - 1 < 0 || n + j - 1 < 0 || m + i - 1 >= wid || n + j - 1 >= hei)
                    {
                        reg[i] = reg[i] + 0;   //边界问题处理
                    }
                    else
                    {
                        reg[i] = reg[i] + (A[j] * (int)orig.GetPixel(m + i - 1, n + j - 1).G);
                    }
                }
            for (int i = 0; i < 4; i++)
            {
                g += reg[i] * B[i];
            }
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (m + i - 1 < 0 || n + j - 1 < 0 || m + i - 1 >= wid || n + j - 1 >= hei)
                    {
                        reb[i] = reb[i] + 0;   //边界问题处理
                    }
                    else
                    {
                        reb[i] = reb[i] + (A[j] * (int)orig.GetPixel(m + i - 1, n + j - 1).B);
                    }
                }
            for (int i = 0; i < 4; i++)
            {
                b += reb[i] * B[i];
            }

            if (r > 255) r = 255;
            if (r < 0) r = 0;
            if (g > 255) g = 255;
            if (g < 0) g = 0;
            if (b > 255) b = 255;
            if (b < 0) b = 0;
            Color c = Color.FromArgb((int)r, (int)g, (int)b);
            return c;
        }
        public double Sval(double x)//双三次插值基函数
        {
            double S = 0;
            double absx = Math.Abs(x);
            if (absx <= 1)
            {
                S = 1 - 2 * Math.Pow(absx, 2) + Math.Pow(absx, 3);
                return S;
            }
            if (absx > 1 && absx < 2)
            {
                S = 4 - 8 * absx + 5 * Math.Pow(absx, 2) - Math.Pow(absx, 3);
                return S;
            }
            return S;
        }
        public void rotationtf(double angle, double radius, int method)//图像旋转
        {
            Color c;
            if (method == 0)
            {
                for (int i = 0; i < bmp.Width; i++)
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        if (Math.Sqrt(Math.Pow((double)i - centralx, 2) + Math.Pow((double)j - centraly, 2)) <= radius) //在旋转半径之外的点无需计算
                        {
                            double[] tfpt = rotatept(i, j, angle, radius);
                            c = nearestneighbour(tfpt[0], tfpt[1]);
                        }
                        else
                        {
                            c = orig.GetPixel(i, j);
                        }
                        bmp.SetPixel(i, j, c);
                    }
            }
            else if (method == 1)
            {
                for (int i = 0; i < bmp.Width; i++)
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        if (Math.Sqrt(Math.Pow((double)i - centralx, 2) + Math.Pow((double)j - centraly, 2)) <= radius)
                        {
                            double[] tfpt = rotatept(i, j, angle, radius);
                            c = bilinear(tfpt[0], tfpt[1]);
                        }
                        else
                        {
                            c = orig.GetPixel(i, j);
                        }
                        bmp.SetPixel(i, j, c);
                    }
            }
            else
            {
                for (int i = 0; i < bmp.Width; i++)
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        if (Math.Sqrt(Math.Pow((double)i - centralx, 2) + Math.Pow((double)j - centraly, 2)) <= radius)
                        {
                            double[] tfpt = rotatept(i, j, angle, radius);
                            c = bicubic(tfpt[0], tfpt[1]);
                        }
                        else
                        {
                            c = orig.GetPixel(i, j);
                        }
                        bmp.SetPixel(i, j, c);
                    }
            }
        }

        public void distortion(double radius,int method,int type)//畸变
        {
            Color c;
            if (type == 1)
            {
                if (method == 0)
                {
                    for (int i = 0; i < bmp.Width; i++)
                        for (int j = 0; j < bmp.Height; j++)
                        {
                            if (Math.Sqrt(Math.Pow((double)i - centralx, 2) + Math.Pow((double)j - centraly, 2)) <= radius)
                            {
                                double[] tfpt = distf1(i, j, radius);
                                c = nearestneighbour(tfpt[0], tfpt[1]);
                            }
                            else
                            {
                                c = orig.GetPixel(i, j);
                            }
                            bmp.SetPixel(i, j, c);
                        }
                }
                else if (method == 1)
                {
                    for (int i = 0; i < bmp.Width; i++)
                        for (int j = 0; j < bmp.Height; j++)
                        {
                            if (Math.Sqrt(Math.Pow((double)i - centralx, 2) + Math.Pow((double)j - centraly, 2)) <= radius)
                            {
                                double[] tfpt = distf1(i, j,radius);
                                c = bilinear(tfpt[0], tfpt[1]);
                            }
                            else
                            {
                                c = orig.GetPixel(i, j);
                            }
                            bmp.SetPixel(i, j, c);
                        }
                }
                else
                {
                    for (int i = 0; i < bmp.Width; i++)
                        for (int j = 0; j < bmp.Height; j++)
                        {
                            if (Math.Sqrt(Math.Pow((double)i - centralx, 2) + Math.Pow((double)j - centraly, 2)) <= radius)
                            {
                                double[] tfpt = distf1(i, j, radius);
                                c = bicubic(tfpt[0], tfpt[1]);
                            }
                            else
                            {
                                c = orig.GetPixel(i, j);
                            }
                            bmp.SetPixel(i, j, c);
                        }
                }
            }
            else
            {
                if (method == 0)
                {
                    for (int i = 0; i < bmp.Width; i++)
                        for (int j = 0; j < bmp.Height; j++)
                        {
                            if (Math.Sqrt(Math.Pow((double)i - centralx, 2) + Math.Pow((double)j - centraly, 2)) <= radius)
                            {
                                double[] tfpt = distf2(i, j, radius);
                                c = nearestneighbour(tfpt[0], tfpt[1]);
                            }
                            else
                            {
                                c = orig.GetPixel(i, j);
                            }
                            bmp.SetPixel(i, j, c);
                        }
                }
                else if (method == 1)
                {
                    for (int i = 0; i < bmp.Width; i++)
                        for (int j = 0; j < bmp.Height; j++)
                        {
                            if (Math.Sqrt(Math.Pow((double)i - centralx, 2) + Math.Pow((double)j - centraly, 2)) <= radius)
                            {
                                double[] tfpt = distf2(i, j, radius);
                                c = bilinear(tfpt[0], tfpt[1]);
                            }
                            else
                            {
                                c = orig.GetPixel(i, j);
                            }
                            bmp.SetPixel(i, j, c);
                        }
                }
                else
                {
                    for (int i = 0; i < bmp.Width; i++)
                        for (int j = 0; j < bmp.Height; j++)
                        {
                            if (Math.Sqrt(Math.Pow((double)i - centralx, 2) + Math.Pow((double)j - centraly, 2)) <= radius)
                            {
                                double[] tfpt = distf2(i, j, radius);
                                c = bicubic(tfpt[0], tfpt[1]);
                            }
                            else
                            {
                                c = orig.GetPixel(i, j);
                            }
                            bmp.SetPixel(i, j, c);
                        }
                }
            }
        }

        public double U(double r)
        {
            double basev = 0;
            if (r != 0)
            {
                basev = r * r * Math.Log10(r * r);
            }
            else
                basev = 0;
            return basev;
        }

        public void getcoefficient(double[,] conp,double[,] desp,int num)//求解系数
        {
            P = new double[num, 3];
            YY = new double[num+3,2];
            K = new double[num ,num];
            L = new double[num + 3, num + 3];
            XX = new double[num + 3, 2];//系数矩阵
            LT = new double[num + 3, num + 3];//L的逆
            kpn = num;
            for (int i = 0; i < num; i++)
            {
                P[i,0] = 1;
                P[i, 1] = conp[i, 0];
                P[i, 2] = conp[i, 1];
            }
            for (int i = 0; i < num; i++)
            {
                YY[i, 0] = desp[i, 0];
                YY[i, 1] = desp[i, 1];
            }
            for (int i = num; i < num+3; i++)
            {
                YY[i, 0] = 0;
                YY[i, 1] = 0;
            }
            for (int i = 0; i < num; i++)
                for (int j = 0; j < num; j++)
                {

                    K[i, j] = U(Math.Sqrt(Math.Pow(conp[i, 0] - conp[j, 0], 2) + Math.Pow(conp[i, 1] - conp[j, 1], 2)));
                    if (i == j)
                        K[i, j] = lambda;     //设置正则项，可能会有某些矩阵不可逆
                }
            for (int i = 0; i < num + 3; i++)
                for (int j = 0; j < num + 3; j++)
                {
                    if (i < num && j < num)
                    {
                        L[i, j] = K[i, j];
                    }
                    if (j >= num && i < num)
                    {
                        L[i, j] = P[i, j - num];
                        L[j, i] = L[i, j];
                    }
                    if (j >= num && i >= num)
                        L[i, j] = 0;
                }
            LT = matrixinvision(L,num+3);
            //求解系数矩阵
            for (int i = 0; i < num + 3; i++)
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < num + 3; k++)
                    {
                        XX[i, j] += LT[i, k] * YY[k,j];
                    }
                }
        }

        public double Normdis(double x1,double y1,double x2,double y2)
        {
            double dis;
            dis = Math.Sqrt(Math.Pow(x1-x2,2)+Math.Pow(y1-y2,2));
            return dis;
        }

        /*public double[,] matrixtransf(double[,] matrix)//矩阵转置
        {
            
        }*/

        public double[,] matrixinvision(double[,] matrix,int n)//求逆
        {
            double fk = 1;
            double[,] expmrt = new double[n, 2 * n];
            double[,] minvision = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    expmrt[i, j] = matrix[i, j];
                }
            for (int i = 0; i < n; i++)
            {
                expmrt[i, i + n] = 1;
            }
            for (int i = 0; i < n; i++)
            {
                fk = expmrt[i, i];
                if (fk == 0)
                {
                    int r = i;
                    for (int j = i+1; j < n; j++) //不能再从前面换起了
                    {
                        if (expmrt[j, i] != 0)
                        {
                            r = j;
                            break;
                        }
                    }
                    //交换两行
                    for (int j = 0; j < 2 * n; j++)
                    {
                        double temp = 0;
                        temp = expmrt[i, j];
                        expmrt[i, j] = expmrt[r, j];
                        expmrt[r, j] = temp;
                    }
                    fk = expmrt[i, i];
                }
                for (int j = 0; j < 2 * n; j++)
                    expmrt[i, j] = expmrt[i, j] / fk;
                for (int j = 0; j < n; j++)//消去这一列其他行的值使其为0
                {
                    if (j != i)
                    {
                        double kj = -expmrt[j, i];
                        for (int k = 0; k < 2 * n; k++)
                        {
                            expmrt[j, k] = kj*expmrt[i,k] + expmrt[j, k];
                        }
                    }
                }
            }
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    minvision[i, j] = expmrt[i, j + n];
            return minvision;

            //采用旁边加一个单位矩阵的方法

                //伴随矩阵的方法在求行列式时的值太大会溢出，所以不能采用这种方法
                /*double[,] mt = new double[n, n];
                double [,] Acs = new double[n-1,n-1];  //余子式
                double matrixv = determinant(matrix, n);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        for (int k = 0; k < n - 1; k++)
                            for (int t = 0; t < n - 1; t++)
                                Acs[k, t] = matrix[k >= i ? k + 1 : k, t >= j ? t + 1 : t];
                        mt[i, j] = determinant(Acs, n - 1)/matrixv;
                    }
                }
                return mt;*/
        }
        public double determinant(double[,] matrix, int n)//计算行列式实际上没用到
        {
            double result = 0;
            double sec1 = 1;
            double sec2 = 1;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i + j >= n)
                    {
                        sec1 *= matrix[j, i + j - n];
                        sec2 *= matrix[n - 1 - j, i - n + j];
                    }
                    else
                    {
                        sec1 *= matrix[j, i + j];
                        sec2 *= matrix[n-1-j, i + j];
                    }
                }
                result += sec1-sec2;
                sec1 = 1;
                sec2 = 1;
            }
            return result;
        }
        public double[] TPSpttf(double x,double y)//利用求得的系数矩阵变换坐标
        {
            double[] tfpt = new double[2];
            for (int i = 0; i < 68; i++)
            {
                tfpt[0] = tfpt[0] + XX[i, 0] * U(Math.Sqrt(Math.Pow(P[i, 1] - x, 2) + Math.Pow(P[i, 2] - y, 2)));
            }
            tfpt[0] += XX[68, 0] + XX[69, 0] * x + XX[70, 0] * y;
            for (int i = 0; i < 68; i++)
            {
                tfpt[1] = tfpt[1] + XX[i, 1] * U(Math.Sqrt(Math.Pow(P[i, 1] - x, 2) + Math.Pow(P[i, 2] - y, 2)));
            }
            tfpt[1] += XX[68, 1] + XX[69, 1] * x + XX[70, 1] * y;
            return tfpt;
        }
        

        public void TPS(double[,] orpt,double[,] modept,int method)
        {
            getcoefficient(orpt, modept, 68); //这里用的图片都是68个关键点
            double[] test;//debug测试可以忽略
            test = TPSpttf(orpt[1,0],orpt[1,1]);
            test = TPSpttf(orpt[0, 0], orpt[0, 1]);

            Color c;
            if (method == 0)
            {
                for (int i = 0; i < bmp.Width; i++)
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        Color kiu = Color.FromArgb(0,0,0);
                        double[] tfpt = TPSpttf((double)i, (double)j);
                        c = nearestneighbour(tfpt[0], tfpt[1]);
                        //if (c == kiu) c = bmp.GetPixel(i, j);
                        bmp.SetPixel(i, j, c);
                    }
            }
            if (method == 1)
            {
                for (int i = 0; i < bmp.Width; i++)
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        Color kiu = Color.FromArgb(0, 0, 0);
                        double[] tfpt = TPSpttf((double)i, (double)j);
                        c = bilinear(tfpt[0], tfpt[1]);
                        //if (c == kiu) c = bmp.GetPixel(i, j);
                        bmp.SetPixel(i, j, c);
                    }
            }
            if (method == 2)
            {
                for (int i = 0; i < bmp.Width; i++)
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        Color kiu = Color.FromArgb(0, 0, 0);
                        double[] tfpt = TPSpttf((double)i, (double)j);
                        c = bicubic(tfpt[0], tfpt[1]);
                        //if (c == kiu) c = bmp.GetPixel(i, j);
                        bmp.SetPixel(i, j, c);
                    }
            }
        }

        public Bitmap getimage() //返回当前图像
        {
            Bitmap bp = new Bitmap(bmp);
            return bp;
        }
        public void reset()  //图片重置
        {
            bmp = new Bitmap(orig);
        }
    }
}
