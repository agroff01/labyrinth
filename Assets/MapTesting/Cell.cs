
namespace Labyrinth
{
    public class Cell
    {
        readonly public int row;
        readonly public int col;

        public GridSpace gridSpaceType = GridSpace.empty;

        public Cell(int row, int col, GridSpace gridSpaceType = GridSpace.empty)
        {
            this.row = row;
            this.col = col;
            this.gridSpaceType = gridSpaceType;
        }
    }
    
}
