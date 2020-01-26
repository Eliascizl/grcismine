using System;
using System.Collections.Generic;
using System.IO;
using MathSupport;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Utilities;

namespace _086shader
{
  public class AnimatedCamera : DefaultDynamicCamera
  {
    /// <summary>
    /// Optional form-data initialization.
    /// </summary>
    /// <param name="name">Return your full name.</param>
    /// <param name="param">Optional text to initialize the form's text-field.</param>
    /// <param name="tooltip">Optional tooltip = param help.</param>
    public static void InitParams (out string name, out string param, out string tooltip)
    {
      // {{

      name = "Eliáš Cizl";
      param = "loop=false, acc=false";
      tooltip = "loop=<true if your camera should loop (first and last points are equal)>, acc=<true if you want the camera accelerate and deccelerate as it moves between points>";

      // }}
    }

    /// <summary>
    /// Called after form's Param field is changed.
    /// </summary>
    /// <param name="param">String parameters from the form.</param>
    /// <param name="cameraFile">Optional file-name of your custom camera definition (camera script?).</param>
    public override void Update (string param, string cameraFile)
    {
      // {{ Put your parameter-parsing code here

      Dictionary<string, string> p = Util.ParseKeyValueList(param);
      if (p.Count > 0)
      {
        Util.TryParse(p, "loop", ref loop);
        Util.TryParse(p, "acc", ref acceleration);
      }

      Time = Util.Clamp(Time, MinTime, MaxTime);

      // Put your camera-definition-file parsing here.

      if (cameraFile != null && cameraFile != "")
      {
        StreamReader reader = null;

        try
        {
          reader = new StreamReader(cameraFile);
          keyFrames = new List<KeyFrame>();

          // first line, adding twice
          if (reader.EndOfStream)
          {
            throw new IOException("Empty input file.");
          }
          Dictionary<string, string> line = Util.ParseKeyValueList(reader.ReadLine());

          double t = 0.0; // necessary init
          if (!Util.TryParse(line, "t", ref t))
          {
            throw new IOException("Incorrect time input.");
          }

          List<double> vectorParts = new List<double>();
          Vector3d pos;
          if(Util.TryParse(line, "pos", ref vectorParts, ';') && vectorParts.Count == 3)
          {
            pos = new Vector3d(vectorParts[0], vectorParts[1], vectorParts[2]);
          }
          else
          {
            pos = (Vector3d)(Center + new Vector3(Diameter, 0f, 0f));
          }

          Vector3d lookat;
          if (Util.TryParse(line, "lookat", ref vectorParts, ';') && vectorParts.Count == 3)
          {
            lookat = new Vector3d(vectorParts[0], vectorParts[1], vectorParts[2]);
          }
          else
          {
            lookat = (Vector3d)(Center);
          }

          Vector3d up;
          if (Util.TryParse(line, "up", ref vectorParts, ';') && vectorParts.Count == 3)
          {
            up = new Vector3d(vectorParts[0], vectorParts[1], vectorParts[2]);
          }
          else
          {
            up = Vector3d.UnitY;
          }

          KeyFrame keyFrame = new KeyFrame(t, pos, lookat, up);
          keyFrames.Add(keyFrame);
          keyFrames.Add(keyFrame);

          while (!reader.EndOfStream)
          {
            line = Util.ParseKeyValueList(reader.ReadLine());

            if(!Util.TryParse(line, "t", ref t) || t <= keyFrames[keyFrames.Count - 1].Time)
            {
              throw new IOException("Incorrect time input.");
            }

            if (Util.TryParse(line, "pos", ref vectorParts, ';') && vectorParts.Count == 3)
            {
              pos = new Vector3d(vectorParts[0], vectorParts[1], vectorParts[2]);
            }
            else
            {
              pos = keyFrames[keyFrames.Count - 1].Position;
            }

            if (Util.TryParse(line, "lookat", ref vectorParts, ';') && vectorParts.Count == 3)
            {
              lookat = new Vector3d(vectorParts[0], vectorParts[1], vectorParts[2]);
            }
            else
            {
              lookat = keyFrames[keyFrames.Count - 1].LookAt;
            }

            if (Util.TryParse(line, "up", ref vectorParts, ';') && vectorParts.Count == 3)
            {
              up = new Vector3d(vectorParts[0], vectorParts[1], vectorParts[2]);
            }
            else
            {
              up = keyFrames[keyFrames.Count - 1].Up;
            }

            keyFrame = new KeyFrame(t, pos, lookat, up);
            keyFrames.Add(keyFrame);
          }

          // add the last item again
          keyFrames.Add(keyFrame);
          if(keyFrames.Count < 4)
          {
            throw new IOException("At least 2 keyframes are needed for an animation.");
          }

          if (loop)
          {
            if(keyFrames.Count < 5)
            {
              throw new IOException("At least 3 keyframes are needed for an animation to loop.");
            }
            else
            {
              keyFrames[0] = keyFrames[keyFrames.Count - 3];
              keyFrames[keyFrames.Count - 1] = keyFrames[2];
            }
          }
        }
        catch (IOException)
        {
          keyFrames = defaultAnimation;
        }
        finally
        {
          if (reader != null)
            reader.Dispose();
        } 
      }
      else
      {
        keyFrames = defaultAnimation;
      }

      MinTime = keyFrames[1].Time;
      MaxTime = keyFrames[keyFrames.Count - 2].Time;
      currentKeyFrame = 1;

      // }}
    }

