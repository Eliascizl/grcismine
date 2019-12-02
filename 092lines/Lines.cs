using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using LineCanvas;
using Utilities;

namespace _092lines
{
  public class Lines
  {
    /// <summary>
    /// Form data initialization.
    /// </summary>
    /// <param name="name">Your first-name and last-name.</param>
    /// <param name="wid">Initial image width in pixels.</param>
    /// <param name="hei">Initial image height in pixels.</param>
    /// <param name="param">Optional text to initialize the form's text-field.</param>
    /// <param name="tooltip">Optional tooltip = param help.</param>
    public static void InitParams (out string name, out int wid, out int hei, out string param, out string tooltip)
    {
      // {{

      // Put your name here.
      name = "Eliáš Cizl";

      // Image size in pixels.
      wid = 800;
      hei = 150;

      // Specific animation params.
      param = "text=Hello World!,space=2,width=1,rainbow=false,size=100";

      // Tooltip = help.
      tooltip = "text=<string>, space=<int>, width=<int>, rainbow=<bool>, size=<int>";

      // }}
    }

    private static bool CompareColors(Color color1, Color color2)
    {
      return color1.R == color2.R && color1.G == color2.G && color1.B == color2.B;
    }

    private static Random random = new Random();
    private static Color GetRandomColor ()
    {
      return Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
    }

    /// <summary>
    /// Draw the image into the initialized Canvas object.
    /// </summary>
    /// <param name="c">Canvas ready for your drawing.</param>
    /// <param name="param">Optional string parameter from the form.</param>
    public static void Draw (Canvas c, string param)
    {
      string text = "Hello world!";
      int lineSpacing = 2;
      int lineWidth = 1;
      int textSize = c.Height * 2 / 3;
      bool rainbow = false;

      Dictionary<string, string> p = Util.ParseKeyValueList(param);
      if (p.Count > 0)
      {
        // text=<string>
        text = p["text"];

        // space=<lineSpacing>
        if (Util.TryParse(p, "space", ref lineSpacing))
        {
          if (lineSpacing < 1)
            lineSpacing = 1;
        }

        // width=<lineWidth>
        if (Util.TryParse(p, "width", ref lineWidth))
        {
          if (lineWidth < 1)
          {
            lineWidth = 1;
          }
        }

        // size=<textSize>
        if (Util.TryParse(p, "size", ref textSize))
        {
          if (textSize < 10)
          {
            textSize = 10;
          }
        }

        // rainbow=<bool>
        Util.TryParse(p, "rainbow", ref rainbow);
      }

      Bitmap bmp = new Bitmap(c.Width, c.Height);
      using (Graphics graphics = Graphics.FromImage(bmp))
      {
        graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, bmp.Width, bmp.Height);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        graphics.DrawString(text, new Font("Arial", textSize), new SolidBrush(Color.White), 0, 0);
        graphics.Flush();
        graphics.Dispose();
      }

      c.SetColor(Color.White);
      c.SetPenWidth(lineWidth);
      c.SetAntiAlias(false);

      bool currentlyMakingLine = false;
      int startX = 0; // necessary init
      int startY = 0; // necessary init
      for (int i = lineSpacing / 2; i < bmp.Height; i += lineSpacing)
      {
        for (int j = 0; j < bmp.Width; j++)
        {
          if (currentlyMakingLine)
          {
            if(CompareColors(bmp.GetPixel(j, i), Color.Black))
            {
              currentlyMakingLine = false;
              if (rainbow)
              {
                c.SetColor(GetRandomColor());
              }
              c.Line(startX, startY, j, i);
            }
          }
          else
          {
            if(!CompareColors(bmp.GetPixel(j,i), Color.Black))
            {
              currentlyMakingLine = true;
              startX = j;
              startY = i;
            }
          }
        }
      }
    }
  }
}
