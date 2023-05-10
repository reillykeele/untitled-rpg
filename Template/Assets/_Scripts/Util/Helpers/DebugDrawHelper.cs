using UnityEngine;
using UnityEngine.AI;

namespace Util.Helpers
{
    public static class DebugDrawHelper
    {
        #region Sphere

        public static void DrawPoint(Vector3 center, float radius, Color color, float duration = 0)
        {
            Debug.DrawRay(center, Vector3.up * radius, color, duration);
            Debug.DrawRay(center, Vector3.down * radius, color, duration);
            Debug.DrawRay(center, Vector3.left * radius, color, duration);
            Debug.DrawRay(center, Vector3.right * radius, color, duration);
            Debug.DrawRay(center, Vector3.forward * radius, color, duration);
            Debug.DrawRay(center, Vector3.back * radius, color, duration);
        }

        /// <summary>
        ///   Draw a wire sphere.
        ///   Code by u/tratteo on Reddit.
        /// </summary>
        /// <param name="center"> </param>
        /// <param name="radius"> </param>
        /// <param name="color"> </param>
        /// <param name="duration"> </param>
        /// <param name="quality"> Define the quality of the wire sphere, from 1 to 10 </param>
        public static void DrawSphere(Vector3 center, float radius, Color color, float duration = 0f, int quality = 3)
        {
            quality = Mathf.Clamp(quality, 1, 10);

            var segments = quality << 2;
            var subdivisions = quality << 3;
            var halfSegments = segments >> 1;
            var strideAngle = 360f / subdivisions;
            var segmentStride = 180f / segments;

            Vector3 first;
            Vector3 next;
            for (int i = 0; i < segments; i++)
            {
                first = (Vector3.forward * radius);
                first = Quaternion.AngleAxis(segmentStride * (i - halfSegments), Vector3.right) * first;

                for (int j = 0; j < subdivisions; j++)
                {
                    next = Quaternion.AngleAxis(strideAngle, Vector3.up) * first;
                    Debug.DrawLine(first + center, next + center, color, duration);
                    first = next;
                }
            }

            Vector3 axis;
            for (int i = 0; i < segments; i++)
            {
                first = (Vector3.forward * radius);
                first = Quaternion.AngleAxis(segmentStride * (i - halfSegments), Vector3.up) * first;
                axis = Quaternion.AngleAxis(90F, Vector3.up) * first;

                for (int j = 0; j < subdivisions; j++)
                {
                    next = Quaternion.AngleAxis(strideAngle, axis) * first;
                    Debug.DrawLine(first + center, next + center, color, duration);
                    first = next;
                }
            }
        }

        #endregion

        #region Cube

        public static void DrawCube(Vector3 center, float sideLength, Color color, float duration = 0)
        {
            DrawBox(center, new Vector3(sideLength/2, sideLength/2, sideLength/2), Quaternion.identity, color);
        }

        #endregion

        #region DrawBoxCast
        // By HiddenMonk source: http://answers.unity.com/answers/1156088/view.html 

        //Draws the full box from start of cast to its end distance. Can also pass in hitInfoDistance instead of full distance
        public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float distance, Color color)
         {
             direction.Normalize();
             Box bottomBox = new Box(origin, halfExtents, orientation);
             Box topBox = new Box(origin + (direction * distance), halfExtents, orientation);
                 
             Debug.DrawLine(bottomBox.backBottomLeft, topBox.backBottomLeft,    color);
             Debug.DrawLine(bottomBox.backBottomRight, topBox.backBottomRight, color);
             Debug.DrawLine(bottomBox.backTopLeft, topBox.backTopLeft, color);
             Debug.DrawLine(bottomBox.backTopRight, topBox.backTopRight,    color);
             Debug.DrawLine(bottomBox.frontTopLeft, topBox.frontTopLeft,    color);
             Debug.DrawLine(bottomBox.frontTopRight, topBox.frontTopRight, color);
             Debug.DrawLine(bottomBox.frontBottomLeft, topBox.frontBottomLeft, color);
             Debug.DrawLine(bottomBox.frontBottomRight, topBox.frontBottomRight,    color);
         
             DrawBox(bottomBox, color);
             DrawBox(topBox, color);
         }
         