    /// <param name="param">String parameters from the form.</param>
    /// <param name="cameraFile">Optional file-name of your custom camera definition (camera script?).</param>
    public AnimatedCamera (string param, string cameraFile = "")
    {
      // {{ Put your camera initialization code here

      GenerateDefaultAnimation();

      Update(param, cameraFile);

      // }}
    }

    private List<KeyFrame> defaultAnimation;
    private void GenerateDefaultAnimation ()
    {
      defaultAnimation = new List<KeyFrame>();

      Vector3d lookat = new Vector3d(0d, 0d, 0d);
      Vector3d up = Vector3d.UnitY;

      defaultAnimation.Add(new KeyFrame(3d, new Vector3d(0, 0, -Diameter), lookat, up));
      defaultAnimation.Add(new KeyFrame(0d, new Vector3d(Diameter, 0, 0), lookat, up));
      defaultAnimation.Add(new KeyFrame(1d, new Vector3d(0, 0, Diameter), lookat, up));
      defaultAnimation.Add(new KeyFrame(2d, new Vector3d(-Diameter, 0, 0), lookat, up));
      defaultAnimation.Add(defaultAnimation[0]);
      defaultAnimation.Add(new KeyFrame(4d, new Vector3d(Diameter, 0, 0), lookat, up));
      defaultAnimation.Add(defaultAnimation[2]);
    }

    Matrix4 perspectiveProjection;

    /// <summary>
    /// Returns Projection matrix. Must be implemented.
    /// </summary>
    public override Matrix4 Projection => perspectiveProjection;

    /// <summary>
    /// Called every time a viewport is changed.
    /// It is possible to ignore some arguments in case of scripted camera.
    /// </summary>
    public override void GLsetupViewport (int width, int height, float near = 0.01f, float far = 1000.0f)
    {
      // 1. set ViewPort transform:
      GL.Viewport(0, 0, width, height);

      // 2. set projection matrix
      perspectiveProjection = Matrix4.CreatePerspectiveFieldOfView(Fov, width / (float)height, near, far);
      GLsetProjection();
    }

    private bool loop = false;
    private bool acceleration = false;

    private List<KeyFrame> keyFrames;
    private int currentKeyFrame;

    private readonly Matrix4d catmullRomMatrix = new Matrix4d(-0.5, 1.5, -1.5, 0.5, 1d, -2.5, 2d, -0.5, -0.5, 0d, 0.5, 0d, 0d, 1d, 0d, 0d);

