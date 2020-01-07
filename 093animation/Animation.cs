using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using LineCanvas;
using MathSupport;
using Utilities;

namespace _093animation
{
 
  public class Animation
  {
    private static double screenWidth;
    private static double screenHeight;

    /// <summary>
    /// Form data initialization.
    /// </summary>
    /// <param name="name">Your first-name and last-name.</param>
    /// <param name="screenWidth">Image width in pixels.</param>
    /// <param name="screenHeight">Image height in pixels.</param>
    /// <param name="from">Animation start in seconds.</param>
    /// <param name="to">Animation end in seconds.</param>
    /// <param name="fps">Frames-per-seconds.</param>
    /// <param name="param">Optional text to initialize the form's text-field.</param>
    /// <param name="tooltip">Optional tooltip = param help.</param>
    public static void InitParams (out string name, out int screenWidth, out int screenHeight, out double from, out double to, out double fps, out string param, out string tooltip)
    {
      // {{

      // Put your name here.
      name = "Eliáš Cizl";

      // Image size in pixels.
      screenWidth = 1920;
      screenHeight = 1080;

      // Animation.
      from = 0.0;
      to   = 0.5;
      fps  = 60.0;

      // Specific animation params.
      param = "width=1.0,anti=true,rectangles=10,lined=2";

      // Tooltip = help.
      tooltip = "width=<double>, anti[=<bool>], rectangles=<int>, lined=<int>";

      // }}
    }

    private static double start;
    private static double end;
    private static double deltaTime;
    private static float penWidth;
    private static bool antialias;
    private static int rectanglesCount;
    private static int lined;

    /// <summary>
    /// Global initialization. Called before each animation batch
    /// or single-frame computation.
    /// </summary>
    /// <param name="width">Width of the future canvas in pixels.</param>
    /// <param name="height">Height of the future canvas in pixels.</param>
    /// <param name="start">Start time (t0)</param>
    /// <param name="end">End time (for animation length normalization).</param>
    /// <param name="fps">Required fps.</param>
    /// <param name="param">Optional string parameter from the form.</param>
    public static void InitAnimation (int width, int height, double start, double end, double fps, string param)
    {
      Animation.start = start;
      screenWidth = width;
      screenHeight = height;
      deltaTime = 1.0 / fps;
      Animation.end = end + deltaTime; // updating end time
      deltaTime = 1.0 / (fps + 1); // updating deltaTime


      // input params:
      penWidth = 1.0f;   // pen width
      antialias = false;  // use anti-aliasing?
      rectanglesCount = 10;
      lined = 2;

      Dictionary<string, string> p = Util.ParseKeyValueList(param);
      if (p.Count > 0)
      {
        if (Util.TryParse(p, "width", ref penWidth) && penWidth < 0.0f)
          penWidth = 1.0f;

        Util.TryParse(p, "anti", ref antialias);

        if (Util.TryParse(p, "rectangles", ref rectanglesCount) && rectanglesCount < 1)
          rectanglesCount = 10;

        if (Util.TryParse(p, "lined", ref lined) && lined < 1)
          lined = 2;
      }

      
    }

    /// <summary>
    /// Draw single animation frame.
    /// Has to be re-entrant!
    /// </summary>
    /// <param name="canvas">Canvas to draw to.</param>
    /// <param name="time">Current time in seconds.</param>
    /// <param name="start">Start time (t0)</param>
    /// <param name="end">End time (for animation length normalization).</param>
    /// <param name="param">Optional string parameter from the form.</param>
    public static void DrawFrame (Canvas canvas, double time, double start, double end, string param)
    {
      canvas.SetPenWidth(penWidth);
      canvas.SetAntiAlias(antialias);

      // removing bias
      start = Animation.start;
      end = Animation.end;

      double timeNorm = (time - start) / (end - start);

      double change = Math.Pow(2, timeNorm);
      double rightChange = change * screenWidth / 2;
      double upChange = change * screenHeight;
      //sierpinsky part
      drawSierpinsky(canvas, 10, screenWidth / 2 + rightChange / 2, screenHeight - upChange, screenWidth / 2, screenHeight, screenWidth / 2 + rightChange, screenHeight);

      timeNorm *= lined;

      for (int i = 0; i < rectanglesCount; i++)
      {
        // i / rectanglesCount -> (i + 1) / rectanglesCount
        double coefficient1 = (i + timeNorm) / rectanglesCount;
        double distance1 = Math.Pow(2, coefficient1) - 1;//1 / Math.Pow(2, 10 * (1 - coefficient1));
        DrawCenterRectangle(canvas, distance1);

        double coefficient2 = (i + 1 + timeNorm) / rectanglesCount;
        double distance2 = Math.Pow(2, coefficient2) - 1;//1 / Math.Pow(2, 10 * (1 - coefficient2));
        if (i % lined == 0)
        {
          DrawCenterConnectingLines(canvas, distance1, distance2);
        }
      }
    }

