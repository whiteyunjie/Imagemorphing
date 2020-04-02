using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Imagemorphing
{
    public partial class Form2 : Form
    {
        imgprocess im = new imgprocess();
        Bitmap modeim;
        Bitmap orbmp;
        Bitmap finalim;
        Graphics orpt;
        Graphics modept;
        int[,] orgob;
        int[,] mode;
        int method=3;//选择的差值方式
        double[,] mode2or;
        GraphicsPath orpath;
        GraphicsPath modepath;
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)  //读取txt文件坐标并转化为整型数
        {
            string filepath = " ";
            string Suffix = " ";
            var f = new OpenFileDialog();
            orgob = new int[68, 2];
            if (f.ShowDialog() == DialogResult.OK)
            {
                filepath = f.FileName;
                Suffix = filepath.Substring(filepath.IndexOf(".") + 1);
                if (Suffix == "txt")
                {
                    string[] lines = File.ReadAllLines(filepath);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        int e1 = 0;
                        int e2 = 0;
                        int backspace = 0;
                        for (int j = 0; j < lines[i].Length; j++)
                        {
                            if (lines[i][j] == 'e')
                            {
                                if (e1 == 0) e1 = j;
                                else e2 = j;
                            }
                            if (lines[i][j] == ' ') backspace = j;
                        }
                        string num1 = lines[i].Substring(0, e1);
                        string dec1 = lines[i].Substring(e1 + 1, backspace - e1 - 1);
                        string num2 = lines[i].Substring(backspace + 1, e2 - backspace - 1);
                        string dec2 = lines[i].Substring(e2 + 1, lines[i].Length - e2 - 1);

                        orgob[i, 0] = (int)(Convert.ToDouble(num1) * Math.Pow(10, Convert.ToInt32(dec1)));
                        orgob[i, 1] = (int)(Convert.ToDouble(num2) * Math.Pow(10, Convert.ToInt32(dec2)));
                    }
                    orpath = new GraphicsPath();
                    for (int i = 0; i < 68; i++)
                    {
                        orpath.AddRectangle(new RectangleF((float)orgob[i, 0], (float)orgob[i, 1], 1, 1));
                    }
                }
                else
                {
                    MessageBox.Show("文件格式不正确，请选择“txt”格式的文件！", "提示", MessageBoxButtons.OK);
                }
            }
            //orpath.AddRectangle(modepath.GetBounds());//得到这些坐标点的矩形轮廓从而便于坐标变换
            //在该图片上标出轮廓点
            /*Bitmap bp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
            this.pictureBox1.Image = bp;
            modept = Graphics.FromImage(bp);
            GraphicsPath gp = new GraphicsPath();
            for (int i = 0; i < 68; i++)
            {
                gp.AddRectangle(new RectangleF((float)orgob[i, 0] * this.pictureBox1.Width / orbmp.Width, (float)orgob[i, 1] * this.pictureBox1.Height / orbmp.Height, 1, 1));
            }
            modept.DrawPath(new Pen(Color.Red), gp);*/
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filepath = " ";
            string Suffix = " ";
            var f = new OpenFileDialog();
            if (f.ShowDialog() == DialogResult.OK)
            {
                filepath = f.FileName;
                Suffix = filepath.Substring(filepath.IndexOf(".") + 1);
                if (Suffix == "jpg")
                {
                    im.creatimage(filepath);
                    orbmp = new Bitmap(filepath);
                    this.pictureBox1.BackgroundImage = orbmp;
                }
                else
                {
                    MessageBox.Show("文件格式不正确，请选择“jpg”格式的图片！", "提示", MessageBoxButtons.OK);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filepath = " ";
            string Suffix = " ";
            var f = new OpenFileDialog();
            if (f.ShowDialog() == DialogResult.OK)
            {
                filepath = f.FileName;
                Suffix = filepath.Substring(filepath.IndexOf(".") + 1);
                if (Suffix == "jpg")
                {
                    modeim = new Bitmap(filepath);
                    this.pictureBox2.BackgroundImage = modeim;
                    string index = filepath.Substring(filepath.IndexOf(".")-1, 1);
                }
                else
                {
                    MessageBox.Show("文件格式不正确，请选择“jpg”格式的图片！", "提示", MessageBoxButtons.OK);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string filepath = " ";
            string Suffix = " ";
            var f = new OpenFileDialog();
            mode = new int[68, 2];
            if (f.ShowDialog() == DialogResult.OK)
            {
                filepath = f.FileName;
                Suffix = filepath.Substring(filepath.IndexOf(".") + 1);
                if (Suffix == "txt")
                {
                    string[] lines = File.ReadAllLines(filepath);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        int e1 = 0;
                        int e2 = 0;
                        int backspace = 0;
                        for (int j = 0; j < lines[i].Length; j++)
                        {
                            if (lines[i][j] == 'e')
                            {
                                if (e1 == 0) e1 = j;
                                else e2 = j;
                            }
                            if (lines[i][j] == ' ') backspace = j;
                        }
                        string num1 = lines[i].Substring(0, e1);
                        string dec1 = lines[i].Substring(e1 + 1, backspace - e1 - 1);
                        string num2 = lines[i].Substring(backspace + 1, e2 - backspace - 1);
                        string dec2 = lines[i].Substring(e2 + 1, lines[i].Length - e2 - 1);

                        mode[i, 0] = (int)(Convert.ToDouble(num1) * Math.Pow(10, Convert.ToInt32(dec1)));
                        mode[i, 1] = (int)(Convert.ToDouble(num2) * Math.Pow(10, Convert.ToInt32(dec2)));
                    }
                    modepath = new GraphicsPath();
                    for (int i = 0; i < 68; i++)
                    {
                        modepath.AddRectangle(new RectangleF((float)mode[i, 0], (float)mode[i, 1], 1, 1));
                    }
                    //modepath.AddRectangle(modepath.GetBounds());//得到这些坐标点的矩形轮廓从而便于坐标变换
                }
                else
                {
                    MessageBox.Show("文件格式不正确，请选择“txt”格式的文件！", "提示", MessageBoxButtons.OK);
                }
            }
            //在该图片上标出轮廓点
            /*Bitmap bp = new Bitmap(this.pictureBox2.Width, this.pictureBox2.Height);
            this.pictureBox2.Image = bp;
            modept = Graphics.FromImage(bp);
            GraphicsPath gp = new GraphicsPath();
            for (int i = 0; i < 68; i++)
            {
                gp.AddRectangle(new RectangleF((float)mode[i, 0]* this.pictureBox2.Width/modeim.Width, (float)mode[i, 1]*this.pictureBox2.Height / modeim.Height, 1, 1));
            }
            gp.AddRectangle(gp.GetBounds());
            modept.DrawPath(new Pen(Color.Red), gp);*/
            //this.pictureBox1.SendToBack();
            //this.pictureBox2.SendToBack();
        }

        public void transferpts()//将模板图中的关键点坐标转移对应到被改的图上的坐标从而进行TPS
        {
            mode2or = new double[68,2];
            RectangleF orrec = orpath.GetBounds();
            RectangleF moderec = modepath.GetBounds();
            float cenx1 = orrec.X + orrec.Width / 2;          //原矩形和模板矩形的中心坐标
            float ceny1 = orrec.Y + orrec.Height / 2;
            float cenx2 = moderec.X + moderec.Width / 2;
            float ceny2 = moderec.Y + moderec.Height / 2;
            float dx = cenx2 - cenx1;  //偏移
            float dy = ceny2 - ceny1;
            //先尝试按长宽缩放
            float k1 = orrec.Width / moderec.Width ;
            float k2 = orrec.Height / moderec.Width;
            //float k = orrec.Width * orrec.Height / (moderec.Width*moderec.Height);
            for (int i = 0; i < 68; i++)
            {
                mode2or[i, 0] = k1*(mode[i, 0] - cenx2) + cenx2 - dx;
                mode2or[i, 1] = k2*(mode[i, 1] - ceny2) + ceny2 - dy;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked) method = 0;
            if (radioButton2.Checked) method = 1;
            if (radioButton3.Checked) method = 2;
            if (method == 3)
            {
                MessageBox.Show("请选择一种插值方法", "提示", MessageBoxButtons.OK);
                method = 3;
            }
            if (method != 3)
            {
                if (orgob!= null && mode != null && orbmp!=null)
                {
                    double[,] orpoint = new double[68, 2];
                    for (int i = 0; i < 68; i++)
                    {
                        orpoint[i, 0] = (double)orgob[i, 0];
                        orpoint[i, 1] = (double)orgob[i, 1];
                    }
                    transferpts();
                    if(method==0)
                        im.TPS(mode2or, orpoint, 0);
                    if (method == 1)
                        im.TPS(mode2or, orpoint, 1);
                    if (method == 2)
                        im.TPS(mode2or, orpoint, 2);      //注意映射和反映射的关系！！！
                    finalim = im.getimage();
                    this.pictureBox3.Image = finalim;
                }
                else
                {
                    MessageBox.Show("图片或坐标数据不全，请补充!","提示",MessageBoxButtons.OK);
                }
            }
            /*Bitmap bp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);//将人脸的关键点表在图上
            this.pictureBox1.Image = bp;
            orpt = Graphics.FromImage(bp);
            GraphicsPath gp = new GraphicsPath();
            for (int i = 0; i < 68; i++)
            {
                gp.AddRectangle(new RectangleF((float)orgob[i, 0] * this.pictureBox1.Width / orbmp.Width, (float)orgob[i, 1] * this.pictureBox1.Height / orbmp.Height, 1, 1));
            }
            gp.AddRectangle(gp.GetBounds());
            for (int i = 0; i < 68; i++)
            {
                gp.AddRectangle(new RectangleF((float)mode2or[i, 0] * this.pictureBox1.Width / orbmp.Width, (float)mode2or[i, 1] * this.pictureBox1.Height / orbmp.Height, 1, 1));
            }
            orpt.DrawPath(new Pen(Color.Red), gp);*/
        }
    }
}