    private double lastTime = 0d;

    /// <summary>
    /// I'm using internal ModelView matrix computation.
    /// </summary>
    Matrix4 computeModelView ()
    {
      if (Time < lastTime)
      {
        currentKeyFrame = 1;
      }
      lastTime = Time;

      while(Time > keyFrames[currentKeyFrame + 1].Time)
      {
        currentKeyFrame++;
      }

      double t = (Time - keyFrames[currentKeyFrame].Time) / (keyFrames[currentKeyFrame + 1].Time - keyFrames[currentKeyFrame].Time);
      if (acceleration)
      {
        if (t < 0.5)
        {
          t = t * t * 2d;
        }
        else
        {
          t = -2d * t * t + 4d * t - 1d;
        }
      }
      Vector4d timeVector = new Vector4d(t * t * t, t * t, t, 1);
      Vector4d baseVector = Multiply(timeVector, catmullRomMatrix);
      Matrix4x3d positionMatrix = new Matrix4x3d(keyFrames[currentKeyFrame - 1].Position, keyFrames[currentKeyFrame].Position, keyFrames[currentKeyFrame + 1].Position, keyFrames[currentKeyFrame + 2].Position);
      Vector3 eye;
      if (keyFrames[currentKeyFrame].Position != keyFrames[currentKeyFrame + 1].Position)
        eye = (Vector3)Multiply(baseVector, positionMatrix);
      else
        eye = (Vector3)keyFrames[currentKeyFrame].Position;
      Matrix4x3d lookAtMatrix = new Matrix4x3d(keyFrames[currentKeyFrame - 1].LookAt, keyFrames[currentKeyFrame].LookAt, keyFrames[currentKeyFrame + 1].LookAt, keyFrames[currentKeyFrame + 2].LookAt);
      Vector3 target;
      if (keyFrames[currentKeyFrame].LookAt != keyFrames[currentKeyFrame + 1].LookAt)
        target = (Vector3)Multiply(baseVector, lookAtMatrix);
      else
        target = (Vector3)keyFrames[currentKeyFrame].LookAt;
      

      return Matrix4.LookAt(eye, target, (Vector3)keyFrames[currentKeyFrame].Up);
    }

    /// <summary>
    /// Crucial property = is called in every frame.
    /// </summary>
    public override Matrix4 ModelView => computeModelView();

    /// <summary>
    /// Crucial property = is called in every frame.
    /// </summary>
    public override Matrix4 ModelViewInv => computeModelView().Inverted();

    public override void Reset ()
    {
      base.Reset();
      currentKeyFrame = 1;
    }

    private Vector4d Multiply (Vector4d left, Matrix4d right)
    {
      Vector4d vector = new Vector4d();
      for (int i = 0; i < 4; i++)
      {
        double sum = 0d;

        for (int j = 0; j < 4; j++)
        {
          sum += left[j] * right[j, i];
        }

        vector[i] = sum;
      }

      return vector;
    }

    private Vector3d Multiply (Vector4d left, Matrix4x3d right)
    {
      Vector3d vector = new Vector3d();

      for (int i = 0; i < 3; i++)
      {
        double sum = 0d;

        for (int j = 0; j < 4; j++)
        {
          sum += left[j] * right[j, i];
        }

        vector[i] = sum;
      }

      return vector;
    }
  }

  class KeyFrame
  {
    public double Time { get; }
    public Vector3d Position { get; }
    public Vector3d LookAt { get; }
    public Vector3d Up { get; }

    public KeyFrame (double t, Vector3d pos, Vector3d lookat, Vector3d up)
    {
      Time = t;
      Position = pos;
      LookAt = lookat;
      Up = up;
    }

    public KeyFrame(double t, double posX, double posY, double posZ, double lookatX, double lookatY, double lookatZ, double upX, double upY, double upZ) : this(t, new Vector3d(posX, posY, posZ), new Vector3d(lookatX, lookatY, lookatZ), new Vector3d(upX, upY, upZ))
    {

    }
  }
}
