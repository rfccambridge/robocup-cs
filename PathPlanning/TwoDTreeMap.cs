using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RFC.Core;
using RFC.Geometry;

namespace RFC.PathPlanning
{
    //This is an implemenation of a map from Vector2 to any desired object type which allows fast lookups of nearest points.
    //The underlying implementaiton is a k-d tree where k = 2. 

    //That is, points are stored recursively in a binary tree that alternately partitions along each dimension, 
    //giving nested boxes. This allows fast lookups by distance since you can simply skip searching entire nodes 
    //of the tree (and all the points in them) if the box represented by that node is too far away to matter.
    public class TwoDTreeMap<T>
    {
        //Parameters
        const int SPLIT_THRESHOLD = 12;  //Split a node when it has this many points in it
        const int MAX_DEPTH = 16;        //But never split more than this many levels deep

        //The root node of the tree
        private TwoDTreeMapNode _root;

        //Construct a tree map for tracking points within the given bounds. The map should still work for points
        //outside of these bounds, but these bounds will be used to determine how to subdivide the tree, so they should be
        //generally chosen to encompass the entire 2D area you're interested in.
        public TwoDTreeMap(double xMin, double xMax, double yMin, double yMax)
        {
            _root = new TwoDTreeMapNode(null, null, xMin, xMax, yMin, yMax, 0);
        }

        //Return the number of points in the map.
        public int Size()
        {
            return _root.num;
        }

        //Add a point to the map
        public void Add(Vector2 vec, T obj)
        {
            _root.AddAndMaybeSplit(vec, obj);
        }

        //Get the nearest neighbor to vec, if one exists, else returns a pair of (null, default(T)).
        public Tuple<Vector2, T> NearestNeighbor(Vector2 vec)
        {
            Vector2 bestVec = null;
            T bestT = default(T);
            double nearestDistSq = double.PositiveInfinity;
            _root.NearestNeighbor(vec, ref bestVec, ref bestT, ref nearestDistSq);
            return new Tuple<Vector2, T>(bestVec, bestT);
        }

        //Get the nearest k neighbors. If fewer than k neighbors exist, returns all of them.
        //Neighbors are NOT guaranteed to be sorted by distance.
        public Tuple<List<Vector2>, List<T>> NearestK(Vector2 vec, int k)
        {
            List<Vector2> bestVecs = new List<Vector2>();
            List<T> bestTs = new List<T>();
            double worstDistSqSoFar = double.PositiveInfinity;
            int numVecs = 0;
            int worstVecId = -1;
            _root.NearestK(vec, k, ref bestVecs, ref bestTs, ref worstDistSqSoFar, ref numVecs, ref worstVecId);
            return new Tuple<List<Vector2>, List<T>>(bestVecs, bestTs);
        }

        //Print this tree down to the specified depth
        public void PrintToDepth(int depth)
        {
            _root.PrintToDepth(depth);
        }


        //All the work is done here
        class TwoDTreeMapNode
        {
            //Data storage
            public int num;
            public List<Vector2> vectors;
            public List<T> objects;

            //Tree pointers
            public TwoDTreeMapNode left;
            public TwoDTreeMapNode right;
            public TwoDTreeMapNode parent;
            public TwoDTreeMapNode root;

            //Cut
            public int cutDimension;
            public int depth;

            //Bounds
            public double xMin;
            public double xMax;
            public double yMin;
            public double yMax;

            public TwoDTreeMapNode(TwoDTreeMapNode root, TwoDTreeMapNode parent, double xMin, double xMax, double yMin, double yMax, int cutDimension)
            {
                num = 0;
                vectors = new List<Vector2>(SPLIT_THRESHOLD + 1);
                objects = new List<T>(SPLIT_THRESHOLD + 1);

                left = null;
                right = null;
                this.parent = parent;
                this.root = root;
                if (this.root == null)
                    this.root = this;

                this.cutDimension = cutDimension;
                depth = (parent == null) ? 0 : parent.depth + 1;

                this.xMin = xMin;
                this.xMax = xMax;
                this.yMin = yMin;
                this.yMax = yMax;
            }

            private double GetCut(double min, double max)
            {
                return (max + min) / 2;
            }

            public bool IsLeaf()
            {
                return vectors != null;
            }

            //Returns the child that contains this point.
            public TwoDTreeMapNode GetChild(Vector2 pt)
            {
                if (IsLeaf())
                {
                    System.Console.WriteLine("TwoDTreeMapNode getChild on LEAF!");
                    return null;
                }

                if (cutDimension == 0)
                    return pt.X < GetCut(xMin, xMax) ? left : right;
                else
                    return pt.Y < GetCut(yMin, yMax) ? left : right;
            }