    private static void DrawCenterConnectingLines (Canvas canvas, double distance1, double distance2)
    {
      DrawConnectingLines(canvas, screenWidth / 4, screenHeight / 2, distance1, distance2);
    }

    private static void DrawConnectingLines (Canvas canvas, double centerX, double centerY, double distance1, double distance2)
    {
      double horizontalDistance1 = (screenWidth / 4) * distance1;
      double verticalDistance1 = (screenHeight / 2) * distance1;
      double horizontalDistance2 = (screenWidth / 4) * distance2;
      double verticalDistance2 = (screenHeight / 2) * distance2;
      canvas.Line(centerX + horizontalDistance1, centerY - verticalDistance1, centerX + horizontalDistance2, centerY - verticalDistance2); // top right
      canvas.Line(centerX + horizontalDistance1, centerY + verticalDistance1, centerX + horizontalDistance2, centerY + verticalDistance2); // bottom right
      canvas.Line(centerX - horizontalDistance1, centerY + verticalDistance1, centerX - horizontalDistance2, centerY + verticalDistance2); // bottom left
      canvas.Line(centerX - horizontalDistance1, centerY - verticalDistance1, centerX - horizontalDistance2, centerY - verticalDistance2); // top left
    }

    private static void DrawCenterRectangle (Canvas canvas, double distanceFromCenterNormalized)
    {
      DrawRectangle(canvas, screenWidth / 4, screenHeight / 2, distanceFromCenterNormalized);
    }

    private static void DrawRectangle (Canvas canvas, double centerX, double centerY, double distanceFromCenterNormalized)
    {
      double horizontalDistance = (screenWidth / 4) * distanceFromCenterNormalized;
      double verticalDistance = (screenHeight / 2) * distanceFromCenterNormalized;
      if (horizontalDistance <= screenWidth / 4)
        canvas.Line(centerX + horizontalDistance, centerY - verticalDistance, centerX + horizontalDistance, centerY + verticalDistance); // right
      canvas.Line(centerX + horizontalDistance, centerY + verticalDistance, centerX - horizontalDistance, centerY + verticalDistance); // bottom
      canvas.Line(centerX - horizontalDistance, centerY + verticalDistance, centerX - horizontalDistance, centerY - verticalDistance); // left
      canvas.Line(centerX - horizontalDistance, centerY - verticalDistance, centerX + horizontalDistance, centerY - verticalDistance); // top
    }

    private static void drawSierpinsky (Canvas canvas, int n, double x1, double y1, double x2, double y2, double x3, double y3)
    {
      Point[] triangle = new Point[] { new Point((int)x1, (int)y1), new Point((int)x2, (int)y2), new Point((int)x3, (int)y3) };
      double leftX = (x1 + x2) / 2;
      double leftY = (y1 + y2) / 2;
      double rightX = (x1 + x3) / 2;
      double rightY = (y1 + y3) / 2;
      double bottomX = (x2 + x3) / 2;
      double bottomY = (y2 + y3) / 2;
      if (n > 0)
      {
        canvas.Line(triangle[0].X, triangle[0].Y, triangle[1].X, triangle[1].Y);
        canvas.Line(triangle[0].X, triangle[0].Y, triangle[2].X, triangle[2].Y);
        canvas.Line(triangle[1].X, triangle[1].Y, triangle[2].X, triangle[2].Y);
        n--;
        drawSierpinsky(canvas, n, leftX, leftY, x2, y2, bottomX, bottomY);
        drawSierpinsky(canvas, n, x1, y1, leftX, leftY, rightX, rightY);
        drawSierpinsky(canvas, n, rightX, rightY, bottomX, bottomY, x3, y3);
      }
    }
  }
}
