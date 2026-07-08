using System;
using System.Text;
using CustomInspector;
using CustomUtils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Labyrinth
{
    [CreateAssetMenu(fileName = "Maze Image Settings", menuName = "Scriptable Objects/Maze Image Settings")]
    public class ImageMap : ScriptableObject
    {
        [Serializable]
        public enum ImagePresets
        {
            blocked = 0,
            valid = 1,
            spawn = 2,
            // end = 3
        }

        [Serializable]
        public class ColorLegend
        {
            
            public Color color = Color.white;
            public ImagePresets type;
        }

        [SerializeField, Preview] private Texture2D _image;
        // public Texture2D OriginalImage => _image;

        [Unfold] public ReorderableDictionary<ImagePresets, Color> colorCodes = new() { {ImagePresets.blocked, Color.black} };

      
        public bool HasValidCells => !(validCells == null || validCells.Length == 0);
        
        
        
        
        
        // Inspector Helpers
        [Button(nameof(BakeCellData)), Button(nameof(ResetData))]
        [SerializeField, ReadOnly, HorizontalLine(5, FixedColor.CherryRed, message = "Baked Values")] bool _baked = false;
         public bool Baked => _baked;
        [ReadOnly] public int BakedMapWidth = -1;
        [ReadOnly] public int BakedMapHeight = -1;
        [ReadOnly] public RectInt BakedImageBounds = RectInt.zero;
        [ReadOnly] public bool[] validCells = null;

        public void ResetData()
        {
            _baked = false;
            BakedMapWidth = BakedMapHeight = -1;
            BakedImageBounds = RectInt.zero;
            validCells = null;
        }

        public void BakeCellData()
        {
            var pixels = _image.GetPixels();
            var start = Array.IndexOf(pixels, colorCodes[ImagePresets.spawn]);
            Debug.Log("Start Pixel is " + GetCoordinatesFromArrayIndex(start, _image.width, _image.height));

            if (start == -1)
            {
                Debug.LogError($"Image of {_image.name} does not provide a valid start point that matches provided start color {colorCodes[ImagePresets.spawn]}");
                return;
            }


            /// Do a DFS for all connected cells to Spawn to find all valid spots
            int imageWidth = _image.width;
            int imageHeight = _image.height;

            validCells = new bool[pixels.Length];

            bool IsValidDFSCell(int index)
            {
                return !pixels[index].Equals(colorCodes[ImagePresets.blocked]) &&
                       !validCells[index];
            }

            void Recursive(int index, int depth = 0)
            {
                if (IsValidDFSCell(index))
                {
                    // Mark validCells 
                    validCells[index] = true;
                    GetCoordinatesFromArrayIndex(index, imageWidth, imageHeight, out var coord);

                    // then add adjacent cells
                    if (coord.x + 1 < imageWidth) Recursive(index + 1, depth+1);// right
                    if (coord.y + 1 < imageHeight) Recursive(index + imageWidth, depth+1); // up
                    if (coord.x - 1 >= 0) Recursive(index - 1, depth+1);// left
                    if (coord.y - 1 >= 0) Recursive(index - imageWidth, depth+1); // down
                }
                // if (depth == 1000) Debug.Log("Reached Depth 1000");
            }

            Debug.Log("Starting Recursive Call");
            Recursive(start);

            TrimValidCells();


            _baked = true;
        }

        public bool IsCellValid(int x, int y)
        {
            int index = (y * (_baked ? BakedMapWidth : _image.width)) + x;
            if (HasValidCells && (index < 0 || index >= validCells.Length)) throw new IndexOutOfRangeException();
            return validCells[index];
        }

        public int GetIndexFromCoordinates(int x, int y, int width) => (y * width) + x;

        public void GetCoordinates(int index, out int2 output)
        {
            if (_image)
                GetCoordinatesFromArrayIndex(index, _image.width, _image.height, out output);
            else
                output = new(-1,-1);
        }

        public static int2 GetCoordinatesFromArrayIndex(int index, int width, int height)
        {
            GetCoordinatesFromArrayIndex(index, width, height, out int2 output);
            return output;
        }
        public static void GetCoordinatesFromArrayIndex(int index, int width, int height, out int2 output)
        {
            if (index > (width * height) - 1 || index < 0)
            {
                throw new IndexOutOfRangeException($"Index of {index} should be out of bounds for coordnate grid ({width} x {height})");
            }
            output = new(index % width, index/width);
        }

        // Trims extra cells on every side of the drawn image to save compute of extranious cells
        void TrimValidCells()
        {
            if(!HasValidCells) return;


            ValueRange2DInt range = ValueRange2DInt.InverseMaxed;

            // Find y min and max with bottom-to-top sweep
            for (int y = 0; y < _image.height; y++)
            {
                for (int x = 0; x < _image.width; x++)
                {
                    if (IsCellValid(x,y))
                    {
                        if (x < range.Min.x) range.Min.x = x;
                        if (x > range.Max.x) range.Max.x = x;
                        if (y < range.Min.y) range.Min.y = y;
                        if (y > range.Max.y) range.Max.y = y;
                    }
                }
            }

            int trimmedX = range.Max.x - range.Min.x + 1;
            int trimmedY = range.Max.y - range.Min.y + 1;
            bool[] trimmedSpace = new bool[trimmedY * trimmedX];

            // Debug.Log(trimmedX);
            // Debug.Log(trimmedY);
            // Debug.Log(range.ToString());
            
            // Step 3: Copy the block data over
            for (int x = 0; x < trimmedX; x++)
            {
                for (int y = 0; y < trimmedY; y++)
                {
                    // Debug.Log(x + " " + y);
                    trimmedSpace[GetIndexFromCoordinates(x, y, trimmedX)] = validCells[GetIndexFromCoordinates(range.Min.x + x, range.Min.y + y, _image.width)];
                }
            }
            
            BakedMapHeight = trimmedY;
            BakedMapWidth = trimmedX;
            validCells = trimmedSpace;

            BakedImageBounds = new(range.Min, new(trimmedX, trimmedY));

        }

    
        public string PrintValidImage()
        {
            var output = new StringBuilder("Current map:\n");
        
            if(validCells == null || validCells.Length == 0)
            {
                output.Append("None\n");
            }
            else
            {
                var line = new StringBuilder();
                for (int i = validCells.Length-1; i >= 0; i--)
                {
                    line.Insert(0, $"{(validCells[i] ? 'X' : '_')}");
                    if (i%_image.width == 0) {
                        line.Append("\n");
                        output.Append(line);
                        line.Clear();
                    }
                }
                output.Append("\n");
            }


            return output.ToString();
        }
    }


    
}
