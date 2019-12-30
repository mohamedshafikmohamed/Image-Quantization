using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    public struct Color
    {
        public RGBPixel val;
        public int index;
    };

    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }




        
        /// <summary>
        /// get the distinected color in a list of pair 
        /// 1- counter of the color and
        /// 2- the rgb color itself 
        /// from the rgb pixel array to a Dictionary
        /// </summary>
        /// <param name="ImageMatrix"></param>
        /// <returns>Dictionary of distinected color and its number </returns>
        
        private static List<RGBPixel> List_of_Dstinected_Color(RGBPixel[,] ImageMatrix)
        {
            List<RGBPixel> list_of_dstinected_color = new List<RGBPixel>();//O(1)
            bool[,,] color_state = new bool[256, 256, 256];//O(1)
            for (int i=0;i<=255; i++)//O(1)
            {
                for (int j = 0; j <= 255; j++)//O(1)
                {
                    for (int k = 0; k <= 255; k++)//O(1)
                    {
                        color_state[i, j, k] = false;//O(1)
                    }
                }
            }
            int Height = ImageMatrix.GetLength(0);//O(1)
            int Width = ImageMatrix.GetLength(1);//O(1)

            for (int i = 0; i < Height; i++)//O(N) --> N = Height == Width
            {
                for (int j = 0; j < Width; j++)//O(N) --> N = Height == Width

                {
                    int ii = ImageMatrix[i, j].red;//O(1)
                    int jj = ImageMatrix[i, j].green;//O(1)
                    int kk = ImageMatrix[i, j].blue;//O(1)
                    if (color_state[ii,jj,kk]==false)//O(1)
                    {
                        list_of_dstinected_color.Add(ImageMatrix[i, j]);//O(1)
                        color_state[ii, jj, kk] = true;//O(1)
                    }
                }
            }
            //Total looping complexity = E(N^2) --> N = Height == Width
            return list_of_dstinected_color;

            //Total function's complexity = E(N^2)
        }
        private static List<Color> list_of_indexed_color(List<RGBPixel> h)
        {
            List<Color> index_color = new List<Color>();//O(1)
            for (int i = 0; i < h.Count; i++)//O(N) -> N = number of distinct colors
            {
                Color c1 = new Color();//O(1)
                c1.index = i;//O(1)
                c1.val = h[i];//O(1)
                index_color.Add(c1);//O(1)
            }
            return index_color;//O(1)
            //Total function's complexity = E(N)
        }
        public static long Get_Number_of_color(RGBPixel[,] ImageMatrix)
        {
            return List_of_Dstinected_Color(ImageMatrix).Count;//O(1)
            //Total function's complexity = E(1)
        }

        public static MST MST_Weight(RGBPixel[,] ImageMatrix)
        {
            long size;//O(1)
            List<RGBPixel> color_list = List_of_Dstinected_Color(ImageMatrix);//O(1)
            List<Color> index_color = list_of_indexed_color(color_list);//O(1)
            List<Node> tree = new List<Node>();//Exact(1)
            heap heap = new heap();//O(1)

            size = color_list.Count;//O(1)
            bool[] k = new bool[size];//O(1)
            int[] p = new int[size];//O(1)
            double[] d = new double[size];//O(1)

            for (int i = 1; i < size; i++)//O(N) --> N = number of distinct colors
            {
                k[i] = false;//O(1)
                d[i] = double.MaxValue;//O(1)
                p[i] = -1;//O(1)
            }
            //total loop -> O(N) --> N = number of distinct colors
            p[0] = -1;//O(1)
            d[0] = 0;//O(1)
            //node
            Node node0 = new Node();//O(1)
            node0.index = -1;//O(1)
            node0.weight = 0;//O(1)
            node0.to = 0;//O(1)
            heap.insert(node0);//O(log(V))

            while (!heap.empty())//O(V)
            {
                //node
                Node node = heap.extract_Min();//O(Log(V))
                int index = node.to;//O(1)
                if (k[index] == true)//O(1)
                    continue;//O(1)
                k[index] = true;//O(1)
                RGBPixel color1 = index_color[index].val;//O(1)
                int i = 0;//O(1)
                tree.Add(node);//O(1)
                foreach (var color2 in color_list)//O(V) --> V = Number of distinct colors
                {
                    if (i != index)//O(1)
                    {
                        double Red_Diff = (color1.red - color2.red) * (color1.red - color2.red);//O(1)
                        double Green_Diff = (color1.green - color2.green) * (color1.green - color2.green);//O(1)
                        double Blue_Diff = (color1.blue - color2.blue) * (color1.blue - color2.blue);//O(1)
                        double Total_Diff = Math.Sqrt(Red_Diff + Green_Diff + Blue_Diff);//O(1)

                        if (k[i] == false && Total_Diff < d[i])//O(1)
                        {
                            d[i] = Total_Diff;//O(1)
                            p[i] = index;//O(1)
                            Node node1 = new Node();//O(1)
                            node1.weight = Total_Diff;//O(1)
                            node1.index = index;//O(1)
                            node1.to = i;//O(1)
                            heap.insert(node1);//O(Log(V))
                            
                        }
                    }
                        i++;
                }
                //total loop --> O(V * Log(V))
            }
            double weight = 0;//O(1)
            for (int i = 0; i < size; i++)//O(N) --> N = number of distinct colors
            {
                weight += d[i];//O(1)
            }
            //total loop --> O(N)
            MST mST = new MST(weight, tree , p , index_color);//O(1)
            return mST;//O(1)

            //total function's complexity ---> O(E * log V)
        }

        public static RGBPixelD[,,] Extract_color_palette(MST mst,int Num_of_clusters)
        {

            RGBPixelD[ , , ] Color_palette = new RGBPixelD[256, 256, 256];//O(1)
            bool[] v = new bool[mst.parent.Length];//O(1)
            List<node2>[] adj = new List<node2>[mst.parent.Length];//O(1)

            ///<summary>
            ///make cluster of a tree
            /// </summary>
            for (int l = 0; l < Num_of_clusters - 1; l++)  //E(K)
            {
                int i = 0;//O(1)
                int index = 0;//O(1)
                double W = double.MinValue;//O(1)
                foreach (Node color in mst.tree) //E(D)
                {
                    if (W < color.weight)//O(1)
                    {
                        W = color.weight;//O(1)
                        index = i;//O(1)
                    }
                    i++;//O(1)
                }
                //total loop --> O(D) --> D = number of distinct colors

                mst.tree.RemoveAt(index);//O(D)  
            }
            //total loop Order --> E(D*K)

            ///<summary>
            ///convert clustered tree from list representation to adjacent list representation
            /// </summary>
            for (int r = 0; r < mst.parent.Length; r++)
            {
                v[r] = false;//O(1)
            }
            //total loop --> O(D) --> D = number of distinct colors

            for (int i = 0; i < mst.parent.Length; i++) //O(D) --> D = number of distinct colors
            {
                adj[i] = new List<node2>();//O(1)
            }
            //total loop --> O(D) --> D = number of distinct colors

            int k = 0;//O(1)
            foreach (Node color in mst.tree)   //E(D)
            {

                if (k == 0)//O(1)
                {
                    k++;//O(1)
                    continue;//O(1)
                }
                node2 n = new node2();   //O(1)
                node2 n2 = new node2(); //O(1)
                n.weight = color.weight; //O(1)
                n.to = color.to;//O(1)
                n2.weight = color.weight; //O(1)
                n2.to = color.index; //O(1)
                adj[color.index].Add(n);  //O(1)
                adj[color.to].Add(n2);//O(1)
            }
            ///<summary>
            ///for loop to catch each root of the cluster and move forward 
            /// to all his adjacent and make a pallete with new color
            /// </summary>
            for (int r = 0; r < mst.parent.Length; r++) // Exact(D) --> total number of disinct colors
            {
                if (v[r] == false)//O(1)
                {
                    avgcolor_BFS(mst.index_color, r, Color_palette, v, adj);//Exact(E + D) 
                }
            }
            //total for loop ---> Exact (K * D) -->  K = number of clusters, D = number of distinct colors
            //BFS was called , K = number of clusters

            return Color_palette;
        }
        //  total function's complexity --> Exact (K * D) -->  K = number of clusters, D = number of distinct colors


        public static void avgcolor_BFS(List<Color> index_color, int Pindex, RGBPixelD[,,] pallete, bool[] v, List<node2>[] adj)
        {


            int count = 1; //O(1)
            RGBPixelD avg = new RGBPixelD(); //O(1)
            Queue<int> Q = new Queue<int>(); //O(1)
            List<RGBPixel> ch = new List<RGBPixel>(); //O(1)


            Q.Enqueue(Pindex);//O(1)
            while (Q.Count != 0) //Exact(E) --> E = number of edges in cluster
            {
                int root = Q.Dequeue();
                RGBPixel ch_color = index_color[root].val; //O(1)
                ch.Add(ch_color);//O(1)
                avg.red += ch_color.red; //O(1)
                avg.blue += ch_color.blue; //O(1)
                avg.green += ch_color.green; //O(1)


                for (int r = 0; r < adj[root].Count; r++) //Exact(adj(D)) --> D = number of nodes
                {
                    if (v[adj[root][r].to] == false)//O(1)
                    {
                        Q.Enqueue(adj[root][r].to);//O(1)
                        count++; // O(1)
                    }
                }
                // total for loop --> Exact(adj(D)) --> D = number of nodes

                v[root] = true;//O(1)
            }
            // total while loop --> Exact(E) --> E = number of edges in cluster
            avg.red /= count; //O(1)
            avg.blue /= count; //O(1)
            avg.green /= count; //O(1)

            //assign the the cluster color with its represented color in the pallete
            foreach (var ch_color1 in ch) // O(D)--> D = number of Distinct colors in this cluster 
                pallete[ch_color1.red, ch_color1.blue, ch_color1.green] = avg; // O(1)
        }
        // total function's complexity --> Exact(E + D) --> E = number of edges in cluster , D = number of Distinct colors in this cluster 



        /// <summary>
        /// replace the color of each pixal in the image matrix array
        /// with its represented color in the pallete
        /// </summary>
        /// <param name="ImageMatrix"></param>
        /// <param name="pallete"></param>
        public static void  Quntization(RGBPixel[,] ImageMatrix,RGBPixelD[,,] pallete)
        {
            int width = ImageMatrix.GetLength(1); // O(1)
            int height = ImageMatrix.GetLength(0); // O(1)
            for (int i = 0; i < height; i++)// E(N) --> N = height
            {
                for (int j = 0; j < width; j++)// E(N) --> N = width
                {
                    var index = ImageMatrix[i, j]; // O(1)
                    var col = pallete[index.red, index.blue, index.green]; // O(1)
                    ImageMatrix[i, j].red = (byte)col.red; // O(1)
                    ImageMatrix[i, j].green = (byte)col.green;  // O(1)
                    ImageMatrix[i, j].blue = (byte)col.blue;  // O(1)
                }
            }
        }//Total_complexity = E(N^2) --> N = width or height


    }
}