using UnityEngine;
using System.Collections.Generic;

public class DynamicTerrain2D : MonoBehaviour
{
    [Header("The camera transform to generate the terrain after a distance")]
    [Space(10)]
    public Transform target;
    [Tooltip("Must be '>' to Horizontal Distance ")]
    public float distanceToRefresh = 50;

    [Space(10)]
    public int initialMapPoints = 10;
    public List<Vector2> SurfacePoints;

    [Header("Terrain Parameters")]
    [Range(1,20)]
    public int LevelOfDetail = 1;
    [Range(1,30)]
    public float mapHeight = 1;
    [Range(1,10)]
    public float smouthFactor = 5;
    
    [Space(10)]
    [Header("Random generation Parameters")]
    public float maxUpHeight = 10;
    public bool useRandomHorizontalDistance;
    public int horizontalDistance = 20;

    [Space(10)]
    [Header("If you want to generate some stuff on the terrain those are the parametes")]
    public bool generateStuff;
    public GameObject objectToBeGenerated;
    public int totalWidth;
    public int width;
    
    private void Update()
    {
        RegenerateWithAnObject();
    }

    public void RefreshTerrain()
     {
         if (LevelOfDetail < 1 || mapHeight <= 0 || SurfacePoints.Count < 2)
         {
             Debug.LogError("Generate terrain failed: LevelOfDetail < 1 or Height <= 0 or SurfacePoints.Length < 2");
             return;
         }

         Mesh mesh;
         mesh = new Mesh();
         gameObject.GetComponent<MeshFilter>().mesh = mesh;

         Vector3[] upper_verts = new Vector3[(SurfacePoints.Count - 1) * LevelOfDetail + 1];
         Vector3[] bottom_verts = new Vector3[upper_verts.Length];
        

         for (int i = 0; i < SurfacePoints.Count - 1; i++)
         {
            for (int j = 0; j <= LevelOfDetail; j++)
             {
                Vector3 nextPosDir = SurfacePoints[i + 1] - SurfacePoints[i];
                
                //Smooth y of points
                float yc = easeInOutSine(SurfacePoints[i].y, SurfacePoints[i + 1].y, (float)(j) / LevelOfDetail);
                nextPosDir *= (float)(j) / LevelOfDetail;
                nextPosDir = new Vector3(nextPosDir.x, yc, nextPosDir.z);
                upper_verts[i * LevelOfDetail + j] = new Vector3(SurfacePoints[i].x, SurfacePoints[i].y, 0) + nextPosDir;

                if(generateStuff)
                    GenerateStuff(width, upper_verts[i * LevelOfDetail + j] + new Vector3(.5f,.5f,0));
             }
         }

         for (int i = 0; i < upper_verts.Length; i++)
         {
             bottom_verts[i] = new Vector3(upper_verts[i].x, upper_verts[i].y - mapHeight, upper_verts[i].z);
         }


         int[] tris = new int[upper_verts.Length * 3 * 2];
         for (int i = 0, j = 0; i < upper_verts.Length - 1; i++, j += 6)
         {
             tris[j] = i;
             tris[j + 1] = i + upper_verts.Length + 1;
             tris[j + 2] = i + upper_verts.Length;

             tris[j + 3] = i;
             tris[j + 4] = i + 1;
             tris[j + 5] = i + upper_verts.Length + 1;
         }

         Vector3[] verts = new Vector3[upper_verts.Length + bottom_verts.Length];
         Vector2[] uvs = new Vector2[verts.Length];

         upper_verts.CopyTo(verts, 0);
         bottom_verts.CopyTo(verts, upper_verts.Length);

         for (int i = 0; i < uvs.Length; i++)
         {
             uvs[i] = new Vector2(verts[i].x, verts[i].y);
         }

         mesh.Clear();
         mesh.vertices = verts;
         mesh.triangles = tris;
         mesh.uv = uvs;
         mesh.RecalculateNormals();


         // Generate polygon colloder
         Vector2[] colPoints = new Vector2[upper_verts.Length + bottom_verts.Length];
         for (int i = 0, j = bottom_verts.Length - 1; i < upper_verts.Length; i++, j--)
         {
             //upper points
             colPoints[i] = new Vector2(upper_verts[i].x, upper_verts[i].y);
             //bottom points
             colPoints[upper_verts.Length + i] = new Vector2(bottom_verts[j].x, bottom_verts[j].y);
         }
         gameObject.GetComponent<PolygonCollider2D>().points = colPoints;
     }
       
    public void GenerateMap()
    {
        SetNewPoint();
    }

    float easeInOutSine(float start, float end, float val)
        {
            end -= start;
            return -end / 2 * (Mathf.Cos(Mathf.PI * val / 1) - 1);
        }

    private Vector2 GetNewPoint()
    {
        float newPointX = 0;
        float newPointY = 0;
        
        if(useRandomHorizontalDistance)
        {
            newPointX = SurfacePoints[SurfacePoints.Count - 1].x + (Random.Range(1, 4) * 10);
        }
        else
        {
            newPointX = SurfacePoints[SurfacePoints.Count - 1].x + horizontalDistance;

        }
        newPointY = Random.Range(0f, maxUpHeight);
    
        while (Mathf.Abs(SurfacePoints[SurfacePoints.Count - 1].y - newPointY) > (11 - smouthFactor))
            newPointY = Random.Range(0f, maxUpHeight);
            
        return new Vector2(newPointX, newPointY);
    }

    public void SetNewPoint()
        {
            if (SurfacePoints.Count < initialMapPoints)
            {
                for (int i = SurfacePoints.Count; i < initialMapPoints; i++)
                {
                    SurfacePoints.Add(GetNewPoint());
                }
            }

            SurfacePoints.Add(GetNewPoint());
            SurfacePoints.RemoveAt(0);
            RefreshTerrain();
        }

    public void RegenerateWithAnObject()
    {
        if (target.transform.position.x > (SurfacePoints[0].x + distanceToRefresh))
            SetNewPoint();
    }

    public void GenerateStuff(int width, Vector3 pos)
    {
        int doesGenerate = Random.Range(0, totalWidth);

        if (doesGenerate < width)
        {
            GameObject obj = Instantiate(objectToBeGenerated, pos, Quaternion.identity) as GameObject;
            obj.transform.parent = this.transform.GetChild(0).transform;
        }
    }
}
    
