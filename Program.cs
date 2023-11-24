using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RayTracing2._0;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {

        // var scene = new Scene();
        // var frameSize = new Size(7680, 4320);
        // var fragmentsCount = 10;
        // var result = await scene.GetFrame(frameSize, fragmentsCount);
        // var canvas = new Bitmap(frameSize.Width, frameSize.Height);
        // var columnsPerFragment = canvas.Width / fragmentsCount + 1;
        // foreach (var imageFragment in result)
        // {
        //     var colors = imageFragment.Colors;
        //     var fragmentShift = imageFragment.FragmentIndex * columnsPerFragment;
        //     for (var x = 0; x < colors.GetLength(0); x++)
        //     {
        //         var dx = x + fragmentShift;
        //         if(dx >= canvas.Width)
        //             break;
        //         for (var y = 0; y < colors.GetLength(1); y++)
        //         {
        //                 
        //             canvas.SetPixel(dx, y, colors[x, y]);
        //         }
        //     }
        // }
        // canvas.Save("8k.png", ImageFormat.Png);
        AllocConsole();
        Application.EnableVisualStyles();
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form1());

        /*
        var scene = new Scene();
        var stopWatch = new Stopwatch();
        var newCanvasSize = new Size(600, 600);
        var sum = 0L;
        var fragmentsCount = 7;
        var count = 10;
        for (int i = 0; i < count; i++)
        {
            stopWatch.Reset();
            stopWatch.Start();
            var result = await scene.GetFrame(newCanvasSize, fragmentsCount);
            //UpdateFrame();
            // var canvas = new Bitmap(newCanvasSize.Width, newCanvasSize.Height);
            // var fragments = task.Result;
            // var columnsPerFragment = canvas.Width / fragmentsCount + 1;
            // foreach (var imageFragment in fragments)
            // {
            //     var colors = imageFragment.Colors;
            //     var fragmentShift = imageFragment.FragmentIndex * columnsPerFragment;
            //     for (var x = 0; x < colors.GetLength(0); x++)
            //     {
            //         var dx = x + fragmentShift;
            //         if (dx >= canvas.Width)
            //             break;
            //         for (var y = 0; y < colors.GetLength(1); y++)
            //         {
            //             canvas.SetPixel(dx, y, colors[x, y]);
            //         }
            //     }
            // }

            stopWatch.Stop();
            var stopWatchElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            sum += stopWatchElapsedMilliseconds;
            Console.Write($"{sum}     ");
            Console.WriteLine(stopWatchElapsedMilliseconds);

        }

        Console.Write($"{sum * 1d / count}");
        Console.ReadKey();*/
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();
}