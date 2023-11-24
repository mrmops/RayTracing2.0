using System;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace RayTracing2._0;

public partial class Form1 : Form {
    private readonly Timer _timer = new() { Interval = 1 };
    private readonly Scene _scene = new();
    protected override bool DoubleBuffered { get; set; } = true;
    private int _angleY;
    private int _angleX;
    private bool _navigate;

    public Form1() {
        InitializeComponent();
        pictureBox.KeyDown += NavigateCamera;
        pictureBox.MouseMove += RotateCamera;
        pictureBox.MouseClick += ToggleIsNavigation;
        Size = new Size(200, 200);
        new Thread(UpdateFrame).Start();
    }

    public async void UpdateFrame() {
        var newCanvasSize = new Size(pictureBox.Width, pictureBox.Height);
        
        Bitmap bitmap = new(width: newCanvasSize.Width, height: newCanvasSize.Height);
        

        await foreach  (var image in _scene.StartRenderFrames(newCanvasSize)) {
            using(var g = Graphics.FromImage(bitmap)) {
                g.Clear(Color.SkyBlue);
            }

            foreach(var pair in image) {
                var color = pair.Value?.ToColor();
                if(color != null) {
                    bitmap.SetPixel(pair.Key.X, pair.Key.Y, color: (Color)color);
                }
            }
            pictureBox.Invoke(new MyDelegate(UpdateImage), bitmap);
        }
    }

    public delegate void MyDelegate(Bitmap bitmap);

    private void UpdateImage(Bitmap bitmap) {
        try {
            pictureBox.Image = bitmap;
        }
        catch (Exception e) {
            Console.WriteLine(e);
            Console.ReadLine();
            throw;
        }
    }

    private void ToggleIsNavigation(object? sender, MouseEventArgs e) {
        _navigate = !_navigate;
        if(_navigate) {
            Point leftTop = pictureBox.PointToScreen(new Point(pictureBox.Width / 2, pictureBox.Height / 2));
            Cursor.Position = leftTop;
            Cursor.Hide();
        } else {
            Cursor.Show();
        }
    }

    private void RotateCamera(object? sender, MouseEventArgs e) {
        if(!_navigate) {
            return;
        }

        var d = Math.PI / 180;

        var pictureBoxWidthCenter = pictureBox.Width / 2;
        var pictureBoxHeightCenter = pictureBox.Height / 2;

        _angleY += e.X - pictureBoxWidthCenter;
        _angleX += e.Y - pictureBoxHeightCenter;

        var rotationMatrix = Matrix4x4.Multiply(
            Matrix4x4.CreateRotationX((float)(_angleX * d)),
            Matrix4x4.CreateRotationY((float)(_angleY * d))

        );


        _scene.RotationMatrix = rotationMatrix;


        Point leftTop = pictureBox.PointToScreen(new Point(pictureBoxWidthCenter, pictureBoxHeightCenter));
        Cursor.Position = leftTop;
    }

    private void pictureBox_Click(object sender, EventArgs e) => pictureBox.Focus();

    private void NavigateCamera(object? sender, KeyEventArgs e) {
        if(!_navigate)
            return;

        var d = 0.2f;
        var camera = _scene.CameraPosition;
        switch(e.KeyCode) {
            case Keys.A: {
                _scene.CameraPosition = Vector3.Transform(new Vector3(-d, 0, 0), _scene.RotationMatrix) + camera;
                return;
            }
            case Keys.D: {
                _scene.CameraPosition = Vector3.Transform(new Vector3(d, 0, 0), _scene.RotationMatrix) + camera;
                return;
            }
            case Keys.W: {
                _scene.CameraPosition = Vector3.Transform(new Vector3(0, 0, d), _scene.RotationMatrix) + camera;
                return;
            }
            case Keys.S: {
                _scene.CameraPosition = Vector3.Transform(new Vector3(0, 0, -d), _scene.RotationMatrix) + camera;
                return;
            }

            case Keys.Space: {
                _scene.CameraPosition = new Vector3(0, d, 0) + camera;
                return;
            }

            case Keys.ShiftKey: {
                _scene.CameraPosition = new Vector3(0, -d, 0) + camera;
                return;
            }
        }
    }
}