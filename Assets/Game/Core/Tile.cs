namespace Game.Core {
  public enum TileType { A,B,C,D,E,F }
  public enum TileState { Normal, Frozen, Locked, Blocker, Line, Burst, ColorBomb, Pebble, Ice }
  public enum CellBlockerType { None, Moss, Vine, Pebble, Ice }
  public enum TileSpecial { None, LineHorizontal, LineVertical, Burst, ColorClear }
  public enum TileLock { None, Locked, Frozen }

  public struct BoardCell {
    public bool Active;
    public CellBlockerType Blocker;
    public int BlockerHp;
    public bool IsSpawnPoint;
    public bool IsDropExit;
  }

  public struct Tile {
    public TileType Type;
    public TileState State;
    public TileSpecial Special;
    // Fx 1 is a DewDrop/drop object carried by the tile layer.
    public byte Fx;
  }
}
