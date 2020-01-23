using System;
using System.Collections.Generic;
using MathSupport;
using OpenTK;
using Utilities;

namespace Scene3D
{
  public class Construction
  {
    #region Form initialization

    /// <summary>
    /// Optional form-data initialization.
    /// </summary>
    /// <param name="name">Return your full name.</param>
    /// <param name="param">Optional text to initialize the form's text-field.</param>
    /// <param name="tooltip">Optional tooltip = param help.</param>
    public static void InitParams (out string name, out string param, out string tooltip)
    {
      name    = "Eliáš Cizl";
      param   = "radius=1,neighbors=true,center=true,depth=0,lift=1.0";
      tooltip = "radius=<radius>, neighbors=<bool>, center=<bool>, depth=<uint>, lift=<float>";
    }

    #endregion

    #region Instance data

    // !!! If you need any instance data, put them here..

    private float radius;
    private bool neighbors; // if the lines between vertices that are next to each other should be displayed
    private bool center; // if lines connecting the vertices to the center should be displayed
    private int depth; // how many vertices (recursively) should be inside faces
    private float lift; // how much should be the vertices lifted from the center

    private void parseParams (string param)
    {
      // Defaults.
      radius = 1.0f;
      neighbors = true;
      center = true;
      depth = 1;
      lift = 1.0f;

      Dictionary<string, string> p = Util.ParseKeyValueList(param);
      if (p.Count > 0)
      {
        Util.TryParse(p, "radius", ref radius);

        Util.TryParse(p, "neighbors", ref neighbors);

        Util.TryParse(p, "center", ref center);

        if (Util.TryParse(p, "depth", ref depth) && depth < 0)
          depth = 0;
        if (Util.TryParse(p, "lift", ref lift) && lift < 0f)
          lift = 1.0f;
      }
    }

    #endregion

    public Construction ()
    {

    }

    #region Mesh construction

    private Vertex[] baseVertices;

    /// <summary>
    /// Construct a new Brep solid (preferebaly closed = regular one).
    /// </summary>
    /// <param name="scene">B-rep scene to be modified</param>
    /// <param name="m">Transform matrix (object-space to world-space)</param>
    /// <param name="param">Shape parameters if needed</param>
    /// <returns>Number of generated faces (0 in case of failure)</returns>
    public int AddMesh (SceneBrep scene, Matrix4 m, string param)
    {
      parseParams(param);

      int elements = 6 + (depth > 0 ? 8 * (int)Math.Pow(3, depth - 1) : 0) + (center ? 1 : 0);

      scene.Reserve(elements);

      int middle = scene.AddVertex(Vector3.TransformPosition(new Vector3(0, 0, 0), m));

      GenerateBaseVertices();
      GenerateSubVertices();

      // Generate int positions
      {
        Queue<Vertex> currentVertices = new Queue<Vertex>(baseVertices);

        while (currentVertices.Count > 0)
        {
          Vertex currentVertex = currentVertices.Dequeue();
          currentVertex.Position = scene.AddVertex(Vector3.TransformPosition(currentVertex.Vector3, m));

          for (int i = 0; i < currentVertex.SubVertices.Count; i++)
          {
            currentVertices.Enqueue(currentVertex.SubVertices[i]);
          }
        }
      }


      if (neighbors)
      {
        Queue<Vertex> currentVertices = new Queue<Vertex>(baseVertices);

        while(currentVertices.Count > 0)
        {
          Vertex currentVertex = currentVertices.Dequeue();
          for (int i = 0; i < currentVertex.Neighbors.Count; i++)
          {
            scene.AddLine(currentVertex.Position, currentVertex.Neighbors[i].Position);
          }

          for (int i = 0; i < currentVertex.SubVertices.Count; i++)
          {
            currentVertices.Enqueue(currentVertex.SubVertices[i]);
          }
        }
      }

      if (center)
      {
        Queue<Vertex> currentVertices = new Queue<Vertex>(baseVertices);

        while (currentVertices.Count > 0)
        {
          Vertex currentVertex = currentVertices.Dequeue();
          scene.AddLine(currentVertex.Position, middle);

          for (int i = 0; i < currentVertex.SubVertices.Count; i++)
          {
            currentVertices.Enqueue(currentVertex.SubVertices[i]);
          }
        }
      }

      scene.LineWidth = 3.0f;

      return elements;
    }

