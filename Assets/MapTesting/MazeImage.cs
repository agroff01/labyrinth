using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomUtils;
using Microsoft.Unity.VisualStudio.Editor;
using SaintsField;
using SaintsField.Playa;
using Unity.Mathematics;
using UnityEngine;

namespace Labyrinth
{
    [CreateAssetMenu(fileName = "Maze Image Settings", menuName = "Scriptable Objects/Maze Image Settings")]
    public class MazeImageSettings : ScriptableObject
    {
        [Serializable]
        public enum ImagePresets
        {
            blocked = 0,
            open = 1,
            spawn = 2,
            end = 3
        }

        [Serializable]
        public class ColorLegend
        {
            
            public Color color = Color.white;
            public ImagePresets type;
        }

        [SerializeField] private Texture2D _image;
        public Texture2D Image => _image;

        public SaintsDictionary<ImagePresets, Color> colorCodes = new();

        Color SampleTexture(int row, int col)
        {
            return _image.GetPixel(row,col);
        }


        [Serializable]
        public struct ImageCell
        {
            public readonly int2 coord;
            public readonly ImagePresets type;

            public ImageCell(int x, int y, ImagePresets type)
            {
                coord = new(x,y);
                this.type = type;
            }
            public ImageCell(int2 coord, ImagePresets type)
            {
                this.coord = coord;
                this.type = type;
            }
        }
        public bool[] visitedCells = null;
        // [ShowInInspector] private List<ImageCell> _bakedCellData = null;
        // public IReadOnlyList<ImageCell> BakedCellData
        // {
        //     get
        //     {
        //         if (HasValidCells)
        //         {
        //             BakeCellData();
        //         }
        //         return _bakedCellData;
        //     }
        // }
        public bool HasValidCells => !(visitedCells == null || visitedCells.Length == 0);

        [Button]
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

            visitedCells = new bool[pixels.Length];

            bool IsValidDFSCell(int index)
            {
                return !pixels[index].Equals(colorCodes[ImagePresets.blocked]) &&
                       !visitedCells[index];
            }

            void Recursive(int index, int depth = 0)
            {
                if (IsValidDFSCell(index))
                {
                    // Mark validCells 
                    visitedCells[index] = true;
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
        }

        public bool IsCellValid(int x, int y)
        {
            int index = (y * _image.width) + x;
            if (visitedCells != null && index < 0 || index >= visitedCells.Length) throw new IndexOutOfRangeException();
            return visitedCells[index];
        }

        public int2 GetCoordinatesFromArrayIndex(int index, int width, int height)
        {
            GetCoordinatesFromArrayIndex(index, width, height, out int2 output);
            return output;
        }
        public void GetCoordinatesFromArrayIndex(int index, int width, int height, out int2 output)
        {
            if (index > (width * height) - 1 || index < 0)
            {
                throw new IndexOutOfRangeException($"Index of {index} should be out of bounds for coordnate grid ({width} x {height})");
            }
            output = new(index % width, index/width);
        }
    
        public string PrintValidImage()
        {
            var output = new StringBuilder("Current map:\n");
        
            if(visitedCells == null || visitedCells.Length == 0)
            {
                output.Append("None\n");
            }
            else
            {
                var line = new StringBuilder();
                for (int i = visitedCells.Length-1; i >= 0; i--)
                {
                    line.Insert(0, $"{(visitedCells[i] ? 'X' : '_')}");
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
