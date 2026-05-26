using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace proiect_atestat_random_nou_pt_nota_10
{
    public partial class PaintHomePage : Form
    {
        List<string>shape_names=new List<string>();
        List<List<Point>> shapes = new List<List<Point>>();
        Dictionary<string, bool> shapeHiddenStatus = new Dictionary<string, bool>();

        public PaintHomePage()
        {
            InitializeComponent();
            this.init_shapes();
            this.clear_picturebox();
            listView1.View = View.Details;
            listView1.Columns.Add("Shapes", 100);
            listView1.Columns.Add("Times Drawn", 100);

            foreach (string shape in shape_names)
            {
                this.listView1.Items.Add(shape).SubItems.Add("0");
                this.comboBox1.Items.Add(shape);
                this.checkedListBox1.Items.Add(shape, true);
                this.shapeHiddenStatus[shape] = false; 
            }
            this.checkedListBox1.ItemCheck += checkedListBox1_ItemCheck;

            this.progressBar1.Maximum = 100;
            this.progressBar1.Value = 100;
            this.UpdateProgressBar();
        }
        private bool drawing = false;
        private Point lastPoint;
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string shapeName = this.checkedListBox1.Items[e.Index].ToString();

            // Update hidden status based on new check state
            bool willBeHidden = (e.NewValue == CheckState.Unchecked);
            this.shapeHiddenStatus[shapeName] = willBeHidden;

            // Refresh the listView to show/hide items
            this.RefreshListView();
        }

        private void RefreshListView()
        {
            Dictionary<string, string> shapeCounts = new Dictionary<string, string>();
            foreach (ListViewItem item in this.listView1.Items)
            {
                shapeCounts[item.Text] = item.SubItems[1].Text;
            }
            this.listView1.Items.Clear();
            for (int i = 0; i < this.shape_names.Count; i++)
            {
                string shape = this.shape_names[i];
                if (!this.shapeHiddenStatus[shape])
                {
                    ListViewItem newItem = this.listView1.Items.Add(shape);
                    newItem.SubItems.Add(shapeCounts.ContainsKey(shape) ? shapeCounts[shape] : "0");
                }
            }
        }

        private void clear_picturebox()
        {
            // init picturebox
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }
            pictureBox1.Image = bmp;
            if (this.pictureBox1.Image != null)
            {
                undoStack.Push(new Bitmap(this.pictureBox1.Image));
            }
            this.UpdateProgressBar();
        }

        private void save_checkpoint()
        {
            if (this.pictureBox1.Image != null)
            {
                undoStack.Push(new Bitmap(this.pictureBox1.Image));
            }
        }

        private Stack<Bitmap> undoStack = new Stack<Bitmap>();
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            this.drawing = true;
            this.lastPoint = e.Location;
            this.save_checkpoint();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.drawing)
            {
                using (Graphics g = Graphics.FromImage(this.pictureBox1.Image))
                {
                    g.DrawLine(new Pen(this.mouse_color, this.mouse_width), this.lastPoint, e.Location);
                }
                this.lastPoint = e.Location;
                this.pictureBox1.Refresh();
            }
        }


        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            this.drawing = false;
            this.UpdateProgressBar();
        }

        private Color mouse_color = Color.Black;
        private int mouse_width = 5;
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            this.mouse_width=(int)numericUpDown1.Value;
        }

        private void clear_button_Click(object sender, EventArgs e)
        {
            this.clear_picturebox();
        }
        private void draw(int idx)
        {
            //this.save_checkpoint();
            using (Graphics g = this.pictureBox1.CreateGraphics())
            {
                for (int i = 0; i < this.shapes[idx].Count; ++i)
                {
                    g.DrawLine(new Pen(this.mouse_color, this.mouse_width), this.shapes[idx][i], this.shapes[idx][(i+1)% this.shapes[idx].Count]);
                }
            }
            this.UpdateProgressBar();
        }

        private void draw_preset_button_Click(object sender, EventArgs e)
        {
            int idx = this.comboBox1.SelectedIndex;
            if (idx < 0) return;

            string selectedShape = this.shape_names[idx];

            if (!this.shapeHiddenStatus[selectedShape])
            {
                foreach (ListViewItem item in this.listView1.Items)
                {
                    if (item.Text == selectedShape)
                    {
                        item.SubItems[1].Text = (Int32.Parse(item.SubItems[1].Text) + 1).ToString();
                        break;
                    }
                }
            }

            this.draw(idx);
        }

        private void undo_button_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                var prevstate=this.undoStack.Pop();
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }
                pictureBox1.Image = prevstate;
                pictureBox1.Refresh();
            }
            this.UpdateProgressBar();
        }

        private void buttonPickColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                this.mouse_color = colorDialog.Color;
            }
        }
        private void UpdateProgressBar()
        {
            if (pictureBox1.Image != null)
            {
                Bitmap bmp = (Bitmap)pictureBox1.Image;
                int totalPixels = bmp.Width * bmp.Height;
                int nonWhitePixels = 0;

                // Count non-white pixels (drawn pixels)
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        if (pixel.R != 255 || pixel.G != 255 || pixel.B != 255)
                        {
                            nonWhitePixels++;
                        }
                    }
                }

                int usedPercentage = (int)(((double)nonWhitePixels / totalPixels) * 100);
                progressBar1.Value = Math.Min(100, usedPercentage);
            }
        }

        private void init_shapes()
        {
            // square
            shape_names.Add("Squares");
            {
                var square = new List<Point>();
                square.Add(new Point(pictureBox1.Size.Width / 2 - 50, pictureBox1.Size.Height / 2 - 50));
                square.Add(new Point(pictureBox1.Size.Width / 2 + 50, pictureBox1.Size.Height / 2 - 50));
                square.Add(new Point(pictureBox1.Size.Width / 2 + 50, pictureBox1.Size.Height / 2 + 50));
                square.Add(new Point(pictureBox1.Size.Width / 2 - 50, pictureBox1.Size.Height / 2 + 50));
                shapes.Add(square);
            }

            // triangle
            shape_names.Add("Triangles");
            {
                var triangle = new List<Point>();
                triangle.Add(new Point(pictureBox1.Size.Width / 2, pictureBox1.Size.Height / 2 - 50));
                triangle.Add(new Point(pictureBox1.Size.Width / 2 + 50, pictureBox1.Size.Height / 2 + 50));
                triangle.Add(new Point(pictureBox1.Size.Width / 2 - 50, pictureBox1.Size.Height / 2 + 50));
                shapes.Add(triangle);
            }

            // rectangle
            shape_names.Add("Rectangles");
            {
                var rectangle = new List<Point>();
                rectangle.Add(new Point(pictureBox1.Size.Width / 2 - 75, pictureBox1.Size.Height / 2 - 40));
                rectangle.Add(new Point(pictureBox1.Size.Width / 2 + 75, pictureBox1.Size.Height / 2 - 40));
                rectangle.Add(new Point(pictureBox1.Size.Width / 2 + 75, pictureBox1.Size.Height / 2 + 40));
                rectangle.Add(new Point(pictureBox1.Size.Width / 2 - 75, pictureBox1.Size.Height / 2 + 40));
                shapes.Add(rectangle);
            }

            // pentagon
            shape_names.Add("Pentagons");
            {
                var pentagon = new List<Point>();
                pentagon.Add(new Point(pictureBox1.Size.Width / 2, pictureBox1.Size.Height / 2 - 50));
                pentagon.Add(new Point(pictureBox1.Size.Width / 2 + 47, pictureBox1.Size.Height / 2 - 15));
                pentagon.Add(new Point(pictureBox1.Size.Width / 2 + 29, pictureBox1.Size.Height / 2 + 40));
                pentagon.Add(new Point(pictureBox1.Size.Width / 2 - 29, pictureBox1.Size.Height / 2 + 40));
                pentagon.Add(new Point(pictureBox1.Size.Width / 2 - 47, pictureBox1.Size.Height / 2 - 15));
                shapes.Add(pentagon);
            }
        }
    }
}