    private void GenerateBaseVertices ()
    {
      baseVertices = new Vertex[6];

      baseVertices[0] = new Vertex(new Vector3(radius, 0, 0));
      baseVertices[1] = new Vertex(new Vector3(0, radius, 0));
      baseVertices[2] = new Vertex(new Vector3(0, 0, radius));
      baseVertices[3] = new Vertex(new Vector3(0, 0, -radius));
      baseVertices[4] = new Vertex(new Vector3(0, -radius, 0));
      baseVertices[5] = new Vertex(new Vector3(-radius, 0, 0));

      for (int i = 0; i < baseVertices.Length; i++)
      {
        for (int j = i + 1; j < baseVertices.Length; j++)
        {
          if(i + j != 5)
          {
            baseVertices[i].Neighbors.Add(baseVertices[j]);
            baseVertices[j].Neighbors.Add(baseVertices[i]);
          }
        }
      }
    }

    private Vector3 GetCenterPoint (Vector3 v1, Vector3 v2, Vector3 v3)
    {
      Vector3 result = v1 + v2 + v3;
      return result / 3;
    }

    private void AddBaseSubVertex(List<Vertex> vertices, int baseIndex, int secondIndex, int thirdIndex)
    {
      AddSubVertex(vertices, baseVertices[baseIndex], baseVertices[secondIndex], baseVertices[thirdIndex]);
    }

    private void AddSubVertex (List<Vertex> vertices, Vertex startVertex, Vertex v2, Vertex v3)
    {
      Vertex vertex = new Vertex(GetCenterPoint(startVertex.Vector3, v2.Vector3, v3.Vector3) * lift);
      vertex.Neighbors.Add(startVertex);
      vertex.Neighbors.Add(v2);
      vertex.Neighbors.Add(v3);
      vertices.Add(vertex);
      startVertex.SubVertices.Add(vertex);
    }

    private void GenerateSubVertices ()
    {
      List<Vertex> lastVertices = new List<Vertex>(8);
      if(depth >= 1)
      {
        AddBaseSubVertex(lastVertices, 0, 1, 2);
        AddBaseSubVertex(lastVertices, 0, 2, 4);
        AddBaseSubVertex(lastVertices, 0, 4, 3);
        AddBaseSubVertex(lastVertices, 0, 3, 1);
        AddBaseSubVertex(lastVertices, 5, 1, 2);
        AddBaseSubVertex(lastVertices, 5, 2, 4);
        AddBaseSubVertex(lastVertices, 5, 4, 3);
        AddBaseSubVertex(lastVertices, 5, 3, 1);
      }

      for (int i = 1; i < depth; i++)
      {
        List<Vertex> newVertices = new List<Vertex>(lastVertices.Count * 3);
        for (int j = 0; j < lastVertices.Count; j++)
        {
          AddSubVertex(newVertices, lastVertices[j], lastVertices[j].Neighbors[0], lastVertices[j].Neighbors[1]);
          AddSubVertex(newVertices, lastVertices[j], lastVertices[j].Neighbors[1], lastVertices[j].Neighbors[2]);
          AddSubVertex(newVertices, lastVertices[j], lastVertices[j].Neighbors[0], lastVertices[j].Neighbors[2]);
        }
        lastVertices = newVertices;
      }
    }

    #endregion
  }

  class Vertex
  {
    public Vertex(Vector3 vector3)
    {
      Vector3 = vector3;
      SubVertices = new List<Vertex>();
      Neighbors = new List<Vertex>();
    }

    public Vector3 Vector3 { get; set; }
    public int Position { get; set; }
    public List<Vertex> SubVertices { get; private set; }

    public List<Vertex> Neighbors { get; private set; }
  }
}