         public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color)
         {
             DrawBox(new Box(origin, halfExtents, orientation), color);
         }

         public static void DrawBox(Box box, Color color)
         {
             Debug.DrawLine(box.frontTopLeft,     box.frontTopRight,    color);
             Debug.DrawLine(box.frontTopRight,     box.frontBottomRight, color);
             Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color);
             Debug.DrawLine(box.frontBottomLeft,     box.frontTopLeft, color);
                                                      
             Debug.DrawLine(box.backTopLeft,         box.backTopRight, color);
             Debug.DrawLine(box.backTopRight,     box.backBottomRight, color);
             Debug.DrawLine(box.backBottomRight,     box.backBottomLeft, color);
             Debug.DrawLine(box.backBottomLeft,     box.backTopLeft, color);
                                                      
             Debug.DrawLine(box.frontTopLeft,     box.backTopLeft, color);
             Debug.DrawLine(box.frontTopRight,     box.backTopRight, color);
             Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color);
             Debug.DrawLine(box.frontBottomLeft,     box.backBottomLeft, color);
         }
         
         public struct Box
         {
             public Vector3 localFrontTopLeft     {get; private set;}
             public Vector3 localFrontTopRight    {get; private set;}
             public Vector3 localFrontBottomLeft  {get; private set;}
             public Vector3 localFrontBottomRight {get; private set;}
             public Vector3 localBackTopLeft      {get {return -localFrontBottomRight;}}
             public Vector3 localBackTopRight     {get {return -localFrontBottomLeft;}}
             public Vector3 localBackBottomLeft   {get {return -localFrontTopRight;}}
             public Vector3 localBackBottomRight  {get {return -localFrontTopLeft;}}
     
             public Vector3 frontTopLeft     {get {return localFrontTopLeft + origin;}}
             public Vector3 frontTopRight    {get {return localFrontTopRight + origin;}}
             public Vector3 frontBottomLeft  {get {return localFrontBottomLeft + origin;}}
             public Vector3 frontBottomRight {get {return localFrontBottomRight + origin;}}
             public Vector3 backTopLeft      {get {return localBackTopLeft + origin;}}
             public Vector3 backTopRight     {get {return localBackTopRight + origin;}}
             public Vector3 backBottomLeft   {get {return localBackBottomLeft + origin;}}
             public Vector3 backBottomRight  {get {return localBackBottomRight + origin;}}
     
             public Vector3 origin {get; private set;}
     
             public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
             {
                 Rotate(orientation);
             }
             public Box(Vector3 origin, Vector3 halfExtents)
             {
                 this.localFrontTopLeft     = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
                 this.localFrontTopRight    = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
                 this.localFrontBottomLeft  = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
                 this.localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);
     
                 this.origin = origin;
             }
     
     
             public void Rotate(Quaternion orientation)
             {
                 localFrontTopLeft     = RotatePointAroundPivot(localFrontTopLeft    , Vector3.zero, orientation);
                 localFrontTopRight    = RotatePointAroundPivot(localFrontTopRight   , Vector3.zero, orientation);
                 localFrontBottomLeft  = RotatePointAroundPivot(localFrontBottomLeft , Vector3.zero, orientation);
                 localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
             }
         }

         //This should work for all cast types
         static Vector3 CastCenterOnCollision(Vector3 origin, Vector3 direction, float hitInfoDistance)
         {
             return origin + (direction.normalized * hitInfoDistance);
         }
     
         static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
         {
             Vector3 direction = point - pivot;
             return pivot + rotation * direction;
         }
        #endregion

        #region NavMeshPath

        public static void DrawPath(NavMeshPath path, Color color = default)
        {
            #if DEBUG
            for (int i = 1; i < path.corners.Length; i++)
                Debug.DrawLine(path.corners[i-1], path.corners[i], color);
            #endif
        }

        #endregion
    }
}