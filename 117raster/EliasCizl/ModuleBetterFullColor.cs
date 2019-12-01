using Raster;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace _117raster.EliasCizl
{
  class ModuleBetterFullColor : Modules.DefaultRasterModule
  {
    public ModuleBetterFullColor ()
    {
      param = "width = 4096, height = 4096";;
    }

    public override string Author => "CizlElias";

    public override string Name => "BetterFullColor";

    /// <summary>
    /// Tooltip for Param (text parameters).
    /// </summary>
    public override string Tooltip => "[width=<width>][,height=<height>][,oblique]";

    /// <summary>
    /// Output raster image.
    /// </summary>
    protected Bitmap outImage = null;

    /// <summary>
    /// Output message (color check).
    /// </summary>
    protected string message;

    public override int InputSlots { get; set; } = 0;

    /// <summary>
    /// Recompute the output image[s] according to input image[s].
    /// Blocking (synchronous) function.
    /// #GetOutput() functions can be called after that.
    /// </summary>
    public override void Update ()
    {
      UserBreak = false;

      // Default values.
      int width = 4096;
      int height = 4096;
      bool oblique = false;

      // We are not using 'paramDirty', so the 'Param' string has to be parsed every time.
      Dictionary<string, string> parameters = Util.ParseKeyValueList(param);
      if (parameters.Count > 0)
      {
        // width=<int> [image width in pixels]
        if (Util.TryParse(parameters, "width", ref width))
          width = Math.Max(1, width);

        // height=<int> [image height in pixels]
        if (Util.TryParse(parameters, "height", ref height))
          height = Math.Max(1, height);

        // slow ... use Bitmap.SetPixel()
        oblique = parameters.ContainsKey("oblique");
      }

      outImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);

      // Generate pixel data (fast memory-mapped code).


      if (oblique)
      {
        // slow

        int x = 0;
        int y = 0;

        for (short sum = 0; sum <= 255 * 3; sum++)
        {
          if (UserBreak)
            break;

          if (sum % 2 == 0)
          {
            for (short r = 0; r <= sum && r <= 255; r++)
            {
              sum -= r;
              if (r % 2 == 0)
              {
                for (short g = 0; g <= sum && g <= 255; g++)
                {
                  sum -= g;
                  if (sum <= 255)
                  {
                    outImage.SetPixel(x, y, Color.FromArgb(r, g, sum));
                    y--;
                    x++;
                    if (y < 0 || x >= width)
                    {
                      y += x + 1;
                      x = 0;
                      int overflow = y - height + 1;
                      if (overflow > 0)
                      {
                        y -= overflow;
                        x = overflow;
                      }
                    }
                  }
                  sum += g;
                }
              }
              else
              {
                for (short g = (sum > 255) ? (short)255 : sum; g >= 0; g--)
                {
                  sum -= g;
                  if (sum <= 255)
                  {
                    outImage.SetPixel(x, y, Color.FromArgb(r, g, sum));
                    y--;
                    x++;
                    if (y < 0 || x >= width)
                    {
                      y += x + 1;
                      x = 0;
                      int overflow = y - height + 1;
                      if (overflow > 0)
                      {
                        y -= overflow;
                        x = overflow;
                      }
                    }
                  }
                  sum += g;
                }
              }
              sum += r;
            }
          }
          else
          {
            for (short r = (sum > 255) ? (short)255 : sum; r >= 0; r--)
            {
              sum -= r;
              if (r % 2 == 0)
              {
                for (short g = (sum > 255) ? (short)255 : sum; g >= 0; g--)
                {
                  sum -= g;
                  if (sum <= 255)
                  {
                    outImage.SetPixel(x, y, Color.FromArgb(r, g, sum));
                    y--;
                    x++;
                    if (y < 0 || x >= width)
                    {
                      y += x + 1;
                      x = 0;
                      int overflow = y - height + 1;
                      if (overflow > 0)
                      {
                        y -= overflow;
                        x = overflow;
                      }
                    }
                  }
                  sum += g;
                }
              }
              else
              {
                for (short g = 0; g <= sum && g <= 255; g++)
                {
                  sum -= g;
                  if (sum <= 255)
                  {
                    outImage.SetPixel(x, y, Color.FromArgb(r, g, sum));
                    y--;
                    x++;
                    if (y < 0 || x >= width)
                    {
                      y += x + 1;
                      x = 0;
                      int overflow = y - height + 1;
                      if (overflow > 0)
                      {
                        y -= overflow;
                        x = overflow;
                      }
                    }
                  }
                  sum += g;
                }
              }
              sum += r;
            }
          }
        }
      }
      else
      {
        // fast

        BitmapData dataOut = outImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, outImage.PixelFormat);
        unsafe
        {
          int dO = Image.GetPixelFormatSize(outImage.PixelFormat) / 8;  // pixel size in bytes
          byte* optr = (byte*)dataOut.Scan0;

          for (short sum = 0; sum <= 255 * 3; sum++)
          {
            if (UserBreak)
              break;

            if(sum % 2 == 0)
            {
              for (short r = 0; r <= sum && r <= 255; r++)
              {
                sum -= r;
                if (r % 2 == 0)
                {
                  for (short g = 0; g <= sum && g <= 255; g++)
                  {
                    sum -= g;
                    if (sum <= 255)
                    {
                      optr[0] = (byte)r;
                      optr[1] = (byte)g;
                      optr[2] = (byte)sum;

                      optr += dO;
                    }
                    sum += g;
                  }
                }
                else
                {
                  for (short g = (sum > 255) ? (short)255 : sum; g >= 0; g--)
                  {
                    sum -= g;
                    if (sum <= 255)
                    {
                      optr[0] = (byte)r;
                      optr[1] = (byte)g;
                      optr[2] = (byte)sum;

                      optr += dO;
                    }
                    sum += g;
                  }
                }
                sum += r;
              }
            }
            else
            {
              for (short r = (sum > 255) ? (short)255 : sum; r >= 0; r--)
              {
                sum -= r;
                if (r % 2 == 0)
                {
                  for (short g = (sum > 255) ? (short)255 : sum; g >= 0; g--)
                  {
                    sum -= g;
                    if (sum <= 255)
                    {
                      optr[0] = (byte)r;
                      optr[1] = (byte)g;
                      optr[2] = (byte)sum;

                      optr += dO;
                    }
                    sum += g;
                  }
                }
                else
                {
                  for (short g = 0; g <= sum && g <= 255; g++)
                  {
                    sum -= g;
                    if (sum <= 255)
                    {
                      optr[0] = (byte)r;
                      optr[1] = (byte)g;
                      optr[2] = (byte)sum;

                      optr += dO;
                    }
                    sum += g;
                  }
                }
                sum += r;
              }
            }
          }
        }
        outImage.UnlockBits(dataOut);
      }



      // Output message.
      if (!UserBreak)
      {
        long colors = Draw.ColorNumber(outImage);
        message = colors == (1 << 24) ? "Colors: 16M, Ok" : $"Colors: {colors}, Fail";
      }
      else
        message = null;
    }

    /// <summary>
    /// Returns an output raster image.
    /// Can return null.
    /// </summary>
    /// <param name="slot">Slot number from 0 to OutputSlots-1.</param>
    public override Bitmap GetOutput (
      int slot = 0) => outImage;

    /// <summary>
    /// Returns an optional output message.
    /// Can return null.
    /// </summary>
    /// <param name="slot">Slot number from 0 to OutputSlots-1.</param>
    public override string GetOutputMessage (
      int slot = 0) => message;
  }
}
