using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACVI_Eigenfaces.Logic
{
    public static class ImageManager
    {
        private static int width;
        private static int height;
        public static byte[] ConvertImageToVector(Bitmap Image)
        {
            byte[] vector = new byte[Image.Width*Image.Height];

            height = Image.Height;
            width = Image.Width;

            int positionCounter = 0;

            for(int i = 0; i<Image.Width;i++)
            {
                for(int j = 0; j<Image.Height;j++)
                {
                    vector[positionCounter] = Image.GetPixel(i, j).R;
                    Color color = Image.GetPixel(i, j);
                    positionCounter++;
                }
            }
            
            return vector;
        }

        public static Bitmap ConvertVectorToImage(byte[] pixels)
        {
            Bitmap image = new Bitmap(width, height);

            int positionCounter = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color color = Color.FromArgb(255, pixels[positionCounter], pixels[positionCounter],
                                                 pixels[positionCounter]);
                    positionCounter++;
                    image.SetPixel(i,j,color);
                }
            }
            return image;
        }

        //public BufferedImage ConvertVectorToImage(int[] ImageVector)
        //{
        
        //}
    }
}