            //Returns the next place to cut this node for further subdivision
            public double GetCut()
            {
                double min, max;
                if (cutDimension == 0)
                { min = xMin; max = xMax; }
                else
                { min = yMin; max = yMax; }

                return GetCut(min, max);
            }

            //Add a point and split if we cross the threshold for too many points.
            //Not recursive, but does split if we get too many things in here
            public void AddAndMaybeSplit(Vector2 vec, T obj)
            {
                if (!IsLeaf())
                {
                    TwoDTreeMapNode node = this;
                    while (!node.IsLeaf())
                    {
                        node.num++;
                        node = node.GetChild(vec);
                    }
                    node.AddAndMaybeSplit(vec, obj);
                    return;
                }

                num++;
                vectors.Add(vec);
                objects.Add(obj);
                if (vectors.Count >= SPLIT_THRESHOLD && depth < MAX_DEPTH)
                {
                    if (cutDimension == 0)
                    {
                        double cut = GetCut(xMin, xMax);
                        left = new TwoDTreeMapNode(root, this, xMin, cut, yMin, yMax, 1);
                        right = new TwoDTreeMapNode(root, this, cut, xMax, yMin, yMax, 1);

                        for (int i = 0; i < vectors.Count; i++)
                        {
                            if (vectors[i].X < cut)
                            { left.vectors.Add(vectors[i]); left.objects.Add(objects[i]); }
                            else
                            { right.vectors.Add(vectors[i]); right.objects.Add(objects[i]); }
                        }
                    }
                    else
                    {
                        double cut = GetCut(yMin, yMax);
                        left = new TwoDTreeMapNode(root, this, xMin, xMax, yMin, cut, 0);
                        right = new TwoDTreeMapNode(root, this, xMin, xMax, cut, yMax, 0);

                        for (int i = 0; i < vectors.Count; i++)
                        {
                            if (vectors[i].Y < cut)
                            { left.vectors.Add(vectors[i]); left.objects.Add(objects[i]); }
                            else
                            { right.vectors.Add(vectors[i]); right.objects.Add(objects[i]); }
                        }
                    }
                    left.num = left.vectors.Count;
                    right.num = right.vectors.Count;

                    vectors = null;
                    objects = null;
                }
            }

            //Get the squared distance of this point from our bounding box
            //We have a special case if we match the root's border - in that case, we don't consider the distance
            //in that direction, so that we correctly handle distances for points outside the root's bounds.
            public double DistanceSqFrom(Vector2 vec)
            {
                double dx = 0;
                double dy = 0;

                if (vec.X < xMin && root.xMin != xMin) dx = xMin - vec.X;
                else if (vec.X > xMax && root.xMax != xMax) dx = vec.X - xMax;

                if (vec.Y < yMin && root.yMin != yMin) dy = yMin - vec.Y;
                else if (vec.Y > yMax && root.yMax != yMax) dy = vec.Y - yMax;

                return dx * dx + dy * dy;
            }

            //Recursively find the nearest neighbor
            public void NearestNeighbor(Vector2 vec, ref Vector2 bestVec, ref T bestT, ref double nearestDistSqSoFar)
            {
                //Leaf node? Test all the points
                if (IsLeaf())
                {
                    for (int i = 0; i < vectors.Count; i++)
                    {
                        double distSq = vectors[i].distanceSq(vec);
                        if (distSq < nearestDistSqSoFar)
                        {
                            bestVec = vectors[i];
                            bestT = objects[i];
                            nearestDistSqSoFar = distSq;
                        }
                    }
                    return;
                }

                //Not leaf node
                double distanceSqFromLeft = left.DistanceSqFrom(vec);
                double distanceSqFromRight = right.DistanceSqFrom(vec);
                TwoDTreeMapNode first;
                TwoDTreeMapNode second;
                double firstDistSq;
                double secondDistSq;

                //Find the closest child and test that first
                if (distanceSqFromLeft < distanceSqFromRight)
                { first = left; second = right; firstDistSq = distanceSqFromLeft; secondDistSq = distanceSqFromRight; }
                else
                { first = right; second = left; firstDistSq = distanceSqFromRight; secondDistSq = distanceSqFromLeft; }

                //If the closest child is too far away, then we're done
                if (firstDistSq >= nearestDistSqSoFar)
                    return;
                first.NearestNeighbor(vec, ref bestVec, ref bestT, ref nearestDistSqSoFar);

                //If the farther child is too far away, then we're done.
                if (secondDistSq >= nearestDistSqSoFar)
                    return;
                second.NearestNeighbor(vec, ref bestVec, ref bestT, ref nearestDistSqSoFar);
            }

