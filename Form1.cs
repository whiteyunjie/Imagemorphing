using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Imagemorphing
{
    public partial class Form1 : Form
    {
        string imagepath;
        int method=3;
        double angle; //旋转角度
        double radius;//畸变半径或旋转半径
        double maxr = 500;  //最大旋转半径
        imgprocess im = new imgprocess();
        Bitmap bmp;
        Bitmap orbmp;
        int type = 2;
        public Form1()
        {
            InitializeComponent();
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
                    bmp = new Bitmap(filepath);
                    orbmp = bmp;
                    this.pictureBox1.Image = orbmp;
                    maxr = Math.Min((orbmp.Width - 1) / 2, (orbmp.Height - 1) / 2);
                    this.numericUpDown2.Maximum = (int)maxr;
                }
                else
                {
                    MessageBox.Show("文件格式不正确，请选择“jpg”格式的图片！", "提示", MessageBoxButtons.OK);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked) method = 0;
            if (radioButton2.Checked) method = 1;
            if (radioButton3.Checked) method = 2;
            angle = (double)this.numericUpDown1.Value;
            radius = (double)this.numericUpDown2.Value;
            /*angle = Convert.ToDouble(textBox1.Text);
            radius = Convert.ToDouble(textBox2.Text);
            if (angle < 0 || angle > 360)
            {
                MessageBox.Show("度数必须在0~360度，请重新输入！", "提示", MessageBoxButtons.OK);
                angle = -1;
            }
            if (radius <= 0 || radius > 256)
            {
                MessageBox.Show("半径必须在0~256，请重新输入！", "提示", MessageBoxButtons.OK);
                radius = -1;
            }*/
            if (method == 3)
            {
                MessageBox.Show("请选择一种插值方法", "提示", MessageBoxButtons.OK);
                method = 3;
            }
            if (method != 3)
            {
                //转化为弧度制
                if (orbmp != null)
                {
                    angle = Math.PI * angle / 180;
                    im.reset();
                    im.rotationtf(angle, radius, method);
                    bmp = im.getimage();
                    this.pictureBox2.Image = bmp;
                }
                else
                {
                    MessageBox.Show("图片数据不全，请补充!", "提示", MessageBoxButtons.OK);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked) method = 0;
            if (radioButton2.Checked) method = 1;
            if (radioButton3.Checked) method = 2;
            if (radioButton4.Checked) type = 0;
            if (radioButton5.Checked) type = 1;
            radius = (double)this.numericUpDown3.Value;
            if (method == 3)
            {
                MessageBox.Show("请选择一种插值方法", "提示", MessageBoxButtons.OK);
                method = 3;
            }
            if (type == 2)
            {
                MessageBox.Show("请选择凸变或凹变", "提示", MessageBoxButtons.OK);
                type = 2;
            }
            if (method != 3 && type != 2)
            {
                if (orbmp != null)
                {
                    im.reset();
                    im.distortion(radius, method, type);
                    bmp = im.getimage();
                    this.pictureBox2.Image = bmp;
                }
                else
                {
                    MessageBox.Show("图片数据不全，请补充!", "提示", MessageBoxButtons.OK);
                }
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}
