public class Tile
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Itembg { get; set; }
    public int Itemfg { get; set; }
    public int Itemdoor { get; set; }
    public Tile(int x, int y, int itembg, int itemfg, int itemdoor)
    {
        X = x;
        Y = y;
        Itembg = itembg;
        Itemfg = itemfg;
        Itemdoor = itemdoor;
    }

    public override string ToString()
    {
        return $"Tile(X={X}, Y={Y}, Itembg={Itembg}, Itemfg={Itemfg}, Itemdoor={Itemdoor})";
    }
}