            //Recursively find the nearest k neighbors
            public void NearestK(Vector2 vec, int k, ref List<Vector2> bestVecs, ref List<T> bestTs,
                ref double worstDistSqSoFar, ref int numVecs, ref int worstVecId)
            {
                //Leaf node? Test all the points
                if (IsLeaf())
                {
                    for (int i = 0; i < vectors.Count; i++)
                    {
                        double distSq = vectors[i].distanceSq(vec);
                        //If we haven't found k yet, then add it no matter what
                        if (numVecs < k)
                        {
                            bestVecs.Add(vectors[i]);
                            bestTs.Add(objects[i]);
                            if (distSq > worstDistSqSoFar)
                            { worstDistSqSoFar = distSq; worstVecId = numVecs; }
                            numVecs++;
                        }
                        //Otherwise, add it only if it beats the worst point we've found so far.
                        else if (distSq < worstDistSqSoFar)
                        {
                            bestVecs[worstVecId] = vectors[i];
                            bestTs[worstVecId] = objects[i];
                            //And recompute the worst
                            worstDistSqSoFar = 0;
                            for (int j = 0; j < k; j++)
                            {
                                distSq = bestVecs[j].distanceSq(vec);
                                if (distSq > worstDistSqSoFar)
                                { worstDistSqSoFar = distSq; worstVecId = j; }
                            }
                        }
                    }
                    return;
                }

                //Not a leaf
                double distanceSqFromLeft = left.DistanceSqFrom(vec);
                double distanceSqFromRight = right.DistanceSqFrom(vec);
                TwoDTreeMapNode first;
                TwoDTreeMapNode second;
                double firstDistSq;
                double secondDistSq;

                //Find the closest child and test that first
                if (distanceSqFromLeft < distanceSqFromRight)
                { first = left; second = right; firstDistSq = distanceSqFromLeft; secondDistSq = distanceSqFromRight; }
                else
                { first = right; second = left; firstDistSq = distanceSqFromRight; secondDistSq = distanceSqFromLeft; }

                //If the closest child is too far away, then we're done
                if (firstDistSq >= worstDistSqSoFar)
                    return;
                first.NearestK(vec, k, ref bestVecs, ref bestTs, ref worstDistSqSoFar, ref numVecs, ref worstVecId);

                //If the farther child is too far away, then we're done
                if (secondDistSq >= worstDistSqSoFar)
                    return;
                second.NearestK(vec, k, ref bestVecs, ref bestTs, ref worstDistSqSoFar, ref numVecs, ref worstVecId);
            }

            public void PrintToDepth(int depth)
            {
                for (int i = 0; i < this.depth; i++)
                    Console.Write(" ");
                Console.WriteLine("(" + xMin + "," + xMax + "," + yMin + "," + yMax + ")" + num);
                if (!IsLeaf() && this.depth < depth)
                {
                    left.PrintToDepth(depth);
                    right.PrintToDepth(depth);
                }
            }
        }

        public static void Test()
        {
            Random rand = new Random();
            for (int r = 0; r < 1; r++)
            {
                if (r % 100 == 0)
                    System.Console.WriteLine(r);

                /*
                Vector2NNFinder map = new Vector2NNFinder();
                for (int i = 0; i < 50; i++)
                {
                    map.AddPoint(new Vector2(rand.NextDouble() * 4 - 2, rand.NextDouble() * 20 - 10));
                }


                for (int i = 0; i < 50; i++)
                {
                    Vector2 vec = new Vector2(rand.NextDouble() * 4 - 2, rand.NextDouble() * 20 - 10);
                    Vector2 result = map.NearestNeighbor(vec);
                    //System.Console.WriteLine(vec + " " + result.First + " " + result.Item2);
                }
                */

                TwoDTreeMap<int> map = new TwoDTreeMap<int>(-2, 2, -10, 10);
                for (int i = 0; i < 200000; i++)
                {
                    map.Add(new Vector2(rand.NextDouble() * 4 - 2, rand.NextDouble() * 20 - 10), i);
                }


                for (int i = 0; i < 200; i++)
                {
                    Vector2 vec = new Vector2(rand.NextDouble() * 4 - 2, rand.NextDouble() * 20 - 10);
                    Tuple<Vector2, int> result = map.NearestNeighbor(vec);
                    System.Console.WriteLine(vec + " " + result.Item1 + " " + result.Item2);
                }


            }
            System.Environment.Exit(0);
        }
    }
}